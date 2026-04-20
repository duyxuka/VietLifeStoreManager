using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;

namespace VietlifeStore.ChucNang.ChatAIs
{
    [Authorize]
    public class ConversationAppService : ApplicationService
    {
        private readonly IRepository<Conversation, Guid> _convRepo;
        private readonly IRepository<ChatMessage, Guid> _msgRepo;
        private readonly IAIProviderService _ai;

        public ConversationAppService(
            IRepository<Conversation, Guid> convRepo,
            IRepository<ChatMessage, Guid> msgRepo,
            IAIProviderService ai)
        {
            _convRepo = convRepo;
            _msgRepo = msgRepo;
            _ai = ai;
        }

        // ─── List conversations ───────────────────────────────────────────────────

        public async Task<List<ConversationDto>> GetListAsync()
        {
            var userId = CurrentUser.GetId();
            var list = await _convRepo.GetListAsync(c => c.UserId == userId && !c.IsArchived);
            return list
                .OrderByDescending(c => c.LastModificationTime ?? c.CreationTime)
                .Select(c => new ConversationDto(c.Id, c.Title, c.Model, c.CreationTime))
                .ToList();
        }

        // ─── Create new conversation ──────────────────────────────────────────────

        public async Task<ConversationDto> CreateAsync(CreateConversationDto input)
        {
            var conv = new Conversation(GuidGenerator.Create(), CurrentUser.GetId(),
                input.Title ?? "New conversation", input.Model ?? ChatbotConsts.DefaultModel);

            if (!string.IsNullOrWhiteSpace(input.SystemPrompt))
                conv.SystemPrompt = input.SystemPrompt;

            await _convRepo.InsertAsync(conv, autoSave: true);
            return new ConversationDto(conv.Id, conv.Title, conv.Model, conv.CreationTime);
        }

        // ─── Get messages ─────────────────────────────────────────────────────────

        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid conversationId)
        {
            await EnsureOwnerAsync(conversationId);
            var msgs = await _msgRepo.GetListAsync(m => m.ConversationId == conversationId);
            return msgs
                .OrderBy(m => m.CreationTime)
                .Select(m => new ChatMessageDto(m.Id, m.Role, m.Content, m.CreationTime))
                .ToList();
        }

        // ─── Send message & stream response ──────────────────────────────────────

        /// <summary>
        /// Saves the user message, builds context, then streams AI response tokens.
        /// The SignalR hub calls this and forwards each chunk to the client.
        /// </summary>
        public async IAsyncEnumerable<StreamChunk> SendMessageStreamAsync(
            Guid conversationId,
            string userContent,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var conv = await EnsureOwnerAsync(conversationId);

            // 1. Persist user message
            var userMsg = new ChatMessage(GuidGenerator.Create(), conversationId,
                MessageRole.User, userContent);
            await _msgRepo.InsertAsync(userMsg, autoSave: true);

            // 2. Build context window (last N messages)
            var history = (await _msgRepo.GetListAsync(m => m.ConversationId == conversationId))
                .OrderBy(m => m.CreationTime)
                .TakeLast(ChatbotConsts.MaxContextMessages)
                .Select(m => new MessageDto(m.Role.ToString().ToLower(), m.Content))
                .ToList();

            var streamRequest = new StreamChatRequest(
                Model: conv.Model,
                SystemPrompt: conv.SystemPrompt ?? ChatbotConsts.DefaultSystemPrompt,
                Messages: history);

            // 3. Stream from OpenAI
            var fullContent = new StringBuilder();
            int? promptTokens = null;
            int? completionTokens = null;

            await foreach (var chunk in _ai.StreamChatAsync(streamRequest, cancellationToken))
            {
                if (!string.IsNullOrEmpty(chunk.Delta))
                    fullContent.Append(chunk.Delta);

                if (chunk.IsFinished)
                {
                    promptTokens = chunk.PromptTokens;
                    completionTokens = chunk.CompletionTokens;
                }

                yield return chunk;
            }

            // 4. Persist assistant message after stream completes
            var assistantMsg = new ChatMessage(GuidGenerator.Create(), conversationId,
                MessageRole.Assistant, fullContent.ToString())
            {
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TokensUsed = (promptTokens ?? 0) + (completionTokens ?? 0)
            };

            await _msgRepo.InsertAsync(assistantMsg, autoSave: true);

            // 5. Auto-set conversation title from first user message
            if (conv.Title == "New conversation")
            {
                conv.Title = userContent.Length > 60
                    ? userContent[..60] + "…"
                    : userContent;
                await _convRepo.UpdateAsync(conv, autoSave: true);
            }
        }

        // ─── Archive ──────────────────────────────────────────────────────────────

        public async Task ArchiveAsync(Guid conversationId)
        {
            var conv = await EnsureOwnerAsync(conversationId);
            conv.IsArchived = true;
            await _convRepo.UpdateAsync(conv, autoSave: true);
        }

        // ─── Helper ───────────────────────────────────────────────────────────────

        private async Task<Conversation> EnsureOwnerAsync(Guid conversationId)
        {
            var conv = await _convRepo.GetAsync(conversationId);
            if (conv.UserId != CurrentUser.GetId())
                throw new AbpAuthorizationException("Access denied.");
            return conv;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace VietlifeStore.ChucNang.ChatAIs
{
    public class ChatMessage : AuditedEntity<Guid>
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public MessageRole Role { get; set; }

        /// <summary>Text content of the message.</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>Total tokens consumed for this message (prompt + completion).</summary>
        public int? TokensUsed { get; set; }

        /// <summary>Prompt tokens (input) from OpenAI usage report.</summary>
        public int? PromptTokens { get; set; }

        /// <summary>Completion tokens (output) from OpenAI usage report.</summary>
        public int? CompletionTokens { get; set; }

        /// <summary>Estimated cost in USD based on model pricing.</summary>
        public decimal? EstimatedCostUsd { get; set; }

        protected ChatMessage() { }

        public ChatMessage(Guid id, Guid conversationId, MessageRole role, string content)
            : base(id)
        {
            ConversationId = conversationId;
            Role = role;
            Content = content;
        }
    }
    public enum MessageRole
    {
        System,
        User,
        Assistant
    }
}

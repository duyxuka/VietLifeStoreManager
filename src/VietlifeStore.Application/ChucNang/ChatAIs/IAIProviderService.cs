using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.ChatAIs
{
    public interface IAIProviderService
    {
        IAsyncEnumerable<StreamChunk> StreamChatAsync(
            StreamChatRequest request,
            CancellationToken cancellationToken = default);
    }

    public record StreamChatRequest(
        string Model,
        string SystemPrompt,
        IReadOnlyList<MessageDto> Messages,
        int MaxTokens = ChatbotConsts.MaxTokens
    );

    public record MessageDto(string Role, string Content);

    public record StreamChunk(
        string Delta,           // incremental text token
        bool IsFinished,        // true on the last chunk
        int? PromptTokens,      // only populated on last chunk
        int? CompletionTokens   // only populated on last chunk
    );
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.ChatAIs
{
    public record ConversationDto(
    Guid Id,
    string Title,
    string Model,
    DateTime CreatedAt);

    public class CreateConversationDto
    {
        public string? Title { get; set; }
        public string? Model { get; set; }
        public string? SystemPrompt { get; set; }
    }

    // ─── Message DTOs ─────────────────────────────────────────────────────────────

    public record ChatMessageDto(
        Guid Id,
        MessageRole Role,
        string Content,
        DateTime CreatedAt);

    public class SendMessageDto
    {
        public string Content { get; set; } = string.Empty;
    }
}

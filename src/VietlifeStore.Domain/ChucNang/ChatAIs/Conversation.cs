using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace VietlifeStore.ChucNang.ChatAIs
{
    public class Conversation : FullAuditedAggregateRoot<Guid>
    {
        public Guid? TenantId { get; set; }

        /// <summary>Owner of this conversation.</summary>
        public Guid UserId { get; set; }

        /// <summary>Display title, auto-generated from first message.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>OpenAI model used, e.g. "gpt-4o".</summary>
        public string Model { get; set; } = ChatbotConsts.DefaultModel;

        /// <summary>System prompt / persona override for this conversation.</summary>
        public string? SystemPrompt { get; set; }

        public bool IsArchived { get; set; } = false;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        protected Conversation() { }

        public Conversation(Guid id, Guid userId, string title, string model = ChatbotConsts.DefaultModel)
            : base(id)
        {
            UserId = userId;
            Title = title;
            Model = model;
        }
    }
}

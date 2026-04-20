using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.ChatAIs;

namespace VietlifeStore.Configurations.ChatAIs
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable(VietlifeStoreConsts.DbTablePrefix + "ChatMessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                   .HasMaxLength(int.MaxValue);

            builder.HasIndex(x => x.ConversationId);

            builder.HasOne(x => x.Conversation)
                   .WithMany(x => x.Messages)
                   .HasForeignKey(x => x.ConversationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

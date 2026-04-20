using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.ChatAIs;
using Volo.Abp.AspNetCore.Mvc;

namespace VietlifeStore.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : AbpControllerBase
    {
        private readonly ConversationAppService _service;

        public ChatController(ConversationAppService service)
        {
            _service = service;
        }

        [HttpGet("conversations")]
        public Task<List<ConversationDto>> GetConversationsAsync()
            => _service.GetListAsync();

        [HttpPost("conversations")]
        public Task<ConversationDto> CreateConversationAsync([FromBody] CreateConversationDto input)
            => _service.CreateAsync(input);

        [HttpGet("conversations/{id:guid}/messages")]
        public Task<List<ChatMessageDto>> GetMessagesAsync(Guid id)
            => _service.GetMessagesAsync(id);

        [HttpDelete("conversations/{id:guid}")]
        public Task ArchiveAsync(Guid id)
            => _service.ArchiveAsync(id);
    }
}

using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace VietlifeStore.ChucNang.ChatAIs
{
    [Authorize]
    public class ChatHub : AbpHub
    {
        private readonly ConversationAppService _convService;

        public ChatHub(ConversationAppService convService)
        {
            _convService = convService;
        }

        public async Task SendMessage(Guid conversationId, string content)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                await foreach (var chunk in _convService.SendMessageStreamAsync(
                    conversationId, content, cts.Token))
                {
                    if (chunk.IsFinished)
                    {
                        await Clients.Caller.SendCoreAsync("ReceiveFinished",
                            Array.Empty<object>(), cts.Token);
                    }
                    else if (!string.IsNullOrEmpty(chunk.Delta))
                    {
                        await Clients.Caller.SendCoreAsync("ReceiveChunk",
                            new object[] { chunk.Delta }, cts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                await Clients.Caller.SendCoreAsync("ReceiveError",
                    new object[] { "Request timed out." });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendCoreAsync("ReceiveError",
                    new object[] { ex.Message });
            }
        }

        public Task CancelStream() => Task.CompletedTask;
    }
}

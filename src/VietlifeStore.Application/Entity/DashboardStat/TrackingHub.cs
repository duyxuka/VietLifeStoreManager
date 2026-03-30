using Microsoft.AspNetCore.SignalR;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace VietlifeStore.Entity.DashboardStat
{
    public class TrackingHub : AbpHub
    {
        // Dùng static ConcurrentDictionary để đếm connection (không cần Redis)
        // Key = ConnectionId, Value = timestamp kết nối
        private static readonly ConcurrentDictionary<string, DateTime> _connections = new();

        public static int OnlineCount => _connections.Count;

        public override async Task OnConnectedAsync()
        {
            _connections[Context.ConnectionId] = DateTime.UtcNow;

            // Broadcast số người online mới cho tất cả client
            await Clients.All.SendAsync("OnlineCountUpdated", OnlineCount);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _connections.TryRemove(Context.ConnectionId, out _);

            await Clients.All.SendAsync("OnlineCountUpdated", OnlineCount);

            await base.OnDisconnectedAsync(exception);
        }
    }
}

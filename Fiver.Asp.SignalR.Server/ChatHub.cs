using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Fiver.Asp.SignalR.Server
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// 静态用户列表
        /// </summary>
        private static IList<string> userList = new List<string>();

        /// <summary>
        /// 用户的connectionID与用户名对照表
        /// </summary>
        private readonly static ConcurrentDictionary<string, string> _connections = new ConcurrentDictionary<string, string>();


        public override async Task OnConnectedAsync()
        {
            await Clients.All.InvokeAsync("Send", $"{Context.ConnectionId} joined");
            //连接就刷新登陆人列表
            await Clients.All.InvokeAsync("login", userList);
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.All.InvokeAsync("Send", $"{Context.ConnectionId} left");
        }

        public Task Send(string message)
        {
            return Clients.All.InvokeAsync("Send", $"{Context.ConnectionId}: {message}");
        }
        public Task SendExceptme(string message)
        {
            IReadOnlyList<string> exceptList = new List<string>(){ Context.ConnectionId };
            return Clients.AllExcept(exceptList).InvokeAsync("Send", $"{Context.ConnectionId}: {message}");
        }
        public Task SendToGroup(string groupName, string message)
        {
            return Clients.Group(groupName).InvokeAsync("Send", $"{Context.ConnectionId}@{groupName}: {message}");
        }
        public Task Join(string name)
        {
            if (!userList.Contains(name))
            {
                userList.Add(name);
                //这里便是将用户id和姓名联系起来
                _connections.TryAdd(name, Context.ConnectionId);
            }
            else
            {
                //每次登陆id会发生变化
                _connections[name] = Context.ConnectionId;
            }

            //新用户上线，服务器广播该用户名
            return Clients.All.InvokeAsync("login", userList);
        }
        public Task SendToUser(string friend,string message)
        {
            string sender=_connections.FirstOrDefault(f => f.Value == Context.ConnectionId).Key;
            return Clients.Client(_connections[friend]).InvokeAsync("Send","来自用户" + sender + " " + DateTime.Now.ToString("yyyy/MM/ddhh:mm:ss") + $"的消息推送！=={message}");
        }
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).InvokeAsync("Send", $"{Context.ConnectionId} joined {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).InvokeAsync("Send", $"{Context.ConnectionId} left {groupName}");
        }

        public Task Echo(string message)
        {
            return Clients.Client(Context.ConnectionId).InvokeAsync("Send", $"{Context.ConnectionId}: {message}");
        }
    }
}

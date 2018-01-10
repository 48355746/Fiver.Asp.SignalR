using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Fiver.Asp.SignalR.Server
{
    public class ReportsPublisher : Hub
    {
        public Task PublishReport(string reportName)
        {
            
            return Clients.All.InvokeAsync("OnReportPublished", reportName);
        }
        public override async Task OnConnectedAsync()
        {
            await Clients.All.InvokeAsync("Send", $"{Context.ConnectionId} joined");
        }

       
        public Task Send(string message)
        {
            return Clients.All.InvokeAsync("Send", $"{Context.ConnectionId}: {message}");
        }

        public Task SendToGroup(string groupName, string message)
        {
            return Clients.Group(groupName).InvokeAsync("Send", $"{Context.ConnectionId}@{groupName}: {message}");
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

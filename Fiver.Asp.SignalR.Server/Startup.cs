using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
//https://weblogs.asp.net/ricardoperes/signalr-in-asp-net-core
namespace Fiver.Asp.SignalR.Server
{
    public class Startup
    {
        public void ConfigureServices(
            IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("fiver",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod());
            });

            services.AddSignalR(); // <-- SignalR
        }

        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseCors("fiver");

            app.UseSignalR(routes =>  // <-- SignalR
            {
                routes.MapHub<ReportsPublisher>("reportsPublisher");
                routes.MapHub<ChatHub>("chat");
            });
            var timer = new Timer((x)=> {
                //后台直接触发发送消息
                var hub = app.ApplicationServices.GetService<IHubContext<ReportsPublisher>>();
               
                hub.Clients.All.InvokeAsync("OnReportPublished", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                
                //hub.Clients.Group("groupName").InvokeAsync("SendToGroup","", DateTime.Now);
            });
            timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(3));
           
        }
    }
}

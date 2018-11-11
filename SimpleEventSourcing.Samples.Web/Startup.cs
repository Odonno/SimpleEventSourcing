using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventSourcing.Samples.Web.Hubs;
using Swashbuckle.AspNetCore.Swagger;
using static SimpleEventSourcing.Samples.Web.Database.Configuration;
using static SimpleEventSourcing.Samples.Web.Program;

namespace SimpleEventSourcing.Samples.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "SimpleEventSourcing Example API",
                    Version = "v1"
                });
            });

            services.AddSignalR();
        }

        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env,
            IHubContext<CartHub> cartHubContext,
            IHubContext<OrderHub> orderHubContext,
            IHubContext<ItemHub> itemHubContext,
            IHubContext<EventHub> eventHubContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    ConfigFile = "webpack.config.js",
                    HotModuleReplacement = true,
                    ProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "app")
                });
            }

            // Handle database creation
            CreateEventsDatabase();
            CreateViewsDatabase();

            app.UseFileServer(new FileServerOptions()
            {
                EnableDirectoryBrowsing = env.IsDevelopment(),
                EnableDefaultFiles = true
            });

            app.UseSignalR(routes =>
            {
                routes.MapHub<CartHub>("/cart");
                routes.MapHub<OrderHub>("/order");
                routes.MapHub<ItemHub>("/item");
                routes.MapHub<EventHub>("/event");
            });

            app.UseMvc();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "SimpleEventSourcing Example API V1");
            });

            // Synchronize backend events (data mutations) with clients using signalr
            OrderEventView.ObserveEntityChange().Subscribe(async order =>
            {
                await orderHubContext.Clients.All.SendAsync("Sync", order);
            });

            CartEventView.ObserveEntityChange().Subscribe(async itemAndQuantity =>
            {
                await cartHubContext.Clients.All.SendAsync("Sync", itemAndQuantity);
            });

            ItemEventView.ObserveEntityChange().Subscribe(async item =>
            {
                await itemHubContext.Clients.All.SendAsync("Sync", item);
            });

            AppEventStore.ObserveEvent().Subscribe(async @event =>
            {
                await eventHubContext.Clients.All.SendAsync("Sync", @event);
            });
        }
    }
}

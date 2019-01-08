using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using Swashbuckle.AspNetCore.Swagger;
using Tagada.Swagger;
using SimpleEventSourcing.Samples.History;
using Newtonsoft.Json.Serialization;

namespace SimpleEventSourcing.Samples.Orders
{
    public class Program
    {
        public static IHostingEnvironment HostingEnvironment { get; private set; }

        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    HostingEnvironment = builderContext.HostingEnvironment;
                })
                .ConfigureServices(s => s.AddRouting())
                .ConfigureServices(s => 
                    s.AddMvc()
                        .AddJsonOptions(o =>
                            o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver
                            {
                                NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = true }
                            }
                        )
                )
                .ConfigureServices(s =>
                {
                    s.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new Info { Title = "Event History API", Version = "v1" });
                        c.GenerateTagadaSwaggerDoc();
                    });
                })
                .Configure(app =>
                {
                    if (HostingEnvironment.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }

                    app.Map("/api")
                        .Get("/events", GetAllEvents)
                        .AddSwagger()
                        .Use();

                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event History API V1");
                    });
                })
                .Build()
                .Run();
        }

        public static Func<GetEventsQuery, IEnumerable<AppEvent>> GetAllEvents = _ =>
        {
            return new List<AppEvent>(); // TODO : Get events from the EventStore
        };
    }

    public class EventDbo
    {
        public string Id { get; set; }
        public string EventName { get; set; }
        public string Data { get; set; }
        public string Metadata { get; set; }
    }

    public class GetEventsQuery { }
}
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Tagada.Swagger;
using Newtonsoft.Json.Serialization;
using SimpleEventSourcing.Samples.Providers;
using SimpleEventSourcing.CloudFirestore;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Samples.History
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
                .ConfigureServices(s =>
                {
                    s.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowCredentials()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    }));
                })
                .Configure(app =>
                {
                    if (HostingEnvironment.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }

                    app.UseCors("CorsPolicy");

                    var dataProvider = new CloudFirestoreProvider("event-sourcing-da233", "firebase.json");
                    var cloudFirestoreStreamProvider = new CloudFirestoreEventStreamProvider<StreamedEvent>(dataProvider.Database, new StreamedEventFirestoreConverter(), new EventStreamFirestoreConverter());

                    app.Map("/api")
                        .GetAsync("/streams", (GetStreamsQuery _) => cloudFirestoreStreamProvider.GetAllStreamsAsync())
                        .GetAsync("/streams/{streamId}/events", (GetStreamEventsQuery query) => GetStreamEventsAsync(cloudFirestoreStreamProvider, query.StreamId))
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

        private static async Task<IEnumerable<StreamedEvent>> GetStreamEventsAsync(CloudFirestoreEventStreamProvider<StreamedEvent> cloudFirestoreStreamProvider, string streamId)
        {
            var stream = await cloudFirestoreStreamProvider.GetStreamAsync(streamId);
            return await stream.GetAllEventsAsync();
        }
    }

    public class GetStreamsQuery { }

    public class GetStreamEventsQuery
    {
        public string StreamId { get; set; }
    }
}
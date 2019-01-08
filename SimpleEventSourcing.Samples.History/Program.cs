using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Tagada.Swagger;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Dynamic;
using Dapper;
using Newtonsoft.Json.Serialization;
using SimpleEventSourcing.Samples.Events;

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
                .Configure(app =>
                {
                    if (HostingEnvironment.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }

                    HandleDatabaseCreation();

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

        private static SqliteConnection GetDatabaseConnection()
        {
            var connection = new SqliteConnection("Data Source=../EventsDatabase.db");
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        private static void HandleDatabaseCreation()
        {
            using (var connection = GetDatabaseConnection())
            {
                connection.Execute(
                    @"
                    CREATE TABLE IF NOT EXISTS [Event] (
                        [Id] VARCHAR(36) NOT NULL PRIMARY KEY,
                        [Number] INTEGER NOT NULL,
                        [EventName] DATETIME NOT NULL,
                        [Data] INTEGER NOT NULL,
                        [Metadata] INTEGER NOT NULL
                    );
                    "
                );
            }
        }

        public static Func<GetEventsQuery, IEnumerable<AppEvent>> GetAllEvents = _ =>
        {
            using (var connection = GetDatabaseConnection())
            {
                return connection
                    .Query<EventDbo>("SELECT * FROM [Event] ORDER BY [Id] DESC")
                    .Select(eventDbo =>
                    {
                        return new AppEvent
                        {
                            Id = eventDbo.Id,
                            Number = eventDbo.Number,
                            EventName = eventDbo.EventName,
                            Data = JsonConvert.DeserializeObject<ExpandoObject>(eventDbo.Data),
                            Metadata = JsonConvert.DeserializeObject<ExpandoObject>(eventDbo.Metadata)
                        };
                    })
                    .ToList();
            }
        };
    }

    public class GetEventsQuery { }
}
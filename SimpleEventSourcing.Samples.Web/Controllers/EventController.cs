using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static SimpleEventSourcing.Samples.Web.DatabaseConfiguration;

namespace SimpleEventSourcing.Samples.Web.Controllers
{
    [Route("api/[controller]")]
    public class EventController : Controller
    {
        [HttpGet("all")]
        public IEnumerable<EventInfo> GetAll()
        {
            using (var connection = GetEventsDatabaseConnection())
            {
                return connection
                    .Query<EventInfo>("SELECT * FROM [Event] ORDER BY [Id] DESC");
            }
        }
    }
}

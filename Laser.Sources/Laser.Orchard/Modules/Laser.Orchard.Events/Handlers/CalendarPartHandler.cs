using Laser.Orchard.Events.Models;
using Laser.Orchard.Events.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System.Linq;
using System.Web.Routing;

namespace Laser.Orchard.Events.Handlers
{
    public class CalendarPartHandler : ContentHandler
    {
        private readonly IEventsService _eventsService;
        private readonly RequestContext _requestContext;

        public CalendarPartHandler(IRepository<CalendarPartRecord> repository, IEventsService eventsService, RequestContext requestContext)
        {
            // Tell this handler to use CalendarPartRecord for storage.
            Filters.Add(StorageFilter.For(repository));

            _eventsService = eventsService;
            _requestContext = requestContext;
        }

        protected override void Loaded(LoadContentContext context)
        {
            base.Loaded(context);

            if (_requestContext.HttpContext.Handler != null)
            {
                if (_requestContext.HttpContext.Request.RequestContext != null) //non-routed requests
               {
                    string usedController = _requestContext.HttpContext.Request.RequestContext.RouteData.Values["Controller"].ToString();

                    if (usedController == "json") {
                        if (context.ContentItem.Parts.SingleOrDefault(x => x.PartDefinition.Name == "CalendarPart") == null)
                            return;

                        var calendarPart = (CalendarPart)context.ContentItem.Parts.SingleOrDefault(x => x.PartDefinition.Name == "CalendarPart");

                        calendarPart._eventList.Loader(x => {
                                return _eventsService.GetAggregatedList(calendarPart);
                            });
                    }
                }
            }
        }
    }
}
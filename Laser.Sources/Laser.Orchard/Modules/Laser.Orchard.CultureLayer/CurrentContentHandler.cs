using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.CultureLayer
{
    public class CurrentContentHandler : ContentHandler
    {
        private readonly IWorkContextAccessor _workContextAccessor;

        public CurrentContentHandler(IWorkContextAccessor workContextAccessor)
        {
            _workContextAccessor = workContextAccessor;
        }

        protected override void BuildDisplayShape(BuildDisplayContext context)
        {
            var workContext = _workContextAccessor.GetContext();
            var contentItems = workContext.GetState<List<IContent>>("ContentItems");
            if (contentItems == null)
            {
                workContext.SetState("ContentItems", contentItems = new List<IContent>());
            }

            contentItems.Add(context.ContentItem);
        }
    }
}
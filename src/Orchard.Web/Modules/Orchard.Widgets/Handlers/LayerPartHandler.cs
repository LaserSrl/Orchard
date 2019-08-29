using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Handlers {
    public class LayerPartHandler : ContentHandler {
        public LayerPartHandler(IRepository<LayerPartRecord> layersRepository) {
            Filters.Add(StorageFilter.For(layersRepository));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<LayerPart>();

            if (part != null) {
                context.Metadata.Identity.Add("Layer.LayerName", part.Name);

                context.Metadata.CreateRouteValues = new RouteValueDictionary {
                    {"Area", "Orchard.Widgets"},
                    {"Controller", "Admin"},
                    {"Action", "AddLayer"}
                };
                context.Metadata.EditorRouteValues = new RouteValueDictionary {
                    {"Area", "Orchard.Widgets"},
                    {"Controller", "Admin"},
                    {"Action", "EditWidget"},
                    {"id", part.Id}
                };
                // remove goes through edit layer...
                context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                    {"Area", "Orchard.Widgets"},
                    {"Controller", "Admin"},
                    {"Action", "EditLayer"},
                    {"Id", part.Id}
                };
            }
        }
    }
}
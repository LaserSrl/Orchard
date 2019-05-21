using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Users.Models;
using System.Web.Routing;

namespace Orchard.Users.Handlers {
    public class UserPartHandler : ContentHandler {
        public UserPartHandler(IRepository<UserPartRecord> repository) {
            Filters.Add(new ActivatingFilter<UserPart>("User"));
            Filters.Add(StorageFilter.For(repository));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<UserPart>();

            if (part != null) {
                context.Metadata.Identity.Add("User.UserName", part.UserName);
                context.Metadata.DisplayText = part.UserName;

                context.Metadata.CreateRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Users"},
                {"Controller", "Admin"},
                {"Action", "Create"}
            };
                context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Users"},
                {"Controller", "Admin"},
                {"Action", "Edit"},
                {"id", context.ContentItem.Id}
            };
                context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Users"},
                {"Controller", "Admin"},
                {"Action", "Remove"},
                {"id", context.ContentItem.Id}
            };
            }
        }
    }
}
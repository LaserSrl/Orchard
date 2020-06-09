using Orchard.Blogs.Models;
using Orchard.Blogs.Extensions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;

namespace Orchard.Blogs.Drivers {
    public class BlogPostPartDriver : ContentPartDriver<BlogPostPart> {
        private readonly IFeedManager _feedManager;
        private readonly IContentManager _contentManager;

        public BlogPostPartDriver(IFeedManager feedManager, IContentManager contentManager) {
            _feedManager = feedManager;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(BlogPostPart part, string displayType, dynamic shapeHelper) {
            if (displayType.StartsWith("Detail")) {
                var blogTitle = _contentManager.GetItemMetadata(part.BlogPart).DisplayText;
                _feedManager.Register(part.BlogPart, blogTitle);
            }

            return null;
        }
        /* elena : sovrascrittura dell'exporting per aggiornare il container
        protected override void Exporting(BlogPostPart part, ExportContentContext context) {
            base.Exporting(part, context);

            if (part.ContentItem.As<CommonPart>()?.Container == null) {
                var contentCommon = _contentManager.Get(part.As<CommonPart>().Record.Container.Id, VersionOptions.Latest);
                var containerIdentity = _contentManager.GetItemMetadata(contentCommon).Identity;
                context.Element(part.As<CommonPart>().PartDefinition.Name).SetAttributeValue("Container", containerIdentity.ToString());
            }
        }
        */
    }
}
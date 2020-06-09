using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.UI.Notify;

namespace Orchard.Blogs.BlogsLocalizationExtensions.Handlers {
    [OrchardFeature("Orchard.Blogs.LocalizationExtensions")]
    public class BlogPostPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IAutorouteService _routeService;
        private readonly ILocalizationService _localizationService;

        public BlogPostPartHandler(RequestContext requestContext, IContentManager contentManager, IAutorouteService routeService, ILocalizationService localizationService, INotifier notifier) {
            _contentManager = contentManager;
            _routeService = routeService;
            _localizationService = localizationService;
            Notifier = notifier;
            T = NullLocalizer.Instance;
            //move posts when created, updated or published
            //changed OnCreating and OnUpdating in OnCreated and OnUpdated so LocalizationPart is already populated
            OnCreated<BlogPostPart>((context, part) => MigrateBlogPost(context.ContentItem));
            OnUpdated<BlogPostPart>((context, part) => MigrateBlogPost(context.ContentItem));
            OnPublishing<BlogPostPart>((context, part) => MigrateBlogPost(context.ContentItem));
        }

        public INotifier Notifier { get; set; }

        public Localizer T { get; set; }

        //This Method checks the blog post's culture and it's parent blog's culture and moves it to the correct blog if they aren't equal.
        private void MigrateBlogPost(ContentItem blogPost) {
            if (!blogPost.Has<LocalizationPart>() || !blogPost.Has<BlogPostPart>()) {
                return;
            }
            //bolgPost just cloned for translation, never saved
            if (blogPost.As<CommonPart>().Container == null) {
                return;
            }
            var blog = _contentManager.Get(blogPost.As<CommonPart>().Container.Id);
            if (!blog.Has<LocalizationPart>() || blog.As<LocalizationPart>().Culture == null) {
                return;
            }

            //get our 2 cultures for comparison
            var blogCulture = blog.As<LocalizationPart>().Culture;
            var blogPostCulture = blogPost.As<LocalizationPart>().Culture;

            //if the post is a different culture than the parent blog change the post's parent blog to the right localization...
            if (blogPostCulture != null && (blogPostCulture.Id != blogCulture.Id)) {
                //Get the id of the current blog
                var blogids = new HashSet<int> { blog.As<BlogPart>().ContentItem.Id };

                //seek for same culture blog
                var realBlog = _localizationService.GetLocalizations(blog).SingleOrDefault(w => w.As<LocalizationPart>().Culture == blogPostCulture);
                if (realBlog.Has<LocalizationPart>() && realBlog.As<LocalizationPart>().Culture.Id == blogPostCulture.Id) {
                    blogPost.As<ICommonPart>().Container = realBlog;
                    if (blogPost.Has<AutoroutePart>()) {
                        _routeService.RemoveAliases(blogPost.As<AutoroutePart>());
                        blogPost.As<AutoroutePart>().DisplayAlias = _routeService.GenerateAlias(blogPost.As<AutoroutePart>());
                        _routeService.PublishAlias(blogPost.As<AutoroutePart>());
                    }
                    Notifier.Information(T("Your Post has been moved under the \"{0}\" Blog", realBlog.As<TitlePart>().Title));
                    return;
                }

                return;
            }
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var blogPost = context.ContentItem;
            if (blogPost.As<BlogPostPart>() == null) {
                return;
            }

            if (blogPost.As<LocalizationPart>() == null) {
                return;
            }

            int blogId = 0;
            var masterIdentity = blogPost.As<LocalizationPart>().MasterContentItem;
            if (masterIdentity != null) {
                var commonPart = masterIdentity.As<CommonPart>();
                if (commonPart != null &&
                    commonPart.Record.Container != null) {
                    blogId = commonPart.Record.Container.Id;
                }
            }

            if (blogId == 0) {
                context.Metadata.CreateRouteValues = new RouteValueDictionary {
                    {"Area", "Orchard.Blogs"},
                    {"Controller", "BlogPostAdmin"},
                    {"Action", "CreateWithoutBlog"}
                };

                return;
            }

            context.Metadata.CreateRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogPostAdmin"},
                {"Action", "Create"},
                {"blogId", blogId}
            };
            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogPostAdmin"},
                {"Action", "Edit"},
                {"postId", context.ContentItem.Id},
                {"blogId", blogId}
            };
            context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogPostAdmin"},
                {"Action", "Delete"},
                {"postId", context.ContentItem.Id},
                {"blogId", blogId}
            };
        }
    }
}
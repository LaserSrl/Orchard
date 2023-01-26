﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.Mvc.Routes;
using Orchard.UI.Zones;

namespace Orchard.ContentManagement {
    public class DefaultContentDisplay : IContentDisplay {
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IShapeFactory _shapeFactory;
        private readonly Lazy<IShapeTableLocator> _shapeTableLocator;

        private readonly RequestContext _requestContext;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ShellSettings _shellSettings;
        private readonly UrlPrefix _urlPrefix;

        public DefaultContentDisplay(
            Lazy<IEnumerable<IContentHandler>> handlers,
            IShapeFactory shapeFactory,
            Lazy<IShapeTableLocator> shapeTableLocator,
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider,
            IWorkContextAccessor workContextAccessor,
            ShellSettings shellSettings) {
            _handlers = handlers;
            _shapeFactory = shapeFactory;
            _shapeTableLocator = shapeTableLocator;
            _requestContext = requestContext;
            _virtualPathProvider = virtualPathProvider;
            _workContextAccessor = workContextAccessor;
            _shellSettings = shellSettings;
            if (!string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                _urlPrefix = new UrlPrefix(_shellSettings.RequestUrlPrefix);
            Logger = NullLogger.Instance;

        }

        public ILogger Logger { get; set; }

        public dynamic BuildDisplay(IContent content, string displayType, string groupId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype;
            var actualDisplayType = string.IsNullOrWhiteSpace(displayType) ? "Detail" : displayType;

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;
            itemShape.Metadata.DisplayType = actualDisplayType;

            var context = new BuildDisplayContext(itemShape, content, actualDisplayType, groupId, _shapeFactory);
            var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);
            context.Layout = workContext.Layout;

            BindPlacement(context, actualDisplayType, stereotype);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            _handlers.Value.Invoke(handler => {
                sw = null;
                sw = new System.Diagnostics.Stopwatch();
                //System.IO.File.AppendAllText(
                //    @"D:\piovanellim\Laser.Orchard\Orchard\src\Orchard.Web\App_Data\Logs\perf-display.log",
                //    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t\t\t ---- \t\t\t" + "BuildDisplay For " + handler.GetType().ToString() + " START" + "\r\n");
                sw.Start();
                handler.BuildDisplay(context);
                sw.Stop();
                System.IO.File.AppendAllText(
                    @"D:\piovanellim\Laser.Orchard\Orchard\src\Orchard.Web\App_Data\Logs\perf-display.log",
                    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t\t\t "
                    + sw.Elapsed.TotalMilliseconds.ToString() + "\t\t\t"
                    + $"BuildDisplay {context.DisplayType} For content {context.Content.Id} "
                    + handler.GetType().ToString() + "\t " + "\r\n");
            }, Logger);
            return context.Shape;
        }

        public dynamic BuildEditor(IContent content, string groupId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            // adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + content.ContentItem.ContentType);

            var context = new BuildEditorContext(itemShape, content, groupId, _shapeFactory);
            var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);
            context.Layout = workContext.Layout;
            BindPlacement(context, null, stereotype);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            _handlers.Value.Invoke(handler => {
                sw = null;
                sw = new System.Diagnostics.Stopwatch();
                //System.IO.File.AppendAllText(
                //    @"D:\piovanellim\Laser.Orchard\Orchard\src\Orchard.Web\App_Data\Logs\perf-editor.log",
                //    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t\t\t ---- \t\t\t" + "BuildEditor For " + handler.GetType().ToString() + " START" + "\r\n");
                sw.Start();
                handler.BuildEditor(context);
                sw.Stop();
                System.IO.File.AppendAllText(
                    @"D:\piovanellim\Laser.Orchard\Orchard\src\Orchard.Web\App_Data\Logs\perf-editor.log",
                    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t\t\t "
                    + sw.Elapsed.TotalMilliseconds.ToString() + "\t\t\t"
                    + $"BuildEditor For {context.Content.Id} "
                    + handler.GetType().ToString() + "\t " + "\r\n");
            }, Logger);


            return context.Shape;
        }

        public dynamic UpdateEditor(IContent content, IUpdateModel updater, string groupInfoId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);

            var theme = workContext.CurrentTheme;
            var shapeTable = _shapeTableLocator.Value.Lookup(theme.Id);

            // adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + content.ContentItem.ContentType);

            var context = new UpdateEditorContext(itemShape, content, updater, groupInfoId, _shapeFactory, shapeTable, GetPath());
            context.Layout = workContext.Layout;
            BindPlacement(context, null, stereotype);

            _handlers.Value.Invoke(handler => handler.UpdateEditor(context), Logger);

            return context.Shape;
        }

        private dynamic CreateItemShape(string actualShapeType) {
            return _shapeFactory.Create(actualShapeType, Arguments.Empty(), () => new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty())));
        }

        private void BindPlacement(BuildShapeContext context, string displayType, string stereotype) {
            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {

                var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);

                var theme = workContext.CurrentTheme;
                var shapeTable = _shapeTableLocator.Value.Lookup(theme.Id);

                ShapeDescriptor descriptor;
                if (shapeTable.Descriptors.TryGetValue(partShapeType, out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        Content = context.ContentItem,
                        ContentType = context.ContentItem.ContentType,
                        Stereotype = stereotype,
                        DisplayType = displayType,
                        Differentiator = differentiator,
                        Path = GetPath()
                    };

                    // define which location should be used if none placement is hit
                    descriptor.DefaultPlacement = defaultLocation;

                    var placement = descriptor.Placement(placementContext);
                    if (placement != null) {
                        placement.Source = placementContext.Source;
                        return placement;
                    }
                }

                return new PlacementInfo {
                    Location = defaultLocation,
                    Source = String.Empty
                };
            };
        }

        /// <summary>
        /// Gets the current app-relative path, i.e. ~/my-blog/foo.
        /// </summary>
        private string GetPath() {
            var appRelativePath = _virtualPathProvider.ToAppRelative(_requestContext.HttpContext.Request.Path);
            // If the tenant has a prefix, we strip the tenant prefix away.
            if (_urlPrefix != null)
                appRelativePath = _urlPrefix.RemoveLeadingSegments(appRelativePath);

            return VirtualPathUtility.AppendTrailingSlash(appRelativePath);
        }
    }
}

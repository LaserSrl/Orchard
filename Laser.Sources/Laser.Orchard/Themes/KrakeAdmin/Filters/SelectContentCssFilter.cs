using System;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Themes.Services;
using Orchard.UI.Admin;
using Orchard.UI.Resources;
using System.Web.Routing;
using System.Text.RegularExpressions;
using System.IO;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Environment.Features;
using System.Collections.Generic;
using Orchard.FileSystems.VirtualPath;
using Orchard.Caching;
using Orchard.Themes;
using Orchard.Environment.Extensions.Models;
using System.Linq;

namespace KrakeAdmin.Filters {
    public class SelectContentCssFilter : FilterProvider, IResultFilter {

        private readonly IResourceManager _resourceManager;
        private readonly IFeatureManager _featureManager;

        private TextWriter _originalWriter;
        private StringWriter _tempWriter;

        public SelectContentCssFilter(IResourceManager resourceManager, IOrchardServices orchardServices,
            IFeatureManager featureManager) {

            _resourceManager = resourceManager;
            _featureManager = featureManager;

        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
            if (isAdminKrakePicker(filterContext.RouteData)) {
                CaptureResponse(filterContext);
            }
        }


        public void OnResultExecuting(ResultExecutingContext filterContext) {                 
            if (isAdminKrakePicker(filterContext.RouteData)) {
                _resourceManager.Require("stylesheet", ResourceManifest.Site).AtHead();
                _resourceManager.Require("stylesheet", ResourceManifest.KrakeAdmin).AtHead();
                _resourceManager.Require("stylesheet", ResourceManifest.Krake).AtHead();
                _resourceManager.Require("stylesheet", ResourceManifest.KrakeNavigation).AtHead();
                _resourceManager.Require("stylesheet", ResourceManifest.Bootstrap).AtHead();

                _originalWriter = filterContext.HttpContext.Response.Output;
                _tempWriter = new StringWriterWithEncoding(_originalWriter.Encoding, _originalWriter.FormatProvider);
                filterContext.HttpContext.Response.Output = _tempWriter;

            }
        }

        public bool isAdminKrakePicker(RouteData route) {
            var featureTheme = _featureManager
                .GetAvailableFeatures()
                .FirstOrDefault(f => f.Id.Equals("KrakeAdmin", StringComparison.OrdinalIgnoreCase));
            var isKrakeTheme = featureTheme != null;
            bool isActionIndex = route.Values.ContainsKey("action") && String.Equals(route.Values["action"].ToString(), "index", StringComparison.OrdinalIgnoreCase);

            //advanced search
            bool isPickerContent = route.Values.ContainsKey("area") && route.Values["area"].Equals("Orchard.ContentPicker");
            bool isAdmin = route.Values.ContainsKey("controller") && route.Values["controller"].Equals("admin");
            
            //simple search
            bool isSearchContent = route.Values.ContainsKey("area") && route.Values["area"].Equals("Orchard.Search");
            bool isContentPiker = route.Values.ContainsKey("controller") && route.Values["controller"].Equals("ContentPicker");

            return isActionIndex && isKrakeTheme && (isAdmin && isPickerContent || isSearchContent && isContentPiker);
        }

        private void CaptureResponse(ControllerContext filterContext) {
            filterContext.HttpContext.Response.Output = _originalWriter;

            string capturedText = _tempWriter.ToString();

            var regex = new Regex("(<[^>]+site.css[^>]+>)");
            Match firstOcc = regex.Match(capturedText);
            var offset = firstOcc.Index + firstOcc.Length;
            capturedText = regex.Replace(capturedText, "", 1, offset);
       
            filterContext.HttpContext.Response.Write(capturedText);
        }
    }
}
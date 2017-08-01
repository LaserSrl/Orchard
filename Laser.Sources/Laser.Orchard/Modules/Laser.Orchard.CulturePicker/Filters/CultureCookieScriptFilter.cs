using Orchard;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.CulturePicker.Filters {
    public class CultureCookieScriptFilter : FilterProvider, IResultFilter {
        private readonly WorkContext _workContext;
        private readonly dynamic _shapeFactory;
        private readonly IResourceManager _resourceManager;

        public CultureCookieScriptFilter(
            WorkContext workContext,
            IShapeFactory shapeFactory,
            IResourceManager resourceManager) {

            _workContext = workContext;
            _shapeFactory = shapeFactory;
            _resourceManager = resourceManager;
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            _resourceManager.Require("script", "culture.cookie").AtHead();
            var tail = _workContext.Layout.Tail;
            tail.Add(_shapeFactory.CultureCookieScripts(Culture: _workContext.CurrentCulture));
        }
    }
}
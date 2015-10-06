using Orchard.Mvc.Filters;
using System;
using System.IO;
using System.Web.Mvc;

namespace Laser.Orchard.Accessibility.Filters
{
    public class HighContrastFilter : FilterProvider, IActionFilter, IResultFilter
    {
        private TextWriter _originalWriter;
        private Action<ControllerContext> _completeResponse;
        private StringWriter _tempWriter;

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _originalWriter = filterContext.HttpContext.Response.Output;
            _tempWriter = new StringWriterWithEncoding(_originalWriter.Encoding, _originalWriter.FormatProvider);
            filterContext.HttpContext.Response.Output = _tempWriter;
            _completeResponse = CaptureResponse;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (_completeResponse != null)
            {
                _completeResponse(filterContext);
            }
        }

        private void CaptureResponse(ControllerContext filterContext)
        {
            filterContext.HttpContext.Response.Output = _originalWriter;

            string capturedText = _tempWriter.ToString();
            _tempWriter.Dispose();

            // se richiesto, modifica l'output per avere "high contrast" sostituendo un file css
            if (new Utils().getTenantCookieValue(Utils.AccessibilityCookieName, filterContext.HttpContext.Request) == Utils.AccessibilityHighContrast)
            {
                // TODO: nella riga seguente mettere il vero nome del css per high contrast
                capturedText = capturedText.Replace("Themes/AnfiteatroMorenico/Styles/style.css\"", "Themes/AnfiteatroMorenico/Styles/highstyle.css\"");
            }
            filterContext.HttpContext.Response.Write(capturedText);
        }
    }
}
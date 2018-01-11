using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Mvc.Extensions;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceLinkFromSettingsProvider : INonceLinkProvider {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICommonsServices _commonsServices;
        public NonceLinkFromSettingsProvider(
            IWorkContextAccessor workContextAccessor,
            ICommonsServices commonsServices) {

            _workContextAccessor = workContextAccessor;
            _commonsServices = commonsServices;
        }

        public string FormatURI(string nonce) {
            return FormatURI(nonce, null);
        }
        public string FormatURI(string nonce,FlowType? flow) {
            if (flow == FlowType.App) {
                  var urlHelper = _commonsServices.GetUrlHelper();
                return urlHelper.MakeAbsolute(urlHelper.Action("GetByURL", "NonceAppCamouflage", new {  area = "Laser.Orchard.MultiStepAuthentication" ,n= nonce }), _workContextAccessor.GetContext().CurrentSite.BaseUrl);
            }
            else
                return string.Format(GetFormat(), nonce);
        }
        public string GetSchema() {
            var format = GetFormat();
            if (format.Contains(@"://")) {
                return format.Substring(0, format.IndexOf(@"://"));
            } else {
                return format;
            }
        }

        private string GetFormat() {
            return _workContextAccessor.GetContext().CurrentSite.As<NonceLoginSettingsPart>().LoginLinkFormat;
        }
    }
}
using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceLinkFromSettingsProvider : INonceLinkProvider {

        private readonly IWorkContextAccessor _workContextAccessor;

        public NonceLinkFromSettingsProvider(
            IWorkContextAccessor workContextAccessor) {

            _workContextAccessor = workContextAccessor;
        }

        public string FormatURI(string nonce) {
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
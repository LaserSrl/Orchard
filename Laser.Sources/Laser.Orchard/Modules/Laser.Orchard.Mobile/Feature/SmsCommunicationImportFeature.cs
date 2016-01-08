﻿using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Mobile.Feature {

    [OrchardFeature("Laser.Orchard.SmsCommunicationImport")]
    public class SmsCommunicationImportFeature : IFeatureEventHandler {

        private readonly ISmsServices _smsServices;


        public SmsCommunicationImportFeature(ISmsServices smsServices) {

            _smsServices = smsServices;
   
        }

        public void Disabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Disabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Enabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            if (feature.Descriptor.Id == "Laser.Orchard.SmsCommunicationImport") {
                _smsServices.Synchronize();
            }
        }

        public void Enabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Installed(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Installing(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Uninstalled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Uninstalling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }
    }
}
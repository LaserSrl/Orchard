using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Drivers {
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperDriver : ContentPartDriver<KeywordHelperPart> {

        public KeywordHelperDriver() {

        }

        /// <summary>
        /// GET Editor
        /// </summary>
        protected override DriverResult Editor(KeywordHelperPart part, dynamic shapeHelper) {
            return ContentShape("Parts_KeywordHelper_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/KeywordHelper",
                    Model: new KeywordHelperPartViewModel(part),
                    Prefix: Prefix));
        }

        /// <summary>
        /// POST editor
        /// </summary>
        protected override DriverResult Editor(KeywordHelperPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vm = new KeywordHelperPartViewModel();
            if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                part.Keywords = vm.ListToString();
            }
            return Editor(part,shapeHelper);
        }
    }
}
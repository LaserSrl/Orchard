using System;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.UI.Resources;
using Laser.Orchard.SEO.Models;
using System.Data.SqlTypes;
using Orchard.Localization.Services;
using Laser.Orchard.StartupConfig.Localization;
using Laser.Orchard.SEO.ViewModels;
using Laser.Orchard.SEO.Services;


namespace Laser.Orchard.SEO.Drivers {


    public class SeoDriver : ContentPartDriver<SeoPart> {


        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ISEOServices _seoServices;

        public SeoDriver(IWorkContextAccessor workContextAccessor, ISEOServices seoServices) {
            _workContextAccessor = workContextAccessor;
            _seoServices = seoServices;
        }


        /// <summary>
        /// GET Display.
        /// </summary>
        protected override DriverResult Display(SeoPart part, string displayType, dynamic shapeHelper) {

            if (displayType != "Detail")
                return null;

            var resourceManager = _workContextAccessor.GetContext().Resolve<IResourceManager>();

            if (!string.IsNullOrWhiteSpace(part.Description)) {
                resourceManager.SetMeta(new MetaEntry {
                    Name = "description",
                    Content = part.Description
                });
            }

            if (!string.IsNullOrWhiteSpace(part.Keywords)) {
                resourceManager.SetMeta(new MetaEntry {
                    Name = "keywords",
                    Content = part.Keywords
                });
            }

            string metaRobots = "";
            metaRobots += part.RobotsNoIndex ? "noindex," : "";
            metaRobots += part.RobotsNoFollow ? "nofollow," : "";
            metaRobots += part.RobotsNoSnippet ? "nosnippet," : "";
            metaRobots += part.RobotsNoOdp ? "noodp," : "";
            metaRobots += part.RobotsNoArchive ? "noarchive," : "";
            metaRobots += part.RobotsUnavailableAfter ? "unavailable_after:" + part.RobotsUnavailableAfterDate.Value.ToUniversalTime().ToString("r") + "," : ""; //date in rfc850 format
            metaRobots += part.RobotsNoImageIndex ? "noimageindex," : "";
            if (!string.IsNullOrWhiteSpace(metaRobots)) {
                resourceManager.SetMeta(new MetaEntry {
                    Name = "robots",
                    Content = metaRobots.Substring(0, metaRobots.Length - 1) //remove trailing comma
                });
            }

            string metaGoogle = "";
            metaGoogle += part.GoogleNoSiteLinkSearchBox ? "nositelinkssearchbox," : "";
            metaGoogle += part.GoogleNoTranslate ? "notranslate," : "";
            if (!string.IsNullOrWhiteSpace(metaGoogle)) {
                resourceManager.SetMeta(new MetaEntry {
                    Name = "google",
                    Content = metaGoogle.Substring(0, metaGoogle.Length - 1)
                });
            }

            if (!string.IsNullOrWhiteSpace(part.TitleOverride)) {
                return ContentShape("Parts_SEO", () => shapeHelper.Parts_SEO(
                  TitleOverride: part.TitleOverride
                ));
            }

            return null;
        }


        /// <summary>
        /// GET Editor.
        /// </summary>
        protected override DriverResult Editor(SeoPart part, dynamic shapeHelper) {
            return ContentShape("Parts_SEO_Edit",
                                () => shapeHelper.EditorTemplate(
                                  TemplateName: "Parts/SEO",
                                  Model: new SeoPartViewModel(part, _seoServices), //use a viewmodel to show times in local base, while keeping UTC on the server side
                                  Prefix: Prefix));
        }


        /// <summary>
        /// POST Editor.
        /// </summary>
        protected override DriverResult Editor(SeoPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vm = new SeoPartViewModel(_seoServices);
            updater.TryUpdateModel(vm, Prefix, null, null);
            vm.UpdatePart(part);
            return Editor(part, shapeHelper);
        }


    }
}
using System;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.UI.Resources;
using Laser.Orchard.SEO.Models;
using System.Data.SqlTypes;
using Orchard.Localization.Services;
using Laser.Orchard.StartupConfig.Localization;


namespace Laser.Orchard.SEO.Drivers {


    public class SeoDriver : ContentPartDriver<SeoPart> {


        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IDateServices _dateServices;
        private readonly IDateLocalization _dateLocalization;


        public SeoDriver(IWorkContextAccessor workContextAccessor,
            IDateServices dateServices, IDateLocalization dateLocalization) {
            _workContextAccessor = workContextAccessor;
            _dateServices = dateServices;
            _dateLocalization = dateLocalization;
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
            metaRobots += part.RobotsUnavailableAfter ? "unavailable_after:" + part.RobotsUnavailableAfterDate.ToUniversalTime().ToString("r") + "," : "";
            metaRobots += part.RobotsNoImageIndex ? "noimageindex," : "";
            if (!string.IsNullOrWhiteSpace(metaRobots)) {
                resourceManager.SetMeta(new MetaEntry {
                    Name = "robots",
                    Content = metaRobots
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

            if (part.RobotsUnavailableAfterDate < (DateTime)SqlDateTime.MinValue) {
                part.RobotsUnavailableAfterDate = ((DateTime)SqlDateTime.MinValue).AddDays(1);
            }
            part.RobotsUnavailableAfterDate = part.RobotsUnavailableAfterDate.ToLocalTime();
            //part.RobotsUnavailableAfterDate = DateTime.SpecifyKind(part.RobotsUnavailableAfterDate, DateTimeKind.Local);
            return ContentShape("Parts_SEO_Edit",
                                () => shapeHelper.EditorTemplate(
                                  TemplateName: "Parts/SEO",
                                  Model: part,
                                  Prefix: Prefix));
        }


        /// <summary>
        /// POST Editor.
        /// </summary>
        protected override DriverResult Editor(SeoPart part, IUpdateModel updater, dynamic shapeHelper) {

            updater.TryUpdateModel(part, Prefix, null, null);
            part.RobotsUnavailableAfterDate = DateTime.SpecifyKind(part.RobotsUnavailableAfterDate, DateTimeKind.Utc);
            //part.RobotsUnavailableAfterDate = (DateTime)(_dateServices.ConvertFromLocalString(_dateLocalization.WriteDateLocalized(part.RobotsUnavailableAfterDate), _dateLocalization.WriteTimeLocalized(part.RobotsUnavailableAfterDate)));
            //part.RobotsUnavailableAfterDate = part.RobotsUnavailableAfterDate.ToUniversalTime();
            return Editor(part, shapeHelper);
        }


    }
}
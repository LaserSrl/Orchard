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
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.SEO.Drivers {


    public class SeoDriver : ContentPartCloningDriver<SeoPart> {


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

        protected override void Cloning(SeoPart originalPart, SeoPart clonePart, CloneContentContext context) {
            clonePart.TitleOverride = originalPart.TitleOverride;
            clonePart.Keywords = originalPart.Keywords;
            clonePart.Description = originalPart.Description;
            clonePart.RobotsNoIndex = originalPart.RobotsNoIndex;
            clonePart.RobotsNoFollow = originalPart.RobotsNoFollow;
            clonePart.RobotsNoSnippet = originalPart.RobotsNoSnippet;
            clonePart.RobotsNoOdp = originalPart.RobotsNoOdp;
            clonePart.RobotsNoArchive = originalPart.RobotsNoArchive;
            clonePart.RobotsUnavailableAfter = originalPart.RobotsUnavailableAfter;
            clonePart.RobotsUnavailableAfterDate = originalPart.RobotsUnavailableAfterDate;
            clonePart.RobotsNoImageIndex = originalPart.RobotsNoImageIndex;
            clonePart.GoogleNoSiteLinkSearchBox = originalPart.GoogleNoSiteLinkSearchBox;
            clonePart.GoogleNoTranslate = originalPart.GoogleNoTranslate;
        }
        protected override void Importing(SeoPart part, ImportContentContext context) {
            var importedTitleOverride = context.Attribute(part.PartDefinition.Name, "TitleOverride");
            if (importedTitleOverride != null) {
                part.TitleOverride = importedTitleOverride;
            }

            var importedKeywords = context.Attribute(part.PartDefinition.Name, "Keywords");
            if (importedKeywords != null) {
                part.Keywords = importedKeywords;
            }

            var importedDescription = context.Attribute(part.PartDefinition.Name, "Description");
            if (importedDescription != null) {
                part.Description = importedDescription;
            }

            var importedRobotsNoIndex = context.Attribute(part.PartDefinition.Name, "RobotsNoIndex");
            if (importedRobotsNoIndex != null) {
                part.RobotsNoIndex = Convert.ToBoolean(importedRobotsNoIndex);
            }

            var importedRobotsNoFollow = context.Attribute(part.PartDefinition.Name, "RobotsNoFollow");
            if (importedRobotsNoFollow != null) {
                part.RobotsNoFollow = Convert.ToBoolean(importedRobotsNoFollow);
            }

            var importedRobotsNoSnippet = context.Attribute(part.PartDefinition.Name, "RobotsNoSnippet");
            if (importedRobotsNoSnippet != null) {
                part.RobotsNoSnippet = Convert.ToBoolean(importedRobotsNoSnippet);
            }

            var importedRobotsNoOdp = context.Attribute(part.PartDefinition.Name, "RobotsNoOdp");
            if (importedRobotsNoOdp != null) {
                part.RobotsNoOdp = Convert.ToBoolean(importedRobotsNoOdp);
            }

            var importedRobotsNoArchive = context.Attribute(part.PartDefinition.Name, "RobotsNoArchive");
            if (importedRobotsNoArchive != null) {
                part.RobotsNoArchive = Convert.ToBoolean(importedRobotsNoArchive);
            }

            var importedRobotsUnavailableAfter = context.Attribute(part.PartDefinition.Name, "RobotsUnavailableAfter");
            if (importedRobotsUnavailableAfter != null) {
                part.RobotsUnavailableAfter = Convert.ToBoolean(importedRobotsUnavailableAfter);
            }

            var importedRobotsUnavailableAfterDate = context.Attribute(part.PartDefinition.Name, "RobotsUnavailableAfterDate");
            if (importedRobotsUnavailableAfterDate != null) {
                part.RobotsUnavailableAfterDate = DateTime.Parse(importedRobotsUnavailableAfterDate);
            }

            var importedRobotsNoImageIndex = context.Attribute(part.PartDefinition.Name, "RobotsNoImageIndex");
            if (importedRobotsNoImageIndex != null) {
                part.RobotsNoImageIndex = Convert.ToBoolean(importedRobotsNoImageIndex);
            }

            var importedGoogleNoSiteLinkSearchBox = context.Attribute(part.PartDefinition.Name, "GoogleNoSiteLinkSearchBox");
            if (importedGoogleNoSiteLinkSearchBox != null) {
                part.GoogleNoSiteLinkSearchBox = Convert.ToBoolean(importedGoogleNoSiteLinkSearchBox);
            }

            var importedGoogleNoTranslate = context.Attribute(part.PartDefinition.Name, "GoogleNoTranslate");
            if (importedGoogleNoTranslate != null) {
                part.GoogleNoTranslate = Convert.ToBoolean(importedGoogleNoTranslate);
            }

        }

        protected override void Exporting(SeoPart part, ExportContentContext context) 
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("TitleOverride", part.TitleOverride);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Keywords", part.Keywords);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Description", part.Description);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoIndex", part.RobotsNoIndex);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoFollow", part.RobotsNoFollow);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoSnippet", part.RobotsNoSnippet);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoOdp", part.RobotsNoOdp);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoArchive", part.RobotsNoArchive);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsUnavailableAfter", part.RobotsUnavailableAfter);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsUnavailableAfterDate", part.RobotsUnavailableAfterDate);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoImageIndex", part.RobotsNoImageIndex);
            context.Element(part.PartDefinition.Name).SetAttributeValue("GoogleNoSiteLinkSearchBox", part.GoogleNoSiteLinkSearchBox);
            context.Element(part.PartDefinition.Name).SetAttributeValue("GoogleNoTranslate", part.GoogleNoTranslate);
        }


    }
}
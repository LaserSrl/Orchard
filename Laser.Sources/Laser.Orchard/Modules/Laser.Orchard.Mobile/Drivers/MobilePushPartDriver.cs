﻿using AutoMapper;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Settings;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;


namespace Laser.Orchard.Mobile.Drivers {
    public class MobilePushPartDriver : ContentPartDriver<MobilePushPart> {

        private readonly IOrchardServices _orchardServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Mobile.MobilePush"; }
        }

        public MobilePushPartDriver(IOrchardServices orchardServices, IControllerContextAccessor controllerContextAccessor) {
            _orchardServices = orchardServices;
            _controllerContextAccessor = controllerContextAccessor;
        }

        protected override DriverResult Editor(MobilePushPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);

        }

        protected override DriverResult Editor(MobilePushPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new MobilePushVM();
            viewModel.ShowTestOptions = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ShowTestOptions;
            if (updater != null) {
                if (viewModel.ShowTestOptions == false)
                    viewModel.TestPush = false;
                // We are in "postback" mode, so update our part
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    Mapper.CreateMap<MobilePushVM, MobilePushPart>();
                    Mapper.Map(viewModel, part);

                } else
                    updater.AddModelError("Cannotupdate", T("Cannot Update!"));
            } else {
                //   viewModel.ListCountries = _countriesService.GetAllNazione();
                // We are in render mode (not postback), so initialize our view model.
                //Mapper.CreateMap<MobilePushPart, MobilePushVM>();

                //Mapper.Map(part, viewModel);
                viewModel.TitlePush = part.TitlePush;
                viewModel.TextPush = part.TextPush;
                viewModel.ToPush = part.ToPush;
                viewModel.TestPush = part.TestPush;
                viewModel.DevicePush = part.DevicePush;
            }

            viewModel.HideRelated = part.Settings.GetModel<PushMobilePartSettingVM>().HideRelated;
            _controllerContextAccessor.Context.Controller.TempData["HideRelated"] = viewModel.HideRelated;
            return ContentShape("Parts_MobilePush_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobilePush_Edit", Model: viewModel, Prefix: Prefix));
        }

        protected override void Importing(MobilePushPart part, ImportContentContext context) {
            var TitlePush = context.Attribute(part.PartDefinition.Name, "TitlePush");
            if (TitlePush != null)
                part.TitlePush = TitlePush;
            var TextPush = context.Attribute(part.PartDefinition.Name, "TextPush");
            if (TextPush != null)
                part.TextPush = TextPush;
            var ToPush = context.Attribute(part.PartDefinition.Name, "ToPush");
            if (ToPush != null)
                part.ToPush = bool.Parse(ToPush);
            var TestPush = context.Attribute(part.PartDefinition.Name, "TestPush");
            if (TestPush != null)
                part.TestPush = bool.Parse(TestPush);
            var DevicePush = context.Attribute(part.PartDefinition.Name, "DevicePush");
            if (DevicePush != null)
                part.DevicePush = DevicePush;
        }

        protected override void Exporting(MobilePushPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("TitlePush", part.TitlePush);
            root.SetAttributeValue("TextPush", part.TextPush);
            root.SetAttributeValue("ToPush", part.ToPush);
            root.SetAttributeValue("TestPush", part.TestPush);
            root.SetAttributeValue("DevicePush", part.DevicePush);
        }
    }
}
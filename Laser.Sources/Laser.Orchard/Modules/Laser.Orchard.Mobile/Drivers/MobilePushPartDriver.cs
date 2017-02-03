using AutoMapper;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Settings;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin;
using System.Collections.Generic;

namespace Laser.Orchard.Mobile.Drivers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class MobilePushPartDriver : ContentPartDriver<MobilePushPart> {

        private readonly IOrchardServices _orchardServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IRepository<PushNotificationRecord> _repoPushNotification;
        private readonly ShellSettings _shellSettings;
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Mobile.MobilePush"; }
        }

        public MobilePushPartDriver(IOrchardServices orchardServices, IControllerContextAccessor controllerContextAccessor,
                                    IRepository<PushNotificationRecord> repoPushNotification, ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _controllerContextAccessor = controllerContextAccessor;
            _repoPushNotification = repoPushNotification;
            _shellSettings = shellSettings;
        }

        protected override DriverResult Display(MobilePushPart part, string displayType, dynamic shapeHelper)
        {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin)
            {
                if (displayType == "Summary")
                {
                    return ContentShape("Parts_MobilePush",
                        () => shapeHelper.Parts_MobilePush(ToPush: part.ToPush, PushSent: part.PushSent, TargetDeviceNumber: part.TargetDeviceNumber, PushSentNumber: part.PushSentNumber));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        protected override DriverResult Editor(MobilePushPart part, dynamic shapeHelper)
        {
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
                    // forza il valore di ToPush che altrimenti sembra non venire aggiornato correttamente
                    if (viewModel.PushSent)
                    {
                        viewModel.ToPush = true;
                    }
                    Mapper.Initialize(cfg => {
                        cfg.CreateMap<MobilePushVM, MobilePushPart>();
                    });
                    Mapper.Map<MobilePushVM, MobilePushPart>(viewModel, part);

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
                viewModel.TestPushToDevice = part.TestPushToDevice;
                viewModel.DevicePush = part.DevicePush;
                viewModel.PushSent = part.PushSent;
                viewModel.TargetDeviceNumber = part.TargetDeviceNumber;
                viewModel.PushSentNumber = part.PushSentNumber;
                viewModel.UseRecipientList = part.UseRecipientList;
                viewModel.RecipientList = part.RecipientList;
            }

            viewModel.SiteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl + "/" + _shellSettings.RequestUrlPrefix;

            viewModel.HideRelated = part.Settings.GetModel<PushMobilePartSettingVM>().HideRelated;
            _controllerContextAccessor.Context.Controller.TempData["HideRelated"] = viewModel.HideRelated;

            // Valorizzo TextNumberPushTest
            viewModel.PushTestNumber = _repoPushNotification.Count(x => x.Produzione == false);
            
            if (part.ContentItem.ContentType == "CommunicationAdvertising") {
                // Flag Approvato all'interno del tab
                viewModel.PushAdvertising = true;
                return ContentShape("Parts_MobilePush_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobilePush_Edit", Model: viewModel, Prefix: Prefix));
            } 
            else {
                // Flag Approvato in fondo
                viewModel.PushAdvertising = false;

                var shapes = new List<DriverResult>();
                shapes.Add(ContentShape("Parts_MobilePush_Edit",
                                 () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobilePush_Edit",
                                     Model: viewModel,
                                     Prefix: Prefix)));
                shapes.Add(ContentShape("Parts_MobilePushApproved_Edit",
                                 () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobilePushApproved_Edit",
                                     Model: viewModel,
                                     Prefix: Prefix)));

                return new CombinedResult(shapes);
            }
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
            var PushSent = context.Attribute(part.PartDefinition.Name, "PushSent");
            if (PushSent != null)
                part.PushSent = bool.Parse(PushSent);
            var TargetDeviceNumber = context.Attribute(part.PartDefinition.Name, "TargetDeviceNumber");
            if (TargetDeviceNumber != null)
                part.TargetDeviceNumber = int.Parse(TargetDeviceNumber);
            var PushSentNumber = context.Attribute(part.PartDefinition.Name, "PushSentNumber");
            if (PushSentNumber != null)
                part.PushSentNumber = int.Parse(PushSentNumber);
        }

        protected override void Exporting(MobilePushPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("TitlePush", part.TitlePush);
            root.SetAttributeValue("TextPush", part.TextPush);
            root.SetAttributeValue("ToPush", part.ToPush);
            root.SetAttributeValue("TestPush", part.TestPush);
            root.SetAttributeValue("DevicePush", part.DevicePush);
            root.SetAttributeValue("PushSent", part.PushSent);
            root.SetAttributeValue("TargetDeviceNumber", part.TargetDeviceNumber);
            root.SetAttributeValue("PushSentNumber", part.PushSentNumber);
        }
    }
}
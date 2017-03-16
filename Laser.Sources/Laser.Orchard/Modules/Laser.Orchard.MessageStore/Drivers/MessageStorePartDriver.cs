using Laser.Orchard.MessageStore.Models;
using Laser.Orchard.MessageStore.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using AutoMapper;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.MessageStore.Drivers {
    public class MessageStorePartDriver : ContentPartDriver<MessageStorePart> {

        private readonly IWorkContextAccessor _workContext;

        public MessageStorePartDriver(IWorkContextAccessor workContext
            ) {
            _workContext = workContext;
        }

        protected override string Prefix {
            get { return "Laser.MessageStore"; }
        }

        protected override DriverResult Editor(MessageStorePart part, dynamic shapeHelper) {
            
            var msgFrom = (_workContext.GetContext().HttpContext.Request.QueryString["msgfrom"] ?? "").ToString();
            var msgTo = (_workContext.GetContext().HttpContext.Request.QueryString["msgto"] ?? "").ToString();
            Int32 gruppoId = Convert.ToInt32(_workContext.GetContext().HttpContext.Request.QueryString["groupId"] ?? "0");
            var markMessage = (_workContext.GetContext().HttpContext.Request.QueryString["MarkMessage"] ?? "0");
            var filterTest = _workContext.GetContext().HttpContext.Request.QueryString["filterTest"];
            var onlyTest = (_workContext.GetContext().HttpContext.Request.QueryString["onlyText"] ?? "0").ToString();
            var windowclose = (_workContext.GetContext().HttpContext.Request.QueryString["CloseWindow"] ?? "0").ToString();

            bool viewOnlyTextMessage = false;
            if (onlyTest == "1")
                viewOnlyTextMessage = true;
            if (msgFrom != "")
                part.MessageFrom = msgFrom;
            if (msgTo != "")
                part.MessageTo = msgTo;
            if (gruppoId != 0)
                part.Gruppoid = gruppoId;
            if (markMessage == "1")
                part.MarkMessage = true;

            if (part.FilterTest == null)
                part.FilterTest = filterTest;
       
            MessageStoreEditModel msem= new MessageStoreEditModel();
            Mapper.CreateMap<MessageStorePart,MessageStoreEditModel>();
            Mapper.Map(part,msem);
            msem.showOnlyMessage=viewOnlyTextMessage;
            if (windowclose=="1")
              msem.CloseWindow=true;
            return ContentShape("Parts_MessageStore_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/MessageStore",
                    Model: msem,
                    Prefix: Prefix
                    ));
        }

        protected override DriverResult Editor(MessageStorePart part, IUpdateModel updater, dynamic shapeHelper) {
            MessageStoreEditModel msem = new MessageStoreEditModel();
            updater.TryUpdateModel(msem, Prefix, null, null);
            Mapper.CreateMap<MessageStoreEditModel, MessageStorePart>();
            Mapper.Map(msem, part);
            if (msem.showOnlyMessage)
                msem.CloseWindow = true;
            return Editor(part, shapeHelper);
        }


        protected override void Importing(MessageStorePart part, ImportContentContext context) {

            var importedMessageText = context.Attribute(part.PartDefinition.Name, "MessageText");
            if (importedMessageText != null) {
                part.MessageText = importedMessageText;
            }

            var importedGruppoid = context.Attribute(part.PartDefinition.Name, "Gruppoid");
            if (importedGruppoid != null) {
                part.Gruppoid = int.Parse(importedGruppoid);
            }

            var importedMessageFrom = context.Attribute(part.PartDefinition.Name, "MessageFrom");
            if (importedMessageFrom != null) {
                part.MessageFrom = importedMessageFrom;
            }

            var importedMessageTo = context.Attribute(part.PartDefinition.Name, "MessageTo");
            if (importedMessageTo != null) {
                part.MessageTo = importedMessageTo;
            }

            var importedMessageDate = context.Attribute(part.PartDefinition.Name, "MessageDate");
            if (importedMessageDate != null) {
                part.MessageDate = DateTime.Parse(importedMessageDate);
            }

            var importedMarkRead = context.Attribute(part.PartDefinition.Name, "MarkRead");
            if (importedMarkRead != null) {
                part.MarkRead = bool.Parse(importedMarkRead);
            }

            var importedFilterTest = context.Attribute(part.PartDefinition.Name, "FilterTest");
            if (importedFilterTest != null) {
                part.FilterTest = importedFilterTest;
            }

            var importedMessageTextHtml = context.Attribute(part.PartDefinition.Name, "MessageTextHtml");
            if (importedMessageTextHtml != null) {
                part.MessageTextHtml = importedMessageTextHtml;
            }

            var importedMessageObject = context.Attribute(part.PartDefinition.Name, "MessageObject");
            if (importedMessageObject != null) {
                part.MessageObject = importedMessageObject;
            }


        }

        protected override void Exporting(MessageStorePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("MessageText", part.MessageText);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Gruppoid", part.Gruppoid);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MessageFrom", part.MessageFrom);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MessageTo", part.MessageTo);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MessageDate", part.MessageDate);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MarkRead", part.MarkRead);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FilterTest", part.FilterTest);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MessageTextHtml", part.MessageTextHtml);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MessageObject", part.MessageObject);
        }



    }
}
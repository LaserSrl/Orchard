using Laser.Orchard.MessageStore.Models;
using Laser.Orchard.MessageStore.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using AutoMapper;

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
    }
}
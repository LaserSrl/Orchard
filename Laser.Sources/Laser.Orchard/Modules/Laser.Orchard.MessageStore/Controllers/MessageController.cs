using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.MessageStore.Services;
using Laser.Orchard.MessageStore.Models;
using Laser.Orchard.MessageStore.ViewModels;
using Orchard.Security;
using Orchard;

namespace Laser.Orchard.MessageStore.Controllers {
    public class MessageController : Controller {
        private readonly IMessageStoreService _messageStoreService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOrchardServices _orchardServices;

        public MessageController(
            IOrchardServices orchardServices,
            IMessageStoreService messageStoreService,
            IAuthenticationService authenticationService) {
            _orchardServices = orchardServices;
            _messageStoreService = messageStoreService;
            _authenticationService = authenticationService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult  Send(string messageFrom="", string messageTo="", int gruppoid=0, string messageObject="",string messageText="",string messageTextHtml="",bool markMessage=false,bool markRead=false ) {
            if (_authenticationService.GetAuthenticatedUser() != null) {
                MessageStoreEditModel msem = new MessageStoreEditModel();
                msem.MessageFrom = messageFrom;
                msem.MessageTo = messageTo;
                msem.MessageDate = DateTime.Now;
                msem.Gruppoid = gruppoid;
                msem.MessageObject = messageObject;
                msem.MessageText = messageText;
                msem.MessageTextHtml=messageTextHtml;
                msem.FilterTest="";
                msem.MarkMessage=markMessage;
                msem.MarkRead=markRead;         
                _messageStoreService.Create(msem);
                _messageStoreService.Send(msem);
                return Json(msem);
            }
            return null;
        }
    }
}


using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.MailCommunication.Controllers {
    public class UnsubscribeController : Controller {

        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        dynamic Shape { get; set; }

        public UnsubscribeController(INotifier notifier, IShapeFactory shapeFactory) {
            notifier = _notifier;
            Shape = shapeFactory;

            T = NullLocalizer.Instance;
        }

        [Themed, HttpGet]
        public ActionResult UnsubscribeIndex() {
            return new ShapeResult(this, Shape.Unsubscribe_Confirm());
        }

        [Themed, HttpPost]
        public ActionResult UnsubscribeMail(string email, string confirmEmail) {

            if (HttpContext.Request.Form["Mail_Unsubscribe_Confirm"] == "") {
                return null;
            } else {
                if (email == confirmEmail && email.Trim() != "") {

                    // TODO: CreateNonce e DecryptNonce inserirli nel StartupConfig
                    // Send Mail dopo aver richiamato CreateNonce e passando il risultato in querystring

                } else {
                    if (email == "" || confirmEmail == "") {
                        _notifier.Error(T("Please insert mail and confirm mail"));
                    } else if (email != confirmEmail) {
                        _notifier.Error(T("Email and email confirmation must match!"));
                    }
                }
            }
            return new ShapeResult(this, Shape.Unsubscribe_Confirm());
        }


        //[Themed]
        //public ActionResult ConfirmUnsubscribe(string email, string token) {
        //    var subRecord = _newsletterServices.TryUnregisterConfirmSubscriber(new SubscriberViewModel {
        //        Email = email,
        //        Guid = token
        //    });
        //    try {
        //        ValidateModel(subRecord);
        //    } catch (Exception ex) {
        //        ModelState.AddModelError("SubscriberError", ex);
        //    }

        //    if (subRecord != null) {
        //        return new ShapeResult(this, Shape.Subscription_UnsubscribedEmail());
        //    } else {
        //        return new ShapeResult(this, Shape.Subscription_UnsubscribeError());
        //    }
        //}

    }
}
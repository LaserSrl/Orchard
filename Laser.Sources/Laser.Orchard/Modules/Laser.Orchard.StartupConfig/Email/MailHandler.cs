using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Orchard.Environment.Extensions;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;

namespace Laser.Orchard.StartupConfig.Email {
    [OrchardFeature("Laser.Orchard.StartupConfig.MailExtensions")]
    public class MailHandler : IMessageEventHandler {
        public void Sending(MessageContext context) {
            if (context.Type == "ActionEmail") {
                if (context.Properties.ContainsKey("Attachment")) {
                    var path = context.Properties["Attachment"];
                    var attachment = new Attachment(path);
                    context.MailMessage.Attachments.Add(attachment);
                } else if (context.Properties.ContainsKey("CC")) {
                    var cc = context.Properties["CC"];
                    context.MailMessage.CC.Add(cc);
                } else if (context.Properties.ContainsKey("Bcc")) {
                    var bcc = context.Properties["Bcc"];
                    context.MailMessage.Bcc.Add(bcc);
                }

            }
        }
        public void Sent(MessageContext context) { }
    }

}
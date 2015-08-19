


using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using System;
using System.Net.Mail;
namespace Laser.Orchard.MessageStore.Events {
    public class MyMessageHandler : IMessageEventHandler {
        public void Sending(MessageContext context) {
            if (context.MessagePrepared)
                return;
            switch (context.Type) {
                case "ActionEmail":
                    context.MailMessage.Subject = context.Properties["Subject"];
                    context.MailMessage.Body = context.Properties["Body"];
                 
                  //  context.MailMessage.To.Add("lorenzo.frediani@gmail.com");
                    if (context.Properties.ContainsKey("Attachment")) {
                        var listpath = context.Properties["Attachment"].Split(new string[] {"||"}, StringSplitOptions.None);
                        foreach (var path in listpath) {
                            if (path != "") {
                                var attachment = new Attachment(path);
                                context.MailMessage.Attachments.Add(attachment);
                            }
                        }
                    }
                    context.MessagePrepared = true;
                    break;
            }
        }

        //we don't care about this right now
        public void Sent(MessageContext context) { }
    }
}
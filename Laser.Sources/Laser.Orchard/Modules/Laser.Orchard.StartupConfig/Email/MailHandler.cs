using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Email.Models;
using Orchard.Email.Services;
using Orchard;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Email {
    [OrchardFeature("Laser.Orchard.StartupConfig.MailExtensions")]
    public class MailHandler : Component, ISmtpChannel, IDisposable {
        private readonly SmtpSettingsPart _smtpSettings;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly Lazy<SmtpClient> _smtpClientField;
        public static readonly string MessageType = "Email";

        public MailHandler(
            IOrchardServices orchardServices,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay) {
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;

            _smtpSettings = orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            _smtpClientField = new Lazy<SmtpClient>(CreateSmtpClient);
        }

        public void Dispose() {
            if (!_smtpClientField.IsValueCreated) {
                return;
            }

            _smtpClientField.Value.Dispose();
        }

        public void Process(IDictionary<string, object> parameters) {


            if (!_smtpSettings.IsValid()) {
                return;
            }
            string _recipient = "", _cc = "", _bcc = "";
            Attachment _attachment = null;
            if (parameters["Recipients"] is String[]) {
                _recipient = String.Join(",", ((string[])parameters["Recipients"]));
            } else
                _recipient = parameters["Recipients"] as string;

            if (parameters.ContainsKey("Attachment")) {
                var path = parameters["Attachment"].ToString();
                _attachment = new Attachment(path);
            } else if (parameters.ContainsKey("CC")) {
                _cc = parameters["CC"].ToString();
            } else if (parameters.ContainsKey("Bcc")) {
                _bcc = parameters["Bcc"].ToString();
            }
            var emailMessage = new EmailMessage {
                Body = parameters["Body"] as string,
                Subject = parameters["Subject"] as string,
                Recipients = _recipient
            };

            if (emailMessage.Recipients.Length == 0) {
                Logger.Error("Email message doesn't have any recipient");
                return;
            }

            // Applying default Body alteration for SmtpChannel
            var template = _shapeFactory.Create("Template_Smtp_Wrapper", Arguments.From(new {
                Content = new MvcHtmlString(emailMessage.Body)
            }));

            var mailMessage = new MailMessage {
                Subject = emailMessage.Subject,
                Body = _shapeDisplay.Display(template),
                IsBodyHtml = true
            };

            var section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            mailMessage.From = !String.IsNullOrWhiteSpace(_smtpSettings.Address)
                ? new MailAddress(_smtpSettings.Address)
                : new MailAddress(section.From);
            if (_attachment != null) {
                mailMessage.Attachments.Add(_attachment);
            }
            try {
                foreach (var recipient in emailMessage.Recipients.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    mailMessage.To.Add(new MailAddress(recipient));
                }
                foreach (var cc in _cc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    mailMessage.CC.Add(new MailAddress(cc));
                }
                foreach (var bcc in _bcc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    mailMessage.Bcc.Add(new MailAddress(bcc));
                }
                foreach (var bcc in _bcc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    mailMessage.Bcc.Add(new MailAddress(bcc));
                }

                _smtpClientField.Value.Send(mailMessage);
            } catch (Exception e) {
                Logger.Error(e, "Could not send email");
            }
        }

        private SmtpClient CreateSmtpClient() {
            // if no properties are set in the dashboard, use the web.config value
            if (String.IsNullOrWhiteSpace(_smtpSettings.Host)) {
                return new SmtpClient();
            }

            var smtpClient = new SmtpClient {
                UseDefaultCredentials = !_smtpSettings.RequireCredentials,
            };

            if (!smtpClient.UseDefaultCredentials && !String.IsNullOrWhiteSpace(_smtpSettings.UserName)) {
                smtpClient.Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);
            }

            if (_smtpSettings.Host != null) {
                smtpClient.Host = _smtpSettings.Host;
            }

            smtpClient.Port = _smtpSettings.Port;
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            return smtpClient;
        }
    }


}
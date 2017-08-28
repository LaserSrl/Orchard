using Laser.Orchard.HID.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Security;
using System;
using System.Text;

namespace Laser.Orchard.HID.Handlers {
    public class HIDSettingsPartHandler : ContentHandler {
        private readonly IEncryptionService _encryptionService;
        public new ILogger Logger { get; set; }

        public HIDSettingsPartHandler(IEncryptionService encryptionService) {
            _encryptionService = encryptionService;
            Filters.Add(new ActivatingFilter<HIDSiteSettingsPart>("Site"));

            Logger = NullLogger.Instance;

            OnLoaded<HIDSiteSettingsPart>(LazyLoadHandlers);
        }

        void LazyLoadHandlers(LoadContentContext context, HIDSiteSettingsPart part) {
            part.ClientSecretField.Getter(() => {
                try {
                    string encryptedPassword = part.Retrieve(x => x.ClientSecret);
                    return String.IsNullOrWhiteSpace(encryptedPassword) 
                        ? String.Empty 
                        : Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(encryptedPassword)));
                } catch {
                    Logger.Error("The HID password could not be decrypted. It might be corrupted, try to reset it.");
                    return null;
                }
            });

            part.ClientSecretField.Setter(value => {
                var encryptedPassword = String.IsNullOrWhiteSpace(value) ? String.Empty : Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(value)));
                part.Store(x => x.ClientSecret, encryptedPassword);
            });
        }

    }
}
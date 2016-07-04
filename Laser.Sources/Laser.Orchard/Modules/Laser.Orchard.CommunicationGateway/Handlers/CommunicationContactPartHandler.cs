using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.StartupConfig.Handlers;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class CommunicationContactPartHandler : ContentHandler, IContactRelatedEventHandler {
        public Localizer T { get; set; }
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<CommunicationEmailRecord> _Emailrepository;
        private readonly IRepository<CommunicationSmsRecord> _Smsrepository;

        public CommunicationContactPartHandler(IRepository<CommunicationSmsRecord> Smsrepository,IRepository<CommunicationEmailRecord> Emailrepository,IRepository<CommunicationContactPartRecord> repository, ICommunicationService communicationService) {
            _Smsrepository = Smsrepository;
            _communicationService = communicationService;
            Filters.Add(StorageFilter.For(repository));
            _Emailrepository = Emailrepository;
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<EmailContactPart>("CommunicationContact"));
            OnLoaded<EmailContactPart>(LazyLoadEmailHandlers);

            Filters.Add(new ActivatingFilter<SmsContactPart>("CommunicationContact"));
            OnLoaded<SmsContactPart>(LazyLoadSmsHandlers);

            Filters.Add(new ActivatingFilter<FavoriteCulturePart>("CommunicationContact"));

            #region sync user profile
            OnCreated<UserPart>((context, part) => UpdateProfile(context.ContentItem));
            OnUpdated<UserPart>((context, part) => UpdateProfile(context.ContentItem));
            OnRemoved<UserPart>((context, part) => { _communicationService.UnboundFromUser(part); });
            OnRemoved<CommunicationContactPart>((context, part) => { _communicationService.RemoveMailsAndSms(part.Id); });
            #endregion
        }

        protected void LazyLoadEmailHandlers(LoadContentContext context, EmailContactPart part) {
            // Add handlers that will load content for id's just-in-time
            part.EmailEntries.Loader(x => OnEmailLoader(context));
        }

        private IList<CommunicationEmailRecord> OnEmailLoader(LoadContentContext context) {
            return _Emailrepository
                    .Fetch(x => x.EmailContactPartRecord_Id == context.ContentItem.Id)
                    .Select(x => new CommunicationEmailRecord {
                        DataInserimento = x.DataInserimento,
                        DataModifica = x.DataModifica,
                        Language = x.Language,
                        Id = x.Id,
                        Produzione = x.Produzione,
                        Email = x.Email,
                        EmailContactPartRecord_Id = x.EmailContactPartRecord_Id,
                        Validated = x.Validated,
                        AccettatoUsoCommerciale = x.AccettatoUsoCommerciale,
                        AutorizzatoTerzeParti = x.AutorizzatoTerzeParti
                    })
                    .ToList();
        }


        protected void LazyLoadSmsHandlers(LoadContentContext context, SmsContactPart part) {
            // Add handlers that will load content for id's just-in-time
            part.SmsEntries.Loader(x => OnSmsLoader(context));
        }

        private IList<CommunicationSmsRecord> OnSmsLoader(LoadContentContext context) {
            return _Smsrepository
                    .Fetch(x => x.SmsContactPartRecord_Id == context.ContentItem.Id)
                    .Select(x => new CommunicationSmsRecord {
                        DataInserimento = x.DataInserimento,
                        DataModifica = x.DataModifica,
                        Language = x.Language,
                        Id = x.Id,
                        Produzione = x.Produzione,
                        Sms = x.Sms,
                        Prefix = x.Prefix,
                        SmsContactPartRecord_Id = x.SmsContactPartRecord_Id,
                        Validated = x.Validated,
                        AccettatoUsoCommerciale = x.AccettatoUsoCommerciale,
                        AutorizzatoTerzeParti = x.AutorizzatoTerzeParti
                    })
                    .ToList();
        }

        private void UpdateProfile(ContentItem item) {
            if (item.ContentType == "User") {
                _communicationService.UserToContact((IUser)item.As<IUser>());
            }
        }

        public void DeviceUpdated() {
            //throw new System.NotImplementedException();
        }

        public void SmsUpdated() {
            //throw new System.NotImplementedException();
        }
    }
}
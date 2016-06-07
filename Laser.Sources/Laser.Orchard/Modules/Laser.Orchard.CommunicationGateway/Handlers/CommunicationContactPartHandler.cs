﻿using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
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

    public class CommunicationContactPartHandler : ContentHandler {
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
            OnRemoved<UserPart>((context, part) => RemoveUserLink(part));
            OnRemoved<CommunicationContactPart>((context, part) => RemoveLinks(part));
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
        private void RemoveLinks(CommunicationContactPart item) {
            item.UserIdentifier=0;

            // elimina le mail associate
            var elencoCer = _Emailrepository.Fetch(x => x.EmailContactPartRecord_Id == item.Id);
            if (elencoCer != null) {
                foreach (var cer in elencoCer) {
                    _Emailrepository.Delete(cer);
                }
            }

            // elimina gli sms associati
            var elencoCsr = _Smsrepository.Fetch(x => x.SmsContactPartRecord_Id == item.Id);
            if (elencoCsr != null) {
                foreach (var csr in elencoCsr) {
                    _Smsrepository.Delete(csr);
                }
            }
        }

        private void RemoveUserLink(UserPart part) {
            _communicationService.UnboundFromUser(part);
        }
    }
}
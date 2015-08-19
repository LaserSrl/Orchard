using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Users.Models;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Data;
using Laser.Orchard.Mobile.Models;

namespace Laser.Orchard.Mobile.Handlers {
    public class UserDeviceHandler : IUserEventHandler {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<UserDeviceRecord> _userDeviceRecord;

        public UserDeviceHandler(
            IHttpContextAccessor httpContextAccessor,
            IRepository<UserDeviceRecord> userDeviceRecord
            ) {
            _httpContextAccessor = httpContextAccessor;
            _userDeviceRecord = userDeviceRecord;
        }
        public void AccessDenied(IUser user) {
          //  throw new NotImplementedException();
        }

        public void Approved(IUser user) {
          //  throw new NotImplementedException();
        }

        public void ChangedPassword(IUser user) {
         //   throw new NotImplementedException();
        }

        public void ConfirmedEmail(IUser user) {
         //   throw new NotImplementedException();
        }

        public void Created(UserContext context) {
         //   throw new NotImplementedException();
        }

        public void Creating(UserContext context) {
         //   throw new NotImplementedException();
        }

        public void LoggedIn(IUser user) {
            var Email = _httpContextAccessor.Current().Request.QueryString["Email"];
            if (!string.IsNullOrWhiteSpace(Email)) {
                ((UserPart)user).Email = Email;
            }
            var UUIdentifier = _httpContextAccessor.Current().Request.QueryString["UUID"];
            if (!string.IsNullOrWhiteSpace(UUIdentifier)) {
                var record = _userDeviceRecord.Fetch(x => x.UUIdentifier == UUIdentifier).FirstOrDefault();
                if (record == null) {
                    UserDeviceRecord newUD = new UserDeviceRecord();
                    newUD.UUIdentifier = UUIdentifier;
                    newUD.UserPartRecord = ((dynamic)user).Record;
                    _userDeviceRecord.Create(newUD);
                    _userDeviceRecord.Flush();
                }
                else {
                    if (record.UserPartRecord.Id != user.Id) {
                        record.UserPartRecord = ((dynamic)user).Record;
                        _userDeviceRecord.Update(record);
                        _userDeviceRecord.Flush();
                    }
                }
            }

        }

        public void LoggedOut(IUser user) {
         //   throw new NotImplementedException();
        }

        public void SentChallengeEmail(IUser user) {
         //   throw new NotImplementedException();
        }
    }
}
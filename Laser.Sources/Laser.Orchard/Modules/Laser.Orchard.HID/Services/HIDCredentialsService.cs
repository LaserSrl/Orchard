using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.HID.Models;
using Orchard.Security;

namespace Laser.Orchard.HID.Services {
    public class HIDCredentialsService : IHIDCredentialsService {

        private readonly IHIDSearchUserService _HIDSearchUserService;

        public HIDCredentialsService(
            IHIDSearchUserService HIDSearchUserService) {

            _HIDSearchUserService = HIDSearchUserService;
        }

        public HIDUser IssueCredentials(HIDUser hidUser, string[] partNumbers) {
            if (partNumbers.Length == 0) {
                hidUser = hidUser.IssueCredential(""); //this assigns the default part number for the customer
            } else {
                foreach (var pn in partNumbers) {
                    hidUser = hidUser.IssueCredential(pn);
                    if (hidUser.Error != UserErrors.NoError && hidUser.Error != UserErrors.PreconditionFailed) {
                        break;  //break on error, but not on PreconditionFailed, because that may be caused by the credential having been
                        //assigned already, which is fine
                    }
                }
            }
            return hidUser;
        }

        public HIDUser IssueCredentials(IUser user, string[] partNumbers) {
            var searchResult = _HIDSearchUserService.SearchHIDUser(user.Email);
            if (searchResult.Error == SearchErrors.NoError) {
                return IssueCredentials(searchResult.User, partNumbers);
            } else {
                return new HIDUser();
            }
        }
        
        public HIDUser RevokeCredentials(HIDUser hidUser, string[] partNumbers) {
            if (partNumbers.Length == 0) {
                hidUser = hidUser.RevokeCredential();
            } else {
                foreach (var pn in partNumbers) {
                    hidUser = hidUser.RevokeCredential(pn);
                    if (hidUser.Error != UserErrors.NoError && hidUser.Error != UserErrors.PreconditionFailed) {
                        break;  //break on error, but not on PreconditionFailed, because that may be caused by the credential being
                        //revoked right now
                    }
                }
            }
            return hidUser;
        }

        public HIDUser RevokeCredentials(IUser user, string[] partNumbers) {
            var searchResult = _HIDSearchUserService.SearchHIDUser(user.Email);
            if (searchResult.Error == SearchErrors.NoError) {
                return RevokeCredentials(searchResult.User, partNumbers);
            } else {
                return new HIDUser();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard.Security;
using Orchard.Environment.Extensions;
using Orchard.Users.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Users.Models;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceLoginNonceService : INonceService {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IUserService _userService;
        private readonly IEnumerable<IOTPDeliveryService> _deliveryServices;
        private readonly IOTPRepositoryService _otpRepositoryService;
        private readonly IMembershipService _membershipService;

        public NonceLoginNonceService(
            IWorkContextAccessor workContextAccessor,
            IUserService userService,
            IEnumerable<IOTPDeliveryService> deliveryServices,
            IOTPRepositoryService otpRepositoryService,
            IMembershipService membershipService) {

            _workContextAccessor = workContextAccessor;
            _userService = userService;
            _deliveryServices = deliveryServices;
            _otpRepositoryService = otpRepositoryService;
            _membershipService = membershipService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private int ValidityTime() {
            return _workContextAccessor.GetContext().CurrentSite.As<NonceLoginSettingsPart>().NonceMinutesValidity;
        }

        private OTPRecord NewOTP(UserPart user) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }

            // use the base nonce from the IUserServices
            var nonce = _userService.CreateNonce(user, new TimeSpan(0, ValidityTime(), 0));
            string userName;
            DateTime expiration;
            // get the expiration actually assigned on the nonce
            _userService.DecryptNonce(nonce, out userName, out expiration);
            // create the OTP
            var otp = new OTPRecord {
                UserRecord = user.As<UserPart>().Record,
                Password = nonce,
                PasswordType = PasswordType.Nonce.ToString(),
                ExpirationUTCDate = expiration
            };
            // save the OTP
            return _otpRepositoryService.AddOTP(otp);
        }

        public string GenerateOTP(IUser user) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }

            var otp = NewOTP(user.As<UserPart>());

            return otp.Password;
        }

        public string GenerateOTP(IUser user, Dictionary<string, string> additionalInformation) {
            return GenerateOTP(user);
        }

        public bool SendNewOTP(IUser user, DeliveryChannelType? channel) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }

            // create OTP
            var otp = NewOTP(user.As<UserPart>());

            return SendOTP(otp, user, channel);
        }

        public bool SendNewOTP(IUser user, Dictionary<string, string> additionalInformation, DeliveryChannelType? channel) {
            return SendNewOTP(user, channel);
        }

        public bool SendOTP(OTPRecord otp, DeliveryChannelType? channel) {
            if (otp == null) {
                throw new ArgumentNullException("otp");
            }

            // get recipient
            var user = _membershipService.GetUser(otp.UserRecord.UserName);

            return SendOTP(otp, user, channel);
        }

        private bool SendOTP(OTPRecord otp, IUser user, DeliveryChannelType? channel) {
            // Select / order delivery services
            var deliveryServices = _deliveryServices;
            if (channel != null) {
                deliveryServices = deliveryServices.Where(ds => ds.ChannelType == channel);
            }
            deliveryServices = deliveryServices.OrderByDescending(ds => ds.Priority);


            // send through the first channel that does not fail
            var success = false;
            foreach (var ds in deliveryServices) {
                success = ds.TrySendOTP(otp, user);
                if (success)
                    break; // break on first success
            }

            return success;
        }


        public IUser UserFromNonce(string nonce) {
            var otp = _otpRepositoryService.Get(nonce, PasswordType.Nonce.ToString());
            IUser user = null;
            if (otp != null) {
                if (otp.ExpirationUTCDate <= DateTime.UtcNow) {
                    // otp still valid
                    // get recipient
                    user = _membershipService.GetUser(otp.UserRecord.UserName);
                }
                // otp has been used, so delete it
                _otpRepositoryService.Delete(otp);
            }

            return user;
        }

        public bool ValidatePassword(OTPContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }
            if (context.User == null) {
                throw new ArgumentException(T("Context.User cannot be null.").Text);
            }

            //get the otp
            var valid = false;
            var otp = _otpRepositoryService.Get(context.Password, PasswordType.Nonce.ToString());
            if (otp != null) {
                if (otp.ExpirationUTCDate <= DateTime.UtcNow) {
                    // otp still valid
                    // get recipient
                    var user = _membershipService.GetUser(otp.UserRecord.UserName);
                    if (user.UserName == context.User.UserName) {
                        // same user
                        valid = true;
                    }
                }
                // otp has been used, so delete it
                _otpRepositoryService.Delete(otp);
            }

            return valid;
        }

    }
}
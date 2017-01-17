using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using System;
using System.Web.Http;

namespace Laser.Orchard.HID.Controllers {
    [WebApiKeyFilter(true)]
    public class HIDAPIController : ApiController {

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private readonly IHIDAPIService _HIDAPIService;
        private readonly IOrchardServices _orchardServices;

        public HIDAPIController(IHIDAPIService hidapiService, IOrchardServices orchardService) {
            _HIDAPIService = hidapiService;
            _orchardServices = orchardService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        [System.Web.Mvc.HttpGet]
        [Authorize]
        public Response GetInvitation() {
            string message = "";
            HIDErrorCode eCode = HIDErrorCode.GenericError;
            HIDResolutionAction rAction = HIDResolutionAction.NoAction;
            bool success = false;
            string InvitationCode = "";
            IUser caller = _orchardServices.WorkContext.CurrentUser;
            var searchResult = _HIDAPIService.SearchHIDUser(caller);

            /*****************TEST CODE********************/
            //searchResult.User.IssueCredential("CRD633ZZ-TST0053");
            //searchResult.User.RevokeCredential();
            /**************************************/
            switch (searchResult.Error) {
                case SearchErrors.NoError:
                    var hidUser = searchResult.User;
                    if (hidUser.Error == UserErrors.NoError) {
                        InvitationCode = hidUser.CreateInvitation();
                        if (hidUser.Error == UserErrors.NoError) {
                            success = true;
                            eCode = HIDErrorCode.NoError;
                            message = T("The Data field contains the new InvitationCode.").Text;
                        } else {
                            HandleHIDUserError(hidUser, caller, out eCode, out rAction, out message);
                        }
                    } else {
                        HandleHIDUserError(hidUser, caller, out eCode, out rAction, out message);
                    }
                    break;
                case SearchErrors.InvalidParameters:
                    eCode = HIDErrorCode.InvalidSearchParameters;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("There was an error with the parameters passed for the search. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                case SearchErrors.AuthorizationFailed:
                    eCode = HIDErrorCode.AuthenticationFailed;
                    rAction = HIDResolutionAction.TryAgain;
                    message = T("There was an error while authenticating to the HID servers. This may be a temporary condition. Please try again.").Text;
                    break;
                case SearchErrors.InternalServerError:
                    eCode = HIDErrorCode.HIDServerError;
                    rAction = HIDResolutionAction.TryAgain;
                    message = T("There was an error on the HID servers. This may be a temporary condition. Please try again.").Text;
                    break;
                case SearchErrors.UnknownError:
                    eCode = HIDErrorCode.GenericError;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("Unknown error while searching for user on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                case SearchErrors.NoResults:
                    eCode = HIDErrorCode.UserDoesNotExist;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("User does not exist on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                case SearchErrors.TooManyResults:
                    eCode = HIDErrorCode.UserNotUnique;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("User is not unique on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                default:
                    eCode = HIDErrorCode.GenericError;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("Unknown error while searching for user on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
            }
            if (eCode != HIDErrorCode.NoError) {
                Logger.Error(message);
            }
            var response = new HIDResponse() {
                ErrorCode = eCode,
                Success = success,
                Message = message,
                ResolutionAction = rAction
            };
            if (eCode == HIDErrorCode.NoError) {
                response.Data = new { InvitationCode = InvitationCode };
            }
            return (Response)response;
        }

        private void HandleHIDUserError(HIDUser hidUser, IUser caller, out HIDErrorCode eCode, out HIDResolutionAction rAction, out string message) {
            switch (hidUser.Error) {
                case UserErrors.UnknownError:
                    eCode = HIDErrorCode.GenericError;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("Unknown error while searching for user on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                case UserErrors.DoesNotExist:
                    eCode = HIDErrorCode.UserDoesNotExist;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("User does not exist on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                case UserErrors.AuthorizationFailed:
                    eCode = HIDErrorCode.AuthenticationFailed;
                    rAction = HIDResolutionAction.TryAgain;
                    message = T("There was an error while authenticating to the HID servers. This may be a temporary condition. Please try again.").Text;
                    break;
                case UserErrors.InternalServerError:
                    eCode = HIDErrorCode.HIDServerError;
                    rAction = HIDResolutionAction.TryAgain;
                    message = T("There was an error on the HID servers. This may be a temporary condition. Please try again.").Text;
                    break;
                case UserErrors.InvalidParameters:
                    eCode = HIDErrorCode.InvalidSearchParameters;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("There was an error with the parameters passed for the search. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                case UserErrors.EmailNotUnique:
                    eCode = HIDErrorCode.UserNotUnique;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("User is not unique on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                case UserErrors.PreconditionFailed:
                    eCode = HIDErrorCode.GenericError;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("Unknown error while searching for user on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                default:
                    eCode = HIDErrorCode.GenericError;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("Unknown error while searching for user on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
            }
        }
    }


    //We extend Laser.Orchard.StartupConfig.ViewModels.Response because we have specific error codes for HID
    public enum HIDErrorCode {
        NoError = 0, GenericError = 1,
        AuthenticationFailed = 5001,
        HIDServerError = 5002,
        UserDoesNotExist = 5003, UserNotUnique = 5004,
        InvalidSearchParameters = 5005
    }
    public enum HIDResolutionAction {
        NoAction = 0,
        TryAgain = 5001
    }
    public class HIDResponse : Response {
        new public HIDErrorCode ErrorCode { get; set; }
        new public HIDResolutionAction ResolutionAction { get; set; }

        public HIDResponse() {
            this.ErrorCode = HIDErrorCode.GenericError;
            this.Success = false;
            this.Message = "Generic Error";
            this.ResolutionAction = HIDResolutionAction.NoAction;
        }
    }
}
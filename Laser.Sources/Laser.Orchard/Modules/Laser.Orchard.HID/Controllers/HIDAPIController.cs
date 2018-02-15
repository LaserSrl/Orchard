using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using System.Linq;
using System.Web.Http;

namespace Laser.Orchard.HID.Controllers {
    [WebApiKeyFilter(true)]
    public class HIDAPIController : ApiController {

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private readonly IHIDAPIService _HIDAPIService;
        private readonly IOrchardServices _orchardServices;
        private readonly IHIDAdminService _HIDAdminService;
        private readonly IHIDCredentialsService _HIDCredentialsService;
        private readonly IHIDPartNumbersService _HIDPartNumbersService;

        public HIDAPIController(
            IHIDAPIService hidapiService,
            IOrchardServices orchardService,
            IHIDAdminService HIDAdminService,
            IHIDCredentialsService HIDCredentialsService,
            IHIDPartNumbersService HIDPartNumbersService) {

            _HIDAPIService = hidapiService;
            _orchardServices = orchardService;
            _HIDAdminService = HIDAdminService;
            _HIDCredentialsService = HIDCredentialsService;
            _HIDPartNumbersService = HIDPartNumbersService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        /// <summary>
        /// Call this method to request for credentials to be issued to the credential container with the id passed.
        /// call to API/Laser.Orchard.HID/HIDAPI/IssueCredentials
        /// With the following body:
        /// {
        ///     "endpointId":"123456"
        /// }
        /// </summary>
        /// <param name="endpointInfo">The object containing the Id of the credential container to which we should try to issue credentials.</param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost, ActionName("IssueCredentials")]
        [Authorize]
        [System.Web.Mvc.OutputCache(NoStore = true)]
        public Response IssueCredentials(EndpointInfo endpoint) {
            string message = "";
            HIDErrorCode eCode = HIDErrorCode.GenericError;
            HIDResolutionAction rAction = HIDResolutionAction.NoAction;
            bool success = false;
            var endpointId = endpoint.endpointId;

            // given the authenticated user, get the hidUser
            IUser caller = _orchardServices.WorkContext.CurrentUser;
            if (caller != null) {
                var searchResult = _HIDAPIService.SearchHIDUser(caller.Email);
                if (searchResult.Error == SearchErrors.NoError) {
                    var hidUser = searchResult.User;
                    if (hidUser.Error == UserErrors.NoError) {
                        hidUser = _HIDCredentialsService.IssueCredentials(hidUser, _HIDPartNumbersService.GetPartNumbersForUser(caller), endpointId);
                        if (hidUser.Error == UserErrors.NoError) {
                            success = true;
                            eCode = HIDErrorCode.NoError;
                            message = T("Credentials issued. Synchronize your device with the TSM.").Text;
                        } else if (hidUser.Error == UserErrors.InvalidParameters) {
                            success = false;
                            eCode = HIDErrorCode.UserHasNoCredentialContainers;
                            rAction = HIDResolutionAction.NoAction;
                            message = T("There was an error with the parameters passed: endpoint ID not valid for the user. EndpointId: {0}; UserName: {1}; Email: {2}", endpoint.endpointId, caller.UserName, caller.Email).Text;
                        } else {
                            HandleHIDUserError(hidUser, caller, out eCode, out rAction, out message);
                        }
                    } else {
                        HandleHIDUserError(hidUser, caller, out eCode, out rAction, out message);
                    }
                } else {
                    HandleSearchError(searchResult, caller, out eCode, out rAction, out message);
                }
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
                response.Data = new { };
            }
            return (Response)response;
        }

        public class EndpointInfo {
            public int endpointId { get; set; }
        }


        /// <summary>
        /// Call to this method to try and create a new invitation code for the authenticated user making the call.
        /// </summary>
        /// <returns>If successfull this method returns a Response json object that contains the invitation code in
        /// Response.Data.InvitationCode.</returns>
        [System.Web.Mvc.HttpGet, ActionName("GetInvitation")]
        [Authorize]
        [System.Web.Mvc.OutputCache(NoStore = true)]
        public Response GetInvitation() {
            string message = "";
            HIDErrorCode eCode = HIDErrorCode.GenericError;
            HIDResolutionAction rAction = HIDResolutionAction.NoAction;
            bool success = false;
            string InvitationCode = "";
            IUser caller = _orchardServices.WorkContext.CurrentUser;
            var searchResult = _HIDAPIService.SearchHIDUser(caller.Email); //("patrick.negretto@laser-group.com"); // 

            if (searchResult.Error == SearchErrors.NoError) {
                var hidUser = searchResult.User;
                if (hidUser.Error == UserErrors.NoError) {
                    if (_HIDAdminService.GetSiteSettings().PreventMoreThanOneDevice
                        && hidUser.CredentialContainers.Any()) {

                        eCode = HIDErrorCode.CannotConfigureAdditionalContainer;
                        rAction = HIDResolutionAction.NoAction;
                        message = T("The user has already registered a Credential Container and is not allowed to have more. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    } else {
                        InvitationCode = hidUser.CreateInvitation();
                        if (hidUser.Error == UserErrors.NoError) {
                            success = true;
                            eCode = HIDErrorCode.NoError;
                            message = T("The Data field contains the new InvitationCode.").Text;
                        } else {
                            HandleHIDUserError(hidUser, caller, out eCode, out rAction, out message);
                        }
                    }
                } else {
                    HandleHIDUserError(hidUser, caller, out eCode, out rAction, out message);
                }
            } else {
                HandleSearchError(searchResult, caller, out eCode, out rAction, out message);
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

        private void HandleSearchError(HIDUserSearchResult searchResult, IUser caller,
            out HIDErrorCode eCode, out HIDResolutionAction rAction, out string message) {
            switch (searchResult.Error) {
                case SearchErrors.InvalidParameters:
                    eCode = HIDErrorCode.InvalidSearchParameters;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("There was an error with the parameters passed for the search. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                case SearchErrors.AuthorizationFailed:
                    eCode = HIDErrorCode.AuthenticationFailed;
                    rAction = HIDResolutionAction.NoAction;
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
        }

        private void HandleHIDUserError(HIDUser hidUser, IUser caller,
            out HIDErrorCode eCode, out HIDResolutionAction rAction, out string message) {
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
                    rAction = HIDResolutionAction.NoAction;
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
                case UserErrors.DoesNotHaveDevices:
                    eCode = HIDErrorCode.UserHasNoCredentialContainers;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("No valid credential container has been registered for the user. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                default:
                    eCode = HIDErrorCode.GenericError;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("Unknown error while searching for user on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
            }
        }

        private void HandleContainerError(HIDCredentialContainer credentialContainer, IUser caller,
            out HIDErrorCode eCode, out HIDResolutionAction rAction, out string message) {

            switch (credentialContainer.Error) {
                case CredentialErrors.UnknownError:
                    eCode = HIDErrorCode.GenericError;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("Unknown error issueing credentials to user on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
                case CredentialErrors.AuthorizationFailed:
                    eCode = HIDErrorCode.AuthenticationFailed;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("There was an error while authenticating to the HID servers. This may be a temporary condition. Please try again.").Text;
                    break;
                default:
                    eCode = HIDErrorCode.GenericError;
                    rAction = HIDResolutionAction.NoAction;
                    message = T("Unknown error issueing credentials to user on HID server. Id: {0}; UserName: {1}; Email: {2}", caller.Id, caller.UserName, caller.Email).Text;
                    break;
            }
        }
    }


    //We extend Laser.Orchard.StartupConfig.ViewModels.Response because we have specific error codes for HID
    public enum HIDErrorCode {
        NoError = 0, GenericError = 1,
        AuthenticationFailed = 5001,
        HIDServerError = 5002,
        UserDoesNotExist = 5003,
        UserNotUnique = 5004,
        InvalidSearchParameters = 5005,
        CannotConfigureAdditionalContainer = 5006,
        UserHasNoCredentialContainers = 5007
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
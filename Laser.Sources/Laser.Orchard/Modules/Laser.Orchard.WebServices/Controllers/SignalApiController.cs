using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using System;
using System.Web.Http;
using System.Web.Mvc;

namespace Laser.Orchard.WebServices.Controllers {
    
    [WebApiKeyFilter(true)]
    public class SignalApiController : ApiController {
        private readonly IActivityServices _activityServices;
        private readonly IOrchardServices _orchardServices;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IUtilsServices _utilsServices;

        public SignalApiController(IOrchardServices orchardServices, IActivityServices activityServices, ICsrfTokenHelper csrfTokenHelper, IUtilsServices utilsServices) {
            _orchardServices = orchardServices;
            _activityServices = activityServices;
            _csrfTokenHelper = csrfTokenHelper;
            _utilsServices = utilsServices;
        }

        /// <summary>
        /// Trigger the Workflow Signal defined by its Name over the specified ContentItem
        /// </summary>
        /// <param name="signal">an object representing a Signal.
        /// Example: 
        /// Request Header
        ///     Content-Type:application/x-www-form-urlencoded
        /// Request Body
        ///     Name:BookParkingPlace
        ///     ContentId:1118
        ///     ... other custom properties can be added here
        /// </param>
        /// <returns>returns a Response Object</returns>
        [OutputCache(NoStore = true, Duration = 0)]
        public Response Post([FromBody] Signal signal) {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                if (!_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                    return _utilsServices.GetResponse(ResponseType.InvalidXSRF);
                }

            }

            try {
                var response = _activityServices.TriggerSignal(signal.Name, signal.ContentId);
                return response;
            } catch (Exception ex) {
                return new Response { Success = false, Message = ex.Message, ErrorCode = ErrorCode.GenericError };
            }
        }

    }

    public class Signal {
        public string Name { get; set; }
        public int ContentId { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Laser.Orchard.UserProfiler.Service;
using Laser.Orchard.UserProfiler.ViewModels;

namespace Laser.Orchard.UserProfiler.Controllers {
    public class ProfileApiController : ApiController {


        private readonly IOrchardServices _orchardServices;
        private readonly IUtilsServices _utilsServices;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IUserProfilingService _userProfilingService;

        public ProfileApiController(
            IUtilsServices utilsServices,
            IOrchardServices orchardServices,
            ICsrfTokenHelper csrfTokenHelper,
            IUserProfilingService userProfilingService) {
            _utilsServices = utilsServices;
            _orchardServices = orchardServices;
            _csrfTokenHelper = csrfTokenHelper;
            _userProfilingService = userProfilingService;
        }
        public Response Get() {
        
            return _utilsServices.GetResponse(ResponseType.Success, "", _userProfilingService.UpdateProfile(2, "test", TextSourceTypeOptions.Tag, 1));
        }

        [HttpPost]
        public Response Post([FromBody] string text) {
           
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser == null)
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            else
          //      if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken())
                return post_method(currentUser.Id, text);
         //       else
          //          return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));
        }

        [HttpPost]
        public Response Post(UpdateVM uvm) {

            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser == null)
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            else
                //      if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken())
                return post_method(currentUser.Id, uvm);
            //       else
            //          return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));
        }

        private Response post_method(int userid, string text) {
     
            return _utilsServices.GetResponse(ResponseType.Success, "", _userProfilingService.UpdateProfile(userid, text, TextSourceTypeOptions.Tag, 1));
        }
        private Response post_method(int userid, UpdateVM uvm) {

            return _utilsServices.GetResponse(ResponseType.Success, "", _userProfilingService.UpdateProfile(userid, uvm));
        }
    }
}
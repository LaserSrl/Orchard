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
            return _utilsServices.GetResponse(ResponseType.Success, "", _userProfilingService.UpdateUserProfile(2, "test", TextSourceTypeOptions.Tag, 1));
        }




        public class parameterint {
            public int id { get; set; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pint">
        /// {"id":15}</param>
        /// <returns></returns>
        [HttpPost]
        public Response PostId(parameterint pint) {
            bool saltotest=true;
            if (pint == null)
                return _utilsServices.GetResponse(ResponseType.Validation, "Unknow Parameter type");
            var currentUser = _orchardServices.WorkContext.CurrentUser;

            if (currentUser == null)
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            else {
                     if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken() || saltotest)
                {
                    if (pint.id>0)
                        return post_method(currentUser.Id, pint.id);
                    else
                        return _utilsServices.GetResponse(ResponseType.Validation,"Content not found");
                     
                }
               else       return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });

            }
        }


          public class listparameterint {
            public List<int> id { get; set; }
        }

        /// <summary>
          /// 
        /// </summary>
        /// <param name="listIds">
          /// {"id":[15,2]}</param>
        /// <returns></returns>
        [HttpPost]
        public Response PostIds(listparameterint listIds) {
            bool saltotest=true;
            if (listIds == null || listIds.id == null)
                return _utilsServices.GetResponse(ResponseType.Validation, "Unknow Parameter type");
            var currentUser = _orchardServices.WorkContext.CurrentUser;

            if (currentUser == null)
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            else {
                     if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken() || saltotest)
                {
                    return post_method(currentUser.Id, listIds.id);
                }
               else       return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });

            }
        }



        /// <summary>

        /// </summary>
        /// <param name="unknowObject">
        /// [{"Text":"w2","Type":"Tag","Count":1},{"Text":"w2","Type":"Tag","Count":1}]
        /// 2
        /// "ciao"
        /// </param>
        /// <returns></returns>
        //[HttpPost]
        //public Response Post(object unknowObject) {
        //    if (unknowObject == null)
        //        return _utilsServices.GetResponse(ResponseType.Validation, "Unknow Parameter type");
        //    var currentUser = _orchardServices.WorkContext.CurrentUser;

        //    if (currentUser == null)
        //        return _utilsServices.GetResponse(ResponseType.UnAuthorized);
        //    else {
        //        //      if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken())
        //        {
        //            parameterint pint = JsonConvert.DeserializeObject<parameterint>(unknowObject.ToString());
        //            if (pint != null)
        //                return post_method(currentUser.Id, pint.id);

        //            int? val = null;
        //            try {
        //                val = Convert.ToInt32(unknowObject);
        //            }
        //            catch { }
        //            if (val != null)
        //                return post_method(currentUser.Id, (int)val);
        //            else {
        //                string text = unknowObject as string;
        //                if (text != null)
        //                    return post_method(currentUser.Id, text);
        //                else {
        //                    List<ProfileVM> uvm = JsonConvert.DeserializeObject<List<ProfileVM>>(unknowObject.ToString());
        //                    if (uvm != null)
        //                        return post_method(currentUser.Id, uvm);
        //                    else
        //                        return _utilsServices.GetResponse(ResponseType.Validation, "Unknow Parameter type");
        //                }
        //            }
        //        }
        //    }
        //}


        #region [ post_method]
        private Response post_method(int userid, int id) {
            return _utilsServices.GetResponse(ResponseType.Success, "", _userProfilingService.UpdateUserProfile(userid, id));
        }

        private Response post_method(int userid, List<int> ids) {
            var totaloutput = new Dictionary<string, int>();
            foreach (var id in ids) {
               var partial= _userProfilingService.UpdateUserProfile(userid, id);
               foreach (var key in partial.Keys) {
                   if (totaloutput.ContainsKey(key))
                       totaloutput[key] = partial[key];
                   else
                       totaloutput.Add(key, partial[key]);
               }
            }
            return _utilsServices.GetResponse(ResponseType.Success, "", totaloutput);
        }

        //private Response post_method(int userid, string text) {

        //    return _utilsServices.GetResponse(ResponseType.Success, "", _userProfilingService.UpdateUserProfile(userid, text, TextSourceTypeOptions.Tag, 1));
        //}
        //private Response post_method(int userid, List<ProfileVM> uvm) {

        //    return _utilsServices.GetResponse(ResponseType.Success, "", _userProfilingService.UpdateUserProfile(userid, uvm));
        //}
        #endregion
    }
}
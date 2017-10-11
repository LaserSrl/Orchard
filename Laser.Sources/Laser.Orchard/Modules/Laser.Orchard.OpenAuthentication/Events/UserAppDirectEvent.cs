using System;
using System.Linq;
using System.Text;
using System.Web;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.StartupConfig.Handlers;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Users.Events;

namespace Laser.Orchard.OpenAuthentication.Events {
    [OrchardFeature("Laser.Orchard.OpenAuthentication.AppDirect")]
    public class UserAppDirectEvent : IUserEventHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IUserProviderServices _userProviderServices;
        public UserAppDirectEvent(
            IOrchardServices orchardService,
            IUserProviderServices userProviderServices) {
            _orchardServices = orchardService;
            _userProviderServices = userProviderServices;
        }

        public void LoggedOut(IUser user) {
            HttpCookie cookie = _orchardServices.WorkContext.HttpContext.Request.Cookies["oid"];
            if (cookie != null && cookie.Value != null) {
                var urltoredirect = Encoding.UTF8.GetString(Convert.FromBase64String(cookie.Value));
                _orchardServices.WorkContext.HttpContext.Response.Cookies.Remove("oid");
                cookie.Expires = DateTime.Now.AddDays(-10);
                cookie.Value = null;
                _orchardServices.WorkContext.HttpContext.Response.SetCookie(cookie);
                var baseurl = _orchardServices.WorkContext.CurrentSite.As<OpenAuthenticationSettingsPart>().AppDirectBaseUrl;
                _orchardServices.WorkContext.HttpContext.Response.Redirect(baseurl + "/applogout?openid=" + urltoredirect);
            }
        }
        public void LoggedIn(IUser user) {
            if (_userProviderServices.Get(user.Id).Any(x => x.ProviderName == "AppDirect")) {
                var urltoredirect = _orchardServices.WorkContext.HttpContext.Request.QueryString["openid"];
                var cookie = new HttpCookie("oid", Convert.ToBase64String(Encoding.UTF8.GetBytes(urltoredirect))) { // cookie salvato in base64
                    Expires = DateTime.Now.AddMonths(6)
                };
                if (_orchardServices.WorkContext.HttpContext.Response.Cookies["oid"] != null) {
                    _orchardServices.WorkContext.HttpContext.Response.Cookies.Set(cookie);
                }
                else {
                    _orchardServices.WorkContext.HttpContext.Response.Cookies.Add(cookie);
                }
            }
        }
        #region Method not implemented
        public void AccessDenied(IUser user) {

        }

        public void Approved(IUser user) {

        }

        public void ChangedPassword(IUser user) {

        }

        public void ConfirmedEmail(IUser user) {

        }

        public void Created(UserContext context) {

        }

        public void Creating(UserContext context) {

        }





        public void LoggingIn(string userNameOrEmail, string password) {

        }

        public void LogInFailed(string userNameOrEmail, string password) {

        }

        public void SentChallengeEmail(IUser user) {

        }
        #endregion
    }
}
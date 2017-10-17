using System;
using System.Linq;
using System.Net;
using Laser.Orchard.AppDirect.Models;
using Laser.Orchard.AppDirect.Services;
using Laser.Orchard.AppDirect.ViewModels;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Tasks.Scheduling;

namespace Laser.Orchard.AppDirect.Driver {
    public class AppDirectButtonDriver : ContentPartDriver<AppDirectButtonPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IAppDirectCommunication _appDirectCommunication;
        private readonly IRepository<UserTenantRecord> _repoUserTenant;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        public AppDirectButtonDriver(IOrchardServices orchardServices,
            IAppDirectCommunication appDirectCommunication,
            IRepository<UserTenantRecord> repoUserTenant,
            IScheduledTaskManager scheduledTaskManager) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
            _appDirectCommunication = appDirectCommunication;
            _repoUserTenant = repoUserTenant;
            _scheduledTaskManager = scheduledTaskManager;
        }
        public Localizer T { get; set; }
        protected override string Prefix {
            get { return "Laser.Orchard.AppDirect.AppDirectButtonPart"; }
        }

        private ButtonVM GenerateButton(RequestState state) {
            var button = new ButtonVM();
            if (state == RequestState.ToCreate) {
                button.ButtonText = T("Confirm Subscription Order").ToString();
                button.ButtonAction = "ConfirmOrder";
            }
            if (state == RequestState.ToCancel) {
                button.ButtonText = T("Cancel Subscription Order").ToString();
                button.ButtonAction = "CancelOrder";
            }
            if (state == RequestState.ToModify) {
                button.ButtonText = T("Modify Subscription Order").ToString();
                button.ButtonAction = "ModifyOrder";
            }
            if (state == RequestState.ToAssignUser) {
                button.ButtonText = T("Assign User").ToString();
                button.ButtonAction = "AssignUser";
            }
            if (state == RequestState.ToUnAssignUser) {
                button.ButtonText = T("UnAssign User").ToString();
                button.ButtonAction = "UnAssignUser";
            }
            return button;
        }
        protected override DriverResult Editor(AppDirectButtonPart part, dynamic shapeHelper) {
            string state = ((dynamic)part.ContentItem).AppDirectRequestPart.State.Value;
            var requestState = (RequestState)Enum.Parse(typeof(RequestState), state, true);
            var button = GenerateButton(requestState);
            return ContentShape("Parts_AppDirectButton",
                               () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/AppDirectButton",
                                   Model: button,
                                   Prefix: Prefix));
        }
        protected override DriverResult Editor(AppDirectButtonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var button = new ButtonVM();
            if (updater != null && _orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "ConfirmOrder") {
                button = GenerateButton(RequestState.ToCreate);
                var AccountIdentifier = _orchardServices.WorkContext.HttpContext.Request.Form["Laser.Orchard.AppDirect.AppDirectUserPart.AccountIdentifier"];
                if (!string.IsNullOrEmpty(AccountIdentifier)) {
                    bool success = false;
                    var protocol = "https";
                    if (_orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>().EnableHttpForDebug)
                        protocol = "http";
                    var request = (HttpWebRequest)WebRequest.Create(protocol+"://" + AccountIdentifier + "/OpenId/AppDirectLogOn");
                    request.Method = "HEAD";
                    try {
                        var response = (HttpWebResponse)request.GetResponse();
                        success = response.StatusCode == HttpStatusCode.OK;
                    }catch(Exception ex) {
                        updater.AddModelError("NoIdentifierError", T(ex.Message) );
                    }
                    if (!success) {
                        updater.AddModelError("NoIdentifierMessage", T(protocol + "://" + AccountIdentifier + "/OpenId/AppDirectLogOn is not available"));
                    }
                    else {
                        string outresponse;
                        var data = "{\"success\":\"true\",\"accountIdentifier\":\"" + AccountIdentifier + "\"}";
                        var uri = ((dynamic)part.ContentItem).AppDirectRequestPart.Uri.Value + "/result";
                        if (_appDirectCommunication.MakeRequestToAppdirect(uri, Method.POST, data, _orchardServices.WorkContext.HttpContext.Request.Form["AppDirectRequestPart.ProductKey.Text"], out outresponse, "", "")) {
                            button.ButtonAction = "";
                            _appDirectCommunication.WriteEvent(EventType.Output, "Post async " + data + " " + outresponse + uri);
                            part.ContentItem.As<AppDirectUserPart>().AccountIdentifier = AccountIdentifier;
                            ((dynamic)part.ContentItem).AppDirectRequestPart.State.Value = RequestState.Created.ToString();
                            ((dynamic)part.ContentItem).AppDirectRequestPart.Action.Value = "Created";
                            // part.ContentItem.As<TitlePart>().Title = RequestState.Created.ToString()+ " " + AccountIdentifier;
                            _repoUserTenant.Create(new UserTenantRecord() { UuidCreator = part.ContentItem.As<AppDirectUserPart>().UuidCreator, Email = part.ContentItem.As<AppDirectUserPart>().Email, AccountIdentifier = AccountIdentifier, Product = _orchardServices.WorkContext.HttpContext.Request.Form["AppDirectRequestPart.ProductKey.Text"], TimeStamp = DateTime.UtcNow, Enabled = true });
                             _scheduledTaskManager.CreateTask("Laser.Orchard.AppDirect.ActionExecuted", DateTime.UtcNow.AddMinutes(1), part.ContentItem);
                        }
                    }
                }
                else {
                    updater.AddModelError("NoIdentifier", T("AccountIdentifier must not be empty"));
                }

            }
            if (updater != null && _orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "CancelOrder") {
                button = GenerateButton(RequestState.ToCancel);
                var AccountIdentifier = _orchardServices.WorkContext.HttpContext.Request.Form["Laser.Orchard.AppDirect.AppDirectUserPart.AccountIdentifier"];
                if (!string.IsNullOrEmpty(AccountIdentifier)) {
                    string outresponse;
                    var data = "{\"success\":\"true\"}";
                    var uri = ((dynamic)part.ContentItem).AppDirectRequestPart.Uri.Value + "/result";
                    if (_appDirectCommunication.MakeRequestToAppdirect(uri, Method.POST, data, _orchardServices.WorkContext.HttpContext.Request.Form["AppDirectRequestPart.ProductKey.Text"], out outresponse, "", "")) {
                        button.ButtonAction = "";
                        _appDirectCommunication.WriteEvent(EventType.Output, "Post async " + data + " " + outresponse + uri);
                        //part.ContentItem.As<AppDirectUserPart>().AccountIdentifier = AccountIdentifier;
                        ((dynamic)part.ContentItem).AppDirectRequestPart.State.Value = RequestState.Cancelled.ToString();
                        ((dynamic)part.ContentItem).AppDirectRequestPart.Action.Value = "Cancelled";
                        var usertenant = _repoUserTenant.Fetch(x => x.Enabled == true && x.Email.Equals(part.ContentItem.As<AppDirectUserPart>().Email) && x.Product.Equals(_orchardServices.WorkContext.HttpContext.Request.Form["AppDirectRequestPart.ProductKey.Text"])).FirstOrDefault();
                        if (usertenant != null)
                            usertenant.Enabled = false;
                        _repoUserTenant.Update(usertenant);
                        _scheduledTaskManager.CreateTask("Laser.Orchard.AppDirect.ActionExecuted", DateTime.UtcNow.AddMinutes(1), part.ContentItem);

                    }
                }
                else {
                    updater.AddModelError("NoIdentifier", T("AccountIdentifier must not be empty"));
                }

            }
            if (updater != null && _orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "ModifyOrder") {
                button = GenerateButton(RequestState.ToModify);
                var AccountIdentifier = _orchardServices.WorkContext.HttpContext.Request.Form["Laser.Orchard.AppDirect.AppDirectUserPart.AccountIdentifier"];
                if (!string.IsNullOrEmpty(AccountIdentifier)) {
                    string outresponse;
                    var data = "{\"success\":\"true\"}";
                    var uri = ((dynamic)part.ContentItem).AppDirectRequestPart.Uri.Value + "/result";
                    if (_appDirectCommunication.MakeRequestToAppdirect(uri, Method.POST, data, _orchardServices.WorkContext.HttpContext.Request.Form["AppDirectRequestPart.ProductKey.Text"], out outresponse, "", "")) {
                        button.ButtonAction = "";
                        _appDirectCommunication.WriteEvent(EventType.Output, "Post async " + data + " " + outresponse + uri);
                        //part.ContentItem.As<AppDirectUserPart>().AccountIdentifier = AccountIdentifier;
                        ((dynamic)part.ContentItem).AppDirectRequestPart.State.Value = RequestState.Modified.ToString();
                        ((dynamic)part.ContentItem).AppDirectRequestPart.Action.Value = "Modified";
                        _scheduledTaskManager.CreateTask("Laser.Orchard.AppDirect.ActionExecuted", DateTime.UtcNow.AddMinutes(1), part.ContentItem);

                    }
                }
                else {
                    updater.AddModelError("NoIdentifier", T("AccountIdentifier must not be empty"));
                }

            }
            if (updater != null && _orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "AssignUser") {
                button = GenerateButton(RequestState.ToAssignUser);
                var AccountIdentifier = _orchardServices.WorkContext.HttpContext.Request.Form["Laser.Orchard.AppDirect.AppDirectUserPart.AccountIdentifier"];
                if (!string.IsNullOrEmpty(AccountIdentifier)) {
                    string outresponse;
                    var data = "{\"success\":\"true\"}";
                    var uri = ((dynamic)part.ContentItem).AppDirectRequestPart.Uri.Value + "/result";
                    if (_appDirectCommunication.MakeRequestToAppdirect(uri, Method.POST, data, _orchardServices.WorkContext.HttpContext.Request.Form["AppDirectRequestPart.ProductKey.Text"], out outresponse, "", "")) {
                        button.ButtonAction = "";
                        _appDirectCommunication.WriteEvent(EventType.Output, "Post async " + data + " " + outresponse + uri);
                        //part.ContentItem.As<AppDirectUserPart>().AccountIdentifier = AccountIdentifier;
                        ((dynamic)part.ContentItem).AppDirectRequestPart.State.Value = RequestState.AssignedUser.ToString();
                        ((dynamic)part.ContentItem).AppDirectRequestPart.Action.Value = "Assigned";
                        string subjectmail = ((dynamic)part.ContentItem).AppDirectRequestPart.PayloadSubject.Value;
                        ((dynamic)part.ContentItem).AppDirectRequestPart.State.Value = RequestState.AssignedUser.ToString();
                        string jsonstring = ((dynamic)part.ContentItem).AppDirectRequestPart.Request.Value;
                        _scheduledTaskManager.CreateTask("Laser.Orchard.AppDirect.ActionExecuted", DateTime.UtcNow.AddMinutes(1), part.ContentItem);

                        var json = JObject.Parse(jsonstring);
                        if ((json["payload"]).Type != JTokenType.Null) {
                            if ((json["payload"]["user"]).Type != JTokenType.Null) {
                                if ((json["payload"]["email"]).Type != JTokenType.Null) {
                                    if ((json["payload"]["user"]["uuid"]).Type != JTokenType.Null) {
                                        if ((json["payload"]["account"]).Type != JTokenType.Null) {
                                            if ((json["payload"]["account"]["accountIdentifier"]).Type != JTokenType.Null) {
                                                _repoUserTenant.Create(new UserTenantRecord() { UuidCreator = json["payload"]["user"]["uuid"].ToString(), Email = json["payload"]["email"].ToString(), AccountIdentifier = json["payload"]["account"]["accountIdentifier"].ToString(), Product = _orchardServices.WorkContext.HttpContext.Request.Form["AppDirectRequestPart.ProductKey.Text"], TimeStamp = DateTime.UtcNow, Enabled = true });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else {
                    updater.AddModelError("NoIdentifier", T("AccountIdentifier must not be empty"));
                }
            }

            if (updater != null && _orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "UnAssignUser") {
                button = GenerateButton(RequestState.ToUnAssignUser);
                var AccountIdentifier = _orchardServices.WorkContext.HttpContext.Request.Form["Laser.Orchard.AppDirect.AppDirectUserPart.AccountIdentifier"];
                if (!string.IsNullOrEmpty(AccountIdentifier)) {
                    string outresponse;
                    var data = "{\"success\":\"true\"}";
                    var uri = ((dynamic)part.ContentItem).AppDirectRequestPart.Uri.Value + "/result";
                    if (_appDirectCommunication.MakeRequestToAppdirect(uri, Method.POST, data, _orchardServices.WorkContext.HttpContext.Request.Form["AppDirectRequestPart.ProductKey.Text"], out outresponse, "", "")) {
                        button.ButtonAction = "";
                        _appDirectCommunication.WriteEvent(EventType.Output, "Post async " + data + " " + outresponse + uri);
                        //part.ContentItem.As<AppDirectUserPart>().AccountIdentifier = AccountIdentifier;
                        ((dynamic)part.ContentItem).AppDirectRequestPart.State.Value = RequestState.UnAssignedUser.ToString();
                        ((dynamic)part.ContentItem).AppDirectRequestPart.Action.Value = "Unassigned";
                        _scheduledTaskManager.CreateTask("Laser.Orchard.AppDirect.ActionExecuted", DateTime.UtcNow.AddMinutes(1), part.ContentItem);
                        string subjectmail = ((dynamic)part.ContentItem).AppDirectRequestPart.PayloadSubject.Value;
                        var usertenant = _repoUserTenant.Fetch(x => x.Enabled == true && x.Email.Equals(subjectmail) && x.Product.Equals(_orchardServices.WorkContext.HttpContext.Request.Form["AppDirectRequestPart.ProductKey.Text"])).FirstOrDefault();
                        if (usertenant != null) {
                            usertenant.Enabled = false;
                            _repoUserTenant.Update(usertenant);
                        }
                    }
                }
                else {
                    updater.AddModelError("NoIdentifier", T("AccountIdentifier must not be empty"));
                }

            }
            return ContentShape("Parts_AppDirectButton",
                               () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/AppDirectButton",
                                   Model: button,
                                   Prefix: Prefix));
        }
    }
}
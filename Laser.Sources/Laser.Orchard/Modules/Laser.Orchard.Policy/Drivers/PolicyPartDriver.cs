﻿using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Laser.Orchard.Policy.Drivers {
    public class PolicyPartDriver : ContentPartDriver<PolicyPart> {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IContentManager _contentManager;
        private readonly IPolicyServices _policyServices;

        public PolicyPartDriver(IHttpContextAccessor httpContextAccessor,
                                IWorkContextAccessor workContextAccessor,
                                IControllerContextAccessor controllerContextAccessor,
                                IContentManager contentManager,
                                IPolicyServices policyServices)
        {
            _httpContextAccessor = httpContextAccessor;
            _workContextAccessor = workContextAccessor;
            _controllerContextAccessor = controllerContextAccessor;
            _contentManager = contentManager;
            _policyServices = policyServices;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Policy"; }
        }
        protected override DriverResult Display(PolicyPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "Detail") {
                if ((String.IsNullOrWhiteSpace(_httpContextAccessor.Current().Request.QueryString["v"]))) {
                    if (part.HasPendingPolicies ?? false) {
                        var redirectUrl = String.Format("{0}{1}v={2}", _httpContextAccessor.Current().Request.RawUrl, (_httpContextAccessor.Current().Request.RawUrl.Contains("?") ? "&" : "?"), Guid.NewGuid());
                        _httpContextAccessor.Current().Response.Redirect(redirectUrl, true);
                        return null;
                    } else {
                        var redirectUrl = String.Format("{0}{1}v={2}", _httpContextAccessor.Current().Request.RawUrl, (_httpContextAccessor.Current().Request.RawUrl.Contains("?") ? "&" : "?"), "cached-content");
                        _httpContextAccessor.Current().Response.Redirect(redirectUrl, true);
                        return null;
                    }
                }
                if (part.HasPendingPolicies ?? false) {
                    var language = _workContextAccessor.GetContext().CurrentCulture;
                    UrlHelper url = new UrlHelper(_httpContextAccessor.Current().Request.RequestContext);

                    var associatedPolicies = _policyServices.GetPoliciesForContent(part);
                    var encodedAssociatedPolicies = "";
                    if (associatedPolicies != null)
                        encodedAssociatedPolicies = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", associatedPolicies)));
                    else
                        encodedAssociatedPolicies = Convert.ToBase64String(Encoding.UTF8.GetBytes(""));

                    var fullUrl = url.Action("Index", "Policies", new { area = "Laser.Orchard.Policy", lang = language, policies = encodedAssociatedPolicies, returnUrl = _httpContextAccessor.Current().Request.RawUrl });
                    var cookie = _httpContextAccessor.Current().Request.Cookies["PoliciesAnswers"];
                    if (cookie != null && cookie.Value != null) {
                        _httpContextAccessor.Current().Response.Cookies.Add(_httpContextAccessor.Current().Request.Cookies["PoliciesAnswers"]);
                    }
                    _httpContextAccessor.Current().Response.Redirect(fullUrl, true);
                }
            } else if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_Policy_SummaryAdmin",
                     () => shapeHelper.Parts_Policy_SummaryAdmin(IncludePendingPolicy: part.IncludePendingPolicy));

            }
            return null;
        }

        protected override DriverResult Editor(PolicyPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Policy_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/Policy_Edit",
                                 Model: part,
                                 Prefix: Prefix));
        }
        protected override DriverResult Editor(PolicyPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("PolicyPartError", T("PolicyPart Error"));
            }
            return Editor(part, shapeHelper);
        }
        #region [ Import/Export ]
        protected override void Exporting(PolicyPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("IncludePendingPolicy", part.IncludePendingPolicy);

            List<string> PolicyTextReferencesIdentities = new List<string>();
            if (part.PolicyTextReferences != null) {
                if (part.PolicyTextReferences.Contains("{All}"))
                    PolicyTextReferencesIdentities.Add("{All}");
                else {
                    foreach (string PolicyTextReference in part.PolicyTextReferences) {
                        int PolicyTextReferenceId = 0;
                        if (Int32.TryParse(PolicyTextReference.TrimStart('{').TrimEnd('}'), out PolicyTextReferenceId)) {
                            var contentItem = _contentManager.Get(PolicyTextReferenceId);
                            if (contentItem != null) {
                                var containerIdentity = _contentManager.GetItemMetadata(contentItem).Identity;
                                PolicyTextReferencesIdentities.Add(containerIdentity.ToString());
                            }
                        }
                    }
                }
            }

            root.SetAttributeValue("PolicyTextReferencesCsv", String.Join(",", PolicyTextReferencesIdentities.ToArray()));
        }

        protected override void Importing(PolicyPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var includePendingPolicy = IncludePendingPolicyOptions.Yes;
            List<string> policyTextReferencesList = new List<string>();
            var policyTextReferencesIdentities = root.Attribute("PolicyTextReferencesCsv").Value;

            if (policyTextReferencesIdentities != null) {
                if (policyTextReferencesIdentities.Contains("{All}"))
                    policyTextReferencesList.Add("{All}");
                else {
                    foreach (string policyTextReferencesIdentity in policyTextReferencesIdentities.Split(',')) {
                        var contentItem = context.GetItemFromSession(policyTextReferencesIdentity);
                        if (contentItem != null)
                            policyTextReferencesList.Add("{" + contentItem.Id + "}");
                    }
                }
            }

            Enum.TryParse<IncludePendingPolicyOptions>(root.Attribute("IncludePendingPolicy").Value, out includePendingPolicy);
            part.IncludePendingPolicy = includePendingPolicy;
            part.PolicyTextReferencesCsv = String.Join(",", policyTextReferencesList.ToArray());
        }
        #endregion

    }
}
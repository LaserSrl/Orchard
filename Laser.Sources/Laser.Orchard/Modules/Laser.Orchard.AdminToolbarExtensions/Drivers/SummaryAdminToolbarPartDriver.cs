using Laser.Orchard.AdminToolbarExtensions.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Tokens;

namespace Laser.Orchard.AdminToolbarExtensions.Drivers {
    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions.SummaryAdminToolbar")]
    public class SummaryAdminToolbarPartDriver : ContentPartDriver<SummaryAdminToolbarPart> {

        private readonly ITokenizer _tokenizer;

        public SummaryAdminToolbarPartDriver(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }


        protected override DriverResult Display(SummaryAdminToolbarPart part, string displayType, dynamic shapeHelper) {

            var barSettings = part.Settings.GetModel<SummaryAdminToolbarPartSettings>();
            string toParse = "";
            if (part.Settings.TryGetValue("SummaryAdminToolbarPartSettings.Labels", out toParse)) {
                barSettings.ParseStringToList(toParse);
            }
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            foreach (var lbl in barSettings.Labels) {
                //substitute tokens
                lbl.Label = _tokenizer.Replace(lbl.Label, tokens);
                lbl.Area = _tokenizer.Replace(lbl.Area, tokens);
                lbl.Controller = _tokenizer.Replace(lbl.Controller, tokens);
                lbl.Action = _tokenizer.Replace(lbl.Action, tokens);
                lbl.Parameters = _tokenizer.Replace(lbl.Parameters, tokens);
                lbl.CustomUrl = _tokenizer.Replace(lbl.CustomUrl, tokens);
            }
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_SummaryAdminToolbarPart_SummaryAdmin",
                    () => shapeHelper.Parts_SummaryAdminToolbarPart_SummaryAdmin(Toolbar: barSettings, cIId: part.ContentItem.Id));
            }
            return new DriverResult();//base.Display(part, displayType, shapeHelper);
        }
    }
}
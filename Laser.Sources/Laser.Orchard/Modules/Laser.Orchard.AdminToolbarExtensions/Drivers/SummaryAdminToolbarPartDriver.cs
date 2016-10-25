using Laser.Orchard.AdminToolbarExtensions.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Tokens;
using Orchard;
using System.Collections.Specialized;

namespace Laser.Orchard.AdminToolbarExtensions.Drivers {
    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions")]
    public class SummaryAdminToolbarPartDriver : ContentPartDriver<SummaryAdminToolbarPart> {

        private readonly ITokenizer _tokenizer;
        private readonly IOrchardServices _orchardServices;

        public SummaryAdminToolbarPartDriver(ITokenizer tokenizer, IOrchardServices orchardServices) {
            _tokenizer = tokenizer;
            _orchardServices = orchardServices;
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
                //parse information to generate the advanced search 
                //NOTE: this is super custom for the case where we need to egnerate an advanced search by appending the 
                //CustomUrl as new query parameters
                if (!String.IsNullOrWhiteSpace(lbl.CustomUrl)) {
                    //change the content of the CustomUrl string so that it has the whole url to call
                    string hString = _orchardServices.WorkContext.HttpContext.Request.Path;
                    var qs = new NameValueCollection();  //new NameValueCollection(_orchardServices.WorkContext.HttpContext.Request.QueryString);
                    string[] myQsKeys = ((string)(lbl.CustomUrl)).Split(new char[] { '&' });
                    var myQsValues = new string[myQsKeys.Length];
                    int i = 0;
                    foreach (var str in myQsKeys) {
                        //extract the values passed as parameters, and put them in a separate array
                        int ind = str.IndexOf("=");
                        myQsValues[i] = str.Substring(ind + 1);
                        myQsKeys[i++] = str.Substring(0, ind);
                    }
                    i = 0;
                    foreach (var str in myQsKeys) {
                        if (qs.Get(str) == null) {
                            qs.Add(str, myQsValues[i]);
                        } else {
                            qs[str] = myQsValues[i];
                        }
                        i++;
                    }
                    string qsString = "";
                    foreach (var k in qs.AllKeys) {
                        qsString += k + "=" + qs[k] + "&";
                    }
                    string myPath = _orchardServices.WorkContext.HttpContext.Request.Path;
                    //the following condition is needed becacuse the search functionality depends on the advanced search module, but we could
                    //have a list of content items containing the SummaryAdminToolbar by going through the content type creation pages.
                    if (!myPath.Contains("Laser.Orchard.AdvancedSearch")) {
                        myPath = myPath.Replace("Contents", "Laser.Orchard.AdvancedSearch");
                        //in this case we should also remove the content type from the end of the path, otherwise it would carry over as a filter
                        //to the advanced search module, possibly preventing any result to be displayed.
                        //myPath = myPath.Substring(0, myPath.IndexOf("/List") + 5);
                    }
                    //we want to also reset the filter for content-type. That filter, rather than being in the querystring, is in the path
                    myPath = myPath.Substring(0, myPath.IndexOf("/List") + 5);
                    lbl.CustomUrl = myPath + "?" + qsString;
                }
            }
            
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_SummaryAdminToolbarPart_SummaryAdmin",
                    () => shapeHelper.Parts_SummaryAdminToolbarPart_SummaryAdmin(Toolbar: barSettings, cIId: part.ContentItem.Id));
            }
            return new DriverResult();//base.Display(part, displayType, shapeHelper);
        }
    }
}
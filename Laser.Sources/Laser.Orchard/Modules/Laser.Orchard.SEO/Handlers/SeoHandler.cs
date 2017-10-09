using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tokens;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Orchard.DisplayManagement.Shapes;
using System.Web.Mvc;
using System.Web;

namespace Laser.Orchard.SEO.Handlers {

    public class SeoHandler : ContentHandler {
        private readonly ITokenizer _tokenizer;


        public SeoHandler(IRepository<SeoVersionRecord> repository, ITokenizer tokenizer) {

            _tokenizer = tokenizer;

            Filters.Add(StorageFilter.For(repository));

            //we initialize a date that is valid for the database.
            OnInitializing<SeoPart>((context, part) => {
                int currYear = DateTime.Now.Year;
                int currMonth = DateTime.Now.Month;
                int currDay = DateTime.Now.Day;
                part.RobotsUnavailableAfterDate = new DateTime(currYear, currMonth, currDay);
            });


            //load the presets from the settings
            OnInitialized<SeoPart>((context, part) => {
                var settings = part.Settings.GetModel<SeoPartSettings>();
                //copy presets
                part.RobotsNoIndex = settings.RobotsNoIndex;
                part.RobotsNoFollow = settings.RobotsNoFollow;
                part.RobotsNoSnippet = settings.RobotsNoSnippet;
                part.RobotsNoOdp = settings.RobotsNoOdp;
                part.RobotsNoArchive = settings.RobotsNoArchive;
                part.RobotsUnavailableAfter = settings.RobotsUnavailableAfter;
                part.RobotsNoImageIndex = settings.RobotsNoImageIndex;
                part.GoogleNoSiteLinkSearchBox = settings.GoogleNoSiteLinkSearchBox;
                part.GoogleNoTranslate = settings.GoogleNoTranslate;
            });



            OnGetDisplayShape<SeoPart>((context, part) => {
                if (context.DisplayType == "Detail") {
                    var settings = part.Settings.GetModel<SeoPartSettings>();
                    //eval text box area
                    if (!String.IsNullOrEmpty(settings.JsonLd)) {
                        var layout = (dynamic)context.Layout;
                        string script = scriptEval(settings, part);
                        var output = new { Microscript = script };
                        layout.Head.Add(context.New.SeoMicrodataScript(ScriptMicrodata: script));
                    }
                }
            });

        }

        private string scriptEval(SeoPartSettings settings, SeoPart part) {
            var tokensDictionary = new Dictionary<string, object> { { "Content", part.ContentItem } };
            Dictionary<string, string> tokenVal = new Dictionary<string, string>();
            string scriptText = settings.JsonLd;
            Regex r = new Regex(@"{(.+?)}");
            MatchCollection mc = r.Matches(scriptText);

            foreach (Match tokenMatch in mc) {
                string token = tokenMatch.ToString();
                string stringToken = _tokenizer.Replace(token, tokensDictionary);
                tokenVal[token] = stringToken;
            }

            foreach (string token in tokenVal.Keys) {
                scriptText = scriptText.Replace(token, tokenVal[token]);
            }

            return scriptText;
        }

    }

}
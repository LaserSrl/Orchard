using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Tags.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Html;
using System.Web;
using Orchard.Mvc.Extensions;
using Orchard.ContentPicker.Fields;
using Laser.Orchard.ShortLinks.Services;



namespace Laser.Orchard.CommunicationGateway.Services {
    public interface ICommunicationService : IDependency {
        string GetCampaignLink(string CampaignSource, ContentPart part);
    }

    public class CommunicationService : ICommunicationService {
        private readonly IOrchardServices _orchardServices;
        private readonly IShortLinksService _shortLinksService;
        public CommunicationService(IOrchardServices orchardServices, IShortLinksService shortLinksService) {
            _orchardServices = orchardServices;
            _shortLinksService = shortLinksService;
        }

        /// <summary>
        ///La parte sarebbe CommunicationAdvertisingPart ma nobn l'ho definita quindi passo una cosa generica (ContentPart)
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public string GetCampaignLink(string CampaignSource, ContentPart generalpart) {
            //string CampaignSource = "email";
            string shortlink = "";
            ContentPart part = (ContentPart)(((dynamic)generalpart).ContentItem.CommunicationAdvertisingPart);
            string CampaignTerm = string.Join("+", part.ContentItem.As<TagsPart>().CurrentTags.ToArray()).ToLower();
            string CampaignMedium = CampaignSource;
            string CampaignContent = part.ContentItem.As<TitlePart>().Title.ToLower();
            string CampaignName = "";
            try {
                int idCampagna = ((int[])((dynamic)part).Campaign.Ids)[0];
                CampaignName = _orchardServices.ContentManager.Get(idCampagna).As<TitlePart>().Title;
            }
            catch (Exception ex) {
                // cuomunicato non legato a campagna
            }
            string link = "";
            if (!string.IsNullOrEmpty(((dynamic)part).UrlLinked.Value)) {
                link = (string)(((dynamic)part).UrlLinked.Value);
            }
            else {
                var pickerField = ((dynamic)part).ContentLinked as ContentPickerField;
                if (pickerField != null) {
                    var firstItem = pickerField.ContentItems.FirstOrDefault();
                    if (firstItem != null) {
                       var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                       link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(firstItem));
                    }
                }
                else
                    return "";
            }

            string linkelaborated = ElaborateLink(link, CampaignSource, CampaignMedium, CampaignTerm, CampaignContent, CampaignName);
            if (!string.IsNullOrEmpty(linkelaborated)) {
                shortlink = _shortLinksService.GetShortLink(linkelaborated);
                if (string.IsNullOrEmpty(shortlink)) {
                    throw new Exception("Url Creation Failed");
                }
            }
            return shortlink;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="CampaignSource"></param>
        /// <param name="CampaignMedium"></param>
        /// <param name="CampaignTerm">Tassonomia legata al contenuto cliccato</param>
        /// <param name="CampaignContent">Used for A/B testing</param>
        /// <param name="CampaignName"></param>
        /// <returns></returns>
        private string ElaborateLink(string link, string CampaignSource = "newsletter", string CampaignMedium = "email", string CampaignTerm = "", string CampaignContent = "", string CampaignName = "") {
            var uriBuilder = new UriBuilder(link);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["id"] = "Communication";
            query["referrer"] = string.Format("utm_source%3D{0}", CampaignSource);
            query["utm_medium"] = CampaignMedium;
            query["utm_term"] = CampaignTerm;
            query["utm_content"] = CampaignContent;
            query["utm_campaign"] = CampaignName;
            uriBuilder.Query = query.ToString();
            link = uriBuilder.ToString();
            return link;
        }

    }
}
using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.ViewModels {
    public class SeoPartViewModel {
        public string TitleOverride { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public bool RobotsNoIndex { get; set; }
        public bool RobotsNoFollow { get; set; }
        public bool RobotsNoSnippet { get; set; }
        public bool RobotsNoOdp { get; set; }
        public bool RobotsNoArchive { get; set; }
        public bool RobotsUnavailableAfter { get; set; }
        public DateTime RobotsUnavailableAfterDate { get; set; }
        public bool RobotsNoImageIndex { get; set; }
        public bool GoogleNoSiteLinkSearchBox { get; set; }
        public bool GoogleNoTranslate { get; set; }

        public SeoPartViewModel() {
        }

        public SeoPartViewModel(SeoPart part, ISEOServices seoServices) {

            this.TitleOverride = part.TitleOverride;
            this.Keywords = part.Keywords;
            this.Description = part.Description;
            this.RobotsNoIndex = part.RobotsNoIndex;
            this.RobotsNoFollow = part.RobotsNoFollow;
            this.RobotsNoSnippet = part.RobotsNoSnippet;
            this.RobotsNoOdp = part.RobotsNoOdp;
            this.RobotsNoArchive = part.RobotsNoArchive;
            this.RobotsUnavailableAfter = part.RobotsUnavailableAfter;
            this.RobotsUnavailableAfterDate = seoServices.DateToLocal(part.RobotsUnavailableAfterDate);
            this.RobotsNoImageIndex = part.RobotsNoImageIndex;
            this.GoogleNoSiteLinkSearchBox = part.GoogleNoSiteLinkSearchBox;
            this.GoogleNoTranslate = part.GoogleNoTranslate;
        }

        public void UpdatePart(SeoPart part, ISEOServices seoServices) {
            part.TitleOverride = this.TitleOverride;
            part.Keywords = this.Keywords;
            part.Description = this.Description;
            part.RobotsNoIndex = this.RobotsNoIndex;
            part.RobotsNoFollow = this.RobotsNoFollow;
            part.RobotsNoSnippet = this.RobotsNoSnippet;
            part.RobotsNoOdp = this.RobotsNoOdp;
            part.RobotsNoArchive = this.RobotsNoArchive;
            part.RobotsUnavailableAfter = this.RobotsUnavailableAfter;
            part.RobotsUnavailableAfterDate = seoServices.DateToUTC(this.RobotsUnavailableAfterDate);
            part.RobotsNoImageIndex = this.RobotsNoImageIndex;
            part.GoogleNoSiteLinkSearchBox = this.GoogleNoSiteLinkSearchBox;
            part.GoogleNoTranslate = this.GoogleNoTranslate;
        }
    }
}
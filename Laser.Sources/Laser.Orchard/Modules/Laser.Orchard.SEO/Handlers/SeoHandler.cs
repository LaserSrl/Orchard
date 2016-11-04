using Laser.Orchard.SEO.Models;
using Laser.Orchard.StartupConfig.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization.Services;
using System;
using Orchard.ContentManagement.Aspects;

namespace Laser.Orchard.SEO.Handlers {

    public class SeoHandler : ContentHandler {

        

        public SeoHandler(IRepository<SeoVersionRecord> repository) {



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
            
            
        }
        
    }

}
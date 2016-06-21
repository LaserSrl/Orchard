using Laser.Orchard.SEO.Models;
using Laser.Orchard.StartupConfig.Localization;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization.Services;
using System;

namespace Laser.Orchard.SEO.Handlers {

    public class SeoHandler : ContentHandler {

        

        public SeoHandler(IRepository<SeoVersionRecord> repository) {



            Filters.Add(StorageFilter.For(repository));

            OnInitializing<SeoPart>((context, part) => {
                int currYear = DateTime.Now.Year;
                int currMonth = DateTime.Now.Month;
                int currDay = DateTime.Now.Day;
                part.RobotsUnavailableAfterDate = new DateTime(currYear, currMonth, currDay);
            });

            ////On edit, we use the local time
            //OnGetEditorShape<SeoPart>((context, part) => {
                
            //    part.RobotsUnavailableAfterDate = (DateTime)_dateServices.ConvertToLocal(part.RobotsUnavailableAfterDate);
            //});
            ////On saving, we want the UTC time
            //OnUpdated<SeoPart>((context, part) => {
                
            //    //part.RobotsUnavailableAfterDate = part.RobotsUnavailableAfterDate.Date; // (DateTime)_dateServices.ConvertToLocal(part.RobotsUnavailableAfterDate);
            //    part.RobotsUnavailableAfterDate =
            //        //(DateTime)_dateServices.ConvertFromLocal(part.RobotsUnavailableAfterDate);
            //        (DateTime)(_dateServices.ConvertFromLocalString(_dateLocalization.WriteDateLocalized(part.RobotsUnavailableAfterDate), _dateLocalization.WriteTimeLocalized(part.RobotsUnavailableAfterDate)));
            
            //});

            //OnVersioned<SeoPart>((context, part1, part2) => {
            //    part1.Description="versioned 1";
            //    part2.Description = "versioned 2";
            //});

            //On display, we keep the UTC time
            //OnGetDisplayShape<SeoPart>((context, part) => {
            //    part.RobotsUnavailableAfterDate = (DateTime)_dateServices.ConvertToLocal(part.RobotsUnavailableAfterDate);
            //});
        }
    }

}
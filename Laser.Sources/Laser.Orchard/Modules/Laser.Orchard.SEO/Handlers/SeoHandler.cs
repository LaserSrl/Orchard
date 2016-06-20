using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
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
    }
  }
}
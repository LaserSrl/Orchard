using Laser.Orchard.StartupConfig.Localization;
using Orchard;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Services {
    public interface ISEOServices : IDependency {
        DateTime DateToLocal(DateTime utcDate);
        DateTime DateToUTC(DateTime localDate);
    }

    public class SEOServices : ISEOServices{

        private readonly IDateServices _dateServices;
        private readonly IDateLocalization _dateLocalization;

        public SEOServices(IDateServices dateServices, IDateLocalization dateLocalization){
            _dateServices = dateServices;
            _dateLocalization = dateLocalization;
        }

        public DateTime DateToLocal(DateTime utcDate) {
            return (DateTime)_dateServices.ConvertToLocal(utcDate);
        }

        public DateTime DateToUTC(DateTime localDate) {
            return (DateTime)(_dateServices.ConvertFromLocalString(_dateLocalization.WriteDateLocalized(localDate), _dateLocalization.WriteTimeLocalized(localDate)));
            
        }
    }
}
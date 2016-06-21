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
        /// <summary>
        /// Get the local time from its UTC representation.
        /// </summary>
        /// <param name="utcDate">UTC-based time.</param>
        /// <returns>Local time.</returns>
        public DateTime DateToLocal(DateTime utcDate) {
            return (DateTime)_dateServices.ConvertToLocal(utcDate);
        }
        /// <summary>
        /// Get the UTC time from its local representation
        /// </summary>
        /// <param name="localDate">Local time</param>
        /// <returns>UTC-based time</returns>
        public DateTime DateToUTC(DateTime localDate) {
            return (DateTime)(_dateServices.ConvertFromLocalString(_dateLocalization.WriteDateLocalized(localDate), _dateLocalization.WriteTimeLocalized(localDate)));
            
        }
    }
}
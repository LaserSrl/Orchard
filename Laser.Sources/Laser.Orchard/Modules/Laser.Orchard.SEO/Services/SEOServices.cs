using Laser.Orchard.StartupConfig.Localization;
using Orchard;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Services {
    public interface ISEOServices : IDependency {
        DateTime DateToLocal(DateTime? utcDate);
        DateTime DateToUTC(DateTime? localDate);
    }

    public class SEOServices : ISEOServices {

        private readonly IDateServices _dateServices;
        private readonly IDateLocalization _dateLocalization;

        public SEOServices(IDateServices dateServices, IDateLocalization dateLocalization) {
            _dateServices = dateServices;
            _dateLocalization = dateLocalization;
        }
        /// <summary>
        /// Get the local time from its UTC representation.
        /// </summary>
        /// <param name="utcDate">UTC-based time.</param>
        /// <returns>Local time.</returns>
        public DateTime DateToLocal(DateTime? utcDate) {
            if (utcDate == null || utcDate.Value <= (DateTime)SqlDateTime.MinValue) {
                utcDate = SqlDateTime.MinValue.Value.AddDays(1);
            } else if (utcDate >= (DateTime)SqlDateTime.MaxValue) {
                utcDate = SqlDateTime.MaxValue.Value.Subtract(TimeSpan.FromDays(1));
            }
            return (DateTime)_dateServices.ConvertToLocal(utcDate);
        }
        /// <summary>
        /// Get the UTC time from its local representation
        /// </summary>
        /// <param name="localDate">Local time</param>
        /// <returns>UTC-based time</returns>
        public DateTime DateToUTC(DateTime? localDate) {
            if (localDate == null || localDate.Value <= (DateTime)SqlDateTime.MinValue) {
                localDate = SqlDateTime.MinValue.Value.AddDays(1);
            } else if (localDate >= (DateTime)SqlDateTime.MaxValue) {
                localDate = SqlDateTime.MaxValue.Value.Subtract(TimeSpan.FromDays(1));
            }
            return (DateTime)(_dateServices.ConvertFromLocalString(_dateLocalization.WriteDateLocalized(localDate), _dateLocalization.WriteTimeLocalized(localDate)));

        }
    }
}
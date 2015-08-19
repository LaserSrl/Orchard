using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.ContentManagement.MetaData.Builders;

namespace Laser.Orchard.Calendarizer.Settings {

    /// <summary>
    /// Settings when attaching part to a content item
    /// </summary>
    public class CalendarizerSettings {

        public CalendarizerSettings() {
            UseDateAndTime = false;
        }

        public bool UseDateAndTime { get; set; }

        public void Build(ContentTypePartDefinitionBuilder builder) {
            builder.WithSetting("CalendarizerSettings.UseDateAndTime", UseDateAndTime.ToString());
        }
    }
}
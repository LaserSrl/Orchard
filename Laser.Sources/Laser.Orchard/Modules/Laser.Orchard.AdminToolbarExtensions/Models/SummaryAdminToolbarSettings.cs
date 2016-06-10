using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.AdminToolbarExtensions.Models {
    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions.SummaryAdminToolbar")]
    public class SummaryAdminToolbarLabel {
        public string Label { get; set; } //label that will be visualized in the SummaryAdmin view
        public string Area { get; set; } //Area parameter for dynamically computed action
        public string Controller { get; set; } //the controller called for the computed action
        public string Action { get; set; } //the action that will be called
        public string Parameters { get; set; } //parameters for the action
        public string CustomUrl { get; set; } //custom url used when the SummaryAdmin link is clicked. If null or empty, try to compute the action
        public ValidLabelTargets Target { get; set; } //_blank or _self
    }

    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions.SummaryAdminToolbar")]
    public class SummaryAdminToolbarSettings {
        public List<SummaryAdminToolbarLabel> Labels;

        public SummaryAdminToolbarSettings() {
            Labels = new List<SummaryAdminToolbarLabel>();
        }
    }
}
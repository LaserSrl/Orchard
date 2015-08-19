using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Calendarizer.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Laser.Orchard.Calendarizer.Settings;
using Orchard.Localization;

namespace Laser.Orchard.Calendarizer.Drivers {
    public class SchedulerPartDriver : ContentPartDriver<SchedulerPart> {
        public Localizer T { get; set; }
        public SchedulerPartDriver() {
            T = NullLocalizer.Instance;
        }

        protected override string Prefix {
            get { return "Scheduler"; }
        }
        protected override DriverResult Display(SchedulerPart part, string displayType, dynamic shapeHelper) {
            ContentShapeResult shapeResult;
            if (displayType == "SummaryAdmin")
                shapeResult = ContentShape("Parts_Scheduler_SummaryAdmin",
                                () => shapeHelper.Parts_Scheduler_SummaryAdmin(AllDay: part.AllDay));
            else if (displayType == "Summary")
                shapeResult = ContentShape("Parts_Scheduler_Summary",
                    () => shapeHelper.Parts_Scheduler_Summary(AllDay: part.AllDay));
            else
                shapeResult = ContentShape("Parts_Scheduler",
                    () => shapeHelper.Parts_Scheduler(AllDay: part.AllDay,Settings : part.TypePartDefinition.Settings.GetModel<CalendarizerSettings>() ));
            return shapeResult;
        }
        protected override DriverResult Editor(SchedulerPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Scheduler_Edit", () => shapeHelper
                .EditorTemplate(TemplateName: "Parts/Scheduler", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(SchedulerPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("Cannotupdate", T("Cannot Update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}
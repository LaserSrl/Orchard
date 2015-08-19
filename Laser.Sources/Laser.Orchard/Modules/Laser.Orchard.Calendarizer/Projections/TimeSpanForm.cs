using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Localization;

namespace Laser.Orchard.Calendarizer.Pojections {
    public interface IFormProvider : IEventHandler {
        void Describe(dynamic context);
    }

    public class TimeSpanForm : IFormProvider {
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }

        public TimeSpanForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }
        public void Describe(dynamic context) {
            Func<IShapeFactory, dynamic> form =
                shape => {

                    var f = Shape.Form(
                        Id: "TimeSpanForm",
                        _FromDate: Shape.TextBox(
                            Id: "FromDate", Name: "FromDate",
                            Title: T("Starting from date (Days from today)"),
                            Description: T("Number of days from today."),
                            Classes: new[] { "text medium", "tokenized" }
                            ),
                        _ForDays: Shape.TextBox(
                            Id: "ForDays", Name: "ForDays",
                            Title: T("For next n-days"),
                            Description: T("Number of days to query."),
                            Classes: new[] { "text medium", "tokenized" }
                            ));
                    return f;
                };

            context.Form("TimeSpanForm", form);

        }

    }
}


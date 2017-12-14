using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Caligoo.Projections {
    public class CaligooUserQueryFilterForm : IFormProvider {
        public const string FormName = "CaligooUserQueryFilterForm";
        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }
        public CaligooUserQueryFilterForm(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            context.Form(FormName, shape => {
                var f = _shapeFactory.Form(
                    Id: FormName,
                    _UserType: _shapeFactory.FieldSet(
                        _Value: _shapeFactory.SelectList(
                            Name: "UserType",
                            Title: T("Type"),
                            Size: 1,
                            Multiple: false
                        )
                    ),
                    _IsOnline: _shapeFactory.FieldSet(
                        Title: T("Online users"),
                        _OnlineBoth: _shapeFactory.Radio(
                            Name: "IsOnline",
                            Title: T("Both online and not online users"),
                            Value: true),
                        _OnlineYes: _shapeFactory.Radio(
                            Name: "IsOnline",
                            Title: T("Online users only"),
                            Value: false),
                        _OnlineNo: _shapeFactory.Radio(
                            Name: "IsOnline",
                            Title: T("Not online users only"),
                            Value: false)
                    ),
                    _DateFilter: _shapeFactory.FieldSet(
                        Title: T("Date (dd/MM/yyyy)"),
                        _DateMin: _shapeFactory.TextBox(
                            Name: "DateMin",
                            Title: T("From"),
                            Classes: new[] { "tokenized" }
                        ),
                        _DateMax: _shapeFactory.TextBox(
                            Name: "DateMax",
                            Title: T("To"),
                            Classes: new[] { "tokenized" }
                        )
                    ),
                    _SessionDuration: _shapeFactory.FieldSet(
                        Title: T("Wi-fi session duration (seconds)"),
                        _SessionMin: _shapeFactory.TextBox(
                            Name: "SessionMin",
                            Title: T("From"),
                            Classes: new[] { "tokenized" }
                        ),
                        _SessionMax: _shapeFactory.TextBox(
                            Name: "SessionMax",
                            Title: T("To"),
                            Classes: new[] { "tokenized" }
                        )
                    ),
                    _VisitDuration: _shapeFactory.FieldSet(
                        Title: T("Visit duration (seconds)"),
                        _VisitMin: _shapeFactory.TextBox(
                            Name: "VisitMin",
                            Title: T("From"),
                            Classes: new[] { "tokenized" }
                        ),
                        _VisitMax: _shapeFactory.TextBox(
                            Name: "VisitMax",
                            Title: T("To"),
                            Classes: new[] { "tokenized" }
                        )
                    ),
                    _LocationIdList: _shapeFactory.FieldSet(
                        _Value: _shapeFactory.TextBox(
                                Name: "LocationIdList",
                                Title: T("List of Caligoo Location Id (separated by comma)"),
                                Classes: new[] { "tokenized" }
                        )
                    )
                );
                f._UserType._Value.Add(new SelectListItem { Value = "new", Text = T("New").Text });
                f._UserType._Value.Add(new SelectListItem { Value = "returning", Text = T("Returning").Text });
                return f;
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Laser.Orchard.AppDirect.Activity {
    public class SubscriptionForm : IFormProvider {

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public SubscriptionForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;

            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "ActionEmail",
                        _Type: Shape.FieldSet(
                            Title: T("Subscription Settings"),
                            _CreateTransaction: Shape.Checkbox(
                                Id: "CreateTransaction",
                                Name: "CreateTransaction",
                                Title: T("Create Transaction"),
                                Description: T("Create Transaction."),
                                Value: "CreateTransaction"
                            ),
                            _StatusTransaction: Shape.Checkbox(
                                Id: "StatusTransaction",
                                Name: "StatusTransaction",
                                Title: T("Status Transaction"),
                                Description: T("Status Transaction."),
                                Value: "StatusTransaction"
                            ),
                            _UnAssignUserTransaction: Shape.Checkbox(
                                Id: "UnAssignUserTransaction",
                                Name: "UnAssignUserTransaction",
                                Title: T("UnAssignUser Transaction"),
                                Description: T("UnAssignUser Transaction."),
                                Value: "UnAssignUserTransaction"
                            ),
                            _AssignUserTransaction: Shape.Checkbox(
                                Id: "AssignUserTransaction",
                                Name: "AssignUserTransaction",
                                Title: T("AssignUser Transaction"),
                                Description: T("AssignUser Transaction."),
                                Value: "AssignUserTransaction"
                            ),
                            _CancelTransaction: Shape.Checkbox(
                                Id: "CancelTransaction",
                                Name: "CancelTransaction",
                                Title: T("Cancel Transaction"),
                                Description: T("Cancel Transaction."),
                                Value: "CancelTransaction"
                            ),
                            _EditTransaction: Shape.Checkbox(
                                Id: "EditTransaction",
                                Name: "EditeTransaction",
                                Title: T("Edit Transaction"),
                                Description: T("Edit Transaction."),
                                Value: "EditTransaction"
                            )

                        )
                        );
                    return f;
                };
            context.Form("SubscriptionAppDirectForm", form);
        }
    }

    public class MailFormsValidator : IFormEventHandler {
        public Localizer T { get; set; }

        public void Building(BuildingContext context) {
        }

        public void Built(BuildingContext context) {
        }

        public void Validating(ValidatingContext context) {
        }

        public void Validated(ValidatingContext context) {
        }
    }

}
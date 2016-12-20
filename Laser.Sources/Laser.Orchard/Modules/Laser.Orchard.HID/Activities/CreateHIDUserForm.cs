using Laser.Orchard.HID.Extensions;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;

namespace Laser.Orchard.HID.Activities {
    public class CreateHIDUserForm : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public CreateHIDUserForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form = shape => {
                return Shape.Form(
                    Id: "CreateHIDUser",
                    _Type: Shape.FieldSet(
                        Title: T("User details"),
                        _User: Shape.Textbox(
                            Id: "HIDUser-IUser",
                            Name: "IUser",
                            Title: T("Krake User"),
                            Description: T("Specify the ID or the UserName of the Krake User to be used as origin. Leave blank to use the owner of the Content Item that triggered the workflow."),
                            Classes: new[] { "large", "text", "tokenized" }
                        ),
                        _Email: Shape.Textbox(
                            Id: "HIDUser-EmailAddress",
                            Name: "EMail",
                            Title: T("E-Mail"),
                            Description: T("Specify the email to be associated with the user in the HID system. Leave blank to use the email of the Krake User."),
                            Classes: new[] { "large", "text", "tokenized" }
                        ),
                        _FamilyName: Shape.Textbox(
                            Id: "HIDUser-FamilyName",
                            Name: "FamilyName",
                            Title: T("Family Name"),
                            Description: T("Specify the family name of the user."),
                            Classes: new[] { "large", "text", "tokenized" }
                        ),
                        _GivenName: Shape.Textbox(
                            Id: "HIDUser-GivenName",
                            Name: "GivenName",
                            Title: T("Given Name"),
                            Description: T("Specify the given name of the user."),
                            Classes: new[] { "large", "text", "tokenized" }
                        )
                    )
                );
            };
            context.Form(Constants.ActivityCreateHIDUserFormName, form);
        }

    }

    public class CreateHIDUserFormValidator : IFormEventHandler {
        public Localizer T { get; set; }

        public void Building(BuildingContext context) {
        }

        public void Built(BuildingContext context) {
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName != Constants.ActivityCreateHIDUserFormName) {
                return;
            }

            try {
                if (string.IsNullOrWhiteSpace(context.ValueProvider.GetValue("FamilyName").AttemptedValue)) {
                    context.ModelState.AddModelError("FamilyName", T("The user's family name is required.").Text);
                }
                if (string.IsNullOrWhiteSpace(context.ValueProvider.GetValue("GivenName").AttemptedValue)) {
                    context.ModelState.AddModelError("GivenName", T("The user's given name is required.").Text);
                }
            } catch (Exception) {
                context.ModelState.AddModelError("", T("There were issues validating the values in the form.").Text);
            }
        }

        public void Validated(ValidatingContext context) {
        }
    }
}
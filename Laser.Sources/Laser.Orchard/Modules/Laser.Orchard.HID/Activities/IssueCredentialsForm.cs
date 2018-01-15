using Laser.Orchard.HID.Extensions;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;

namespace Laser.Orchard.HID.Activities {
    public class IssueCredentialsForm : IFormProvider {

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public IssueCredentialsForm(
            IShapeFactory shapeFactory) {

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form = shape => {
                return Shape.Form(
                    Id: "IssueCredentials",
                    _Type: Shape.FieldSet(
                        Title: T("Issue Credentials"),
                        _User: Shape.Textbox(
                            Id: "IssueCredentials-User",
                            Name: "IUser",
                            Title: T("Krake User"),
                            Description: T("Specify the ID or the UserName of the Krake User to be used as origin. Leave blank to use the Creator of the Content Item that triggered the workflow."),
                            Classes: new[] { "large", "text", "tokenized" }
                            ),
                        _PartNumbers: Shape.Textarea(
                            Id: "IssueCredentails-PartNumbers",
                            Name: "PartNumbers",
                            Title: T("Part Numbers"),
                            Description: T("Specify the Part Numbers that credentials will be released for, one per line. Leave blank to issue credentials for all configured Part Numbers."),
                            Classes: new[] { "large", "text", "tokenized" }
                            )
                        )
                    );
            };
            context.Form(Constants.ActivityIssueCredentialsFormName, form);
        }
    }


}
using Orchard.DisplayManagement;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OForms = Orchard.Forms;

namespace Laser.Orchard.Questionnaires.Handlers {
    public class QuestionnaireNotAnsweredFilterForm : OForms.Services.IFormProvider {
        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }
        public const string FormName = "QuestionnaireNotAnsweredFilterForm";

        public QuestionnaireNotAnsweredFilterForm(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
            T = NullLocalizer.Instance;
        }
        public void Describe(OForms.Services.DescribeContext context) {
            // compone il form
            context.Form(FormName, shape => {
                var f = _shapeFactory.Form(
                    Id: FormName,

                     _QuestionnaireId: _shapeFactory.FieldSet(
                            Id: "questionnaireId",
                            _Reaction: _shapeFactory.TextBox(
                            Name: "QuestionnaireId",
                            Title: T("Questionnaire Id"),
                            Classes: new[] { "tokenized" }
                            )
                     )
                );
                return f;
            });
        }
    }
}
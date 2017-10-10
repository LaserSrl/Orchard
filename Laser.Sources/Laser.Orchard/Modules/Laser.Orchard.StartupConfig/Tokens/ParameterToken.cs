using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class ParameterToken : ITokenProvider {

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content")
                   .Token("Parameter:*", T("Parameter:<PartName-PropertyName>"), T("Return the property value of a part specified"));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                .Token(t => t.StartsWith("Parameter:", StringComparison.OrdinalIgnoreCase) ? t.Substring("Parameter:".Length) : null,
                    (fullToken, data) => {return FindProperty(fullToken, data, context); });

        }

        private string FindProperty(string fullToken, IContent data, EvaluateContext context) {
            string[] names = fullToken.Split('-');
            ContentItem content = data.ContentItem;

            if(names.Length < 2) {
                return "";
            }

            string partName = names[0];
            string propName = names[1];
            try {
                foreach (var part in content.Parts) {
                    string partType = part.GetType().ToString().Split('.').Last();
                    if (partType.Equals(partName, StringComparison.InvariantCultureIgnoreCase)) {
                        return part.GetType().GetProperty(propName).GetValue(part, null).ToString();
                    }
                }
            }catch {
                return "parameter error";
            }
            return "parameter not found";
        }
    }
}
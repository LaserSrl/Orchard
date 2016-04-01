using System;
using System.Linq;
using Orchard.Taxonomies.Fields;
using Orchard.Localization;
using Orchard.Tokens;
using System.Globalization;
using Orchard;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class AdditionalDateTokens : ITokenProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private DateTime _resultDate;
        private string _fullTokenName;
        public AdditionalDateTokens(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {

            context.For("Date", T("Date/time"), T("Current date/time tokens"))
                .Token("Add:*(n)", T("Add:Days|Months|Years to a date"), T("Add Days|Months|Years to a date"), "Date");
        }

        public void Evaluate(EvaluateContext context) {
            context.For<DateTime>("Date")
                .Token(
                token => token.StartsWith("Add:", StringComparison.OrdinalIgnoreCase) ? token.Substring(0, (token.IndexOf(".") > 0 ? token.IndexOf(".") : token.Length)) : "",
                    (token, d) => {
                        if (token == "") return null;
                        _fullTokenName = token;
                        var parenthesysPosition = token.IndexOf("(");
                        var function = token.Substring(0, parenthesysPosition);
                        var number = token.Substring(parenthesysPosition + 1, token.Length - parenthesysPosition - 2);
                        if (function.Equals("Add:Days", StringComparison.InvariantCultureIgnoreCase)) {
                            return d.AddDays(Convert.ToDouble(number));
                        } else if (function.Equals("Add:Months", StringComparison.InvariantCultureIgnoreCase)) {
                            return d.AddMonths(Convert.ToInt32(number));
                        } else if (function.Equals("Add:Years", StringComparison.InvariantCultureIgnoreCase)) {
                            return d.AddYears(Convert.ToInt32(number));
                        } else {
                            return null;
                        }
                    }
                )
                .Chain(_fullTokenName, "Date", (d) => d); // il nome del token passato alla chain deve essere identico al token presente nel primo parametro del Token(tokeName,...,...)
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Widgets.Services;

namespace Laser.Orchard.StartupConfig.Rules {
    public class CultureRuleProvider : IRuleProvider {
        private readonly ICultureManager _cultureManager;
        private readonly IWorkContextAccessor _workContextAccessor;


        public CultureRuleProvider(ICultureManager cultureManager, IWorkContextAccessor workContextAccessor) {
            _cultureManager = cultureManager;
            _workContextAccessor = workContextAccessor;
        }

        public void Process(RuleContext ruleContext) {
            if (!String.Equals(ruleContext.FunctionName, "lang", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var culture = ruleContext.Arguments.Cast<String>();
            var userCulture = _cultureManager.GetCurrentCulture(_workContextAccessor.GetContext().HttpContext).ToLower();


            var matches = culture.Any(c => c.ToLower() == userCulture.ToLower());
            if (matches) {
                ruleContext.Result = true;
                return;
            }

            ruleContext.Result = false;
            return;

        }
    }
}

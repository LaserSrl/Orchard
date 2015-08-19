using System;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Widgets.Services;
using Orchard.Localization.Services;

namespace Laser.Orchard.CultureLayer
{
    public class CultureRuleProvider : IRuleProvider
    {
        private readonly ICultureManager _cultureManager;
		private readonly IWorkContextAccessor _workContextAccessor;


		public CultureRuleProvider(ICultureManager cultureManager, IWorkContextAccessor workContextAccessor)
		{
			_cultureManager = cultureManager;
			_workContextAccessor = workContextAccessor;
		}

        public void Process(RuleContext ruleContext) { 
            if (!String.Equals(ruleContext.FunctionName, "lang", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

			var culture = ruleContext.Arguments.Cast<String>();
			var userCulture = _cultureManager.GetCurrentCulture(_workContextAccessor.GetContext().HttpContext).ToLower();

			
            var matches = culture.Any( c => c.ToLower()==userCulture.ToLower());
            if (matches)
            {
                ruleContext.Result = true;
                return;
            }

            ruleContext.Result = false;
            return;

        }
    }
}
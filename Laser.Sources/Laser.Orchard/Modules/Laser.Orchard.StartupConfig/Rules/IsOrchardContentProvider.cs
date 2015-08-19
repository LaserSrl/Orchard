using System;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Widgets.Services;
using Orchard.Localization.Services;
using Laser.Orchard.StartupConfig.Services;

namespace Laser.Orchard.StartupConfig.Rules {
    public class IsOrchardContentProvider : IRuleProvider {
        private readonly ICurrentContentAccessor _currentContentAccessor;


        public IsOrchardContentProvider(ICurrentContentAccessor currentContentAccessor) {
            _currentContentAccessor = currentContentAccessor;
        }

        public void Process(RuleContext ruleContext) {
            if (!String.Equals(ruleContext.FunctionName, "IsOrchardContent", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var matches = _currentContentAccessor.CurrentContentItemId.HasValue;
            if (matches) {
                ruleContext.Result = true;
                return;
            }

            ruleContext.Result = false;
            return;

        }
    }
}
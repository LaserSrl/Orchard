using Laser.Orchard.AppDirect.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Tokens;

namespace Laser.Orchard.AppDirect.Tokens {
    public class AppDirectEmail : ITokenProvider {
        public Localizer T { get; set; }
        public AppDirectEmail() {
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            context.For("Content", T("AppDirect User Email"), T("AppDirect User Email"))
                   .Token("AppDirectUserEmail", T("AppDirectUserEmail"), T("The email of user given from  AppDirect."));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                   .Token("AppDirectUserEmail", x => GetTheValue(x));
        }
        private string GetTheValue(IContent contentItem) {
            return ((dynamic)contentItem.As<AppDirectUserPart>().Email);
        }
    }
}
using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Activities {
    public class CreateHIDUserTask : Task {
        
        public Localizer T { get; set; }

        private readonly IHIDAPIService _HIDAPIService;
        private readonly IContentManager _contentManager;

        public CreateHIDUserTask(IHIDAPIService hidAPIService, IContentManager contentManager) {
            _HIDAPIService = hidAPIService;
            _contentManager = contentManager;
            
            T = NullLocalizer.Instance;
        }

        public override string Form {
            get { return Constants.ActivityCreateHIDUserFormName; }
        }

        public override string Name {
            get { return "CreateHIDUser"; }
        }

        public override LocalizedString Description {
            get { return T("Creates a user in the HID systems."); }
        }

        public override LocalizedString Category {
            get { return T("Security"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("OK"), T("NoIUser"), T("UnknownError"), T("AuthorizationFailed"), T("InvalidParameters"), T("UserExists") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {

            var userString = activityContext.GetState<string>("IUser");
            IUser user = null;
            if (string.IsNullOrWhiteSpace(userString)) {
                if (workflowContext.Content.Has<CommonPart>()) {
                    int uId = (int)(((dynamic)workflowContext.Content.As<CommonPart>()).Creator.Value);
                    user = _contentManager.Get<UserPart>(uId);
                }
            } else {
                //use the string to get the user
                int userId = 0;
                if (int.TryParse(userString, out userId)) {
                    user = _contentManager.Get<UserPart>(userId);
                } else {
                    user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.UserName == userString).List().FirstOrDefault();
                }
            }
            if (user == null) {
                yield return T("NoIUser");
            }

            var familyName = activityContext.GetState<string>("FamilyName");
            var givenName = activityContext.GetState<string>("GivenName");

            var email = activityContext.GetState<string>("EMail");
            if (string.IsNullOrWhiteSpace(email)) {
                email = user.Email;
            }

            var hidUser = _HIDAPIService.CreateHIDUser(user, familyName, givenName, email);
            switch (hidUser.Error) {
                case UserErrors.NoError:
                    yield return T("OK");
                    break;
                case UserErrors.UnknownError:
                    yield return T("UnknownError");
                    break;
                case UserErrors.AuthorizationFailed:
                    yield return T("AuthorizationFailed");
                    break;
                case UserErrors.InvalidParameters:
                    yield return T("InvalidParameters");
                    break;
                case UserErrors.PreconditionFailed:
                    yield return T("UnknownError");
                    break;
                case UserErrors.EmailNotUnique:
                    yield return T("UserExists");
                    break;
                default:
                    yield return T("UnknownError");
                    break;
            }

        }
    }
}
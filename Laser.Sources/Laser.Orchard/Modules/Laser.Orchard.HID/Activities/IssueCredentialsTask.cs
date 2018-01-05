using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Laser.Orchard.HID.Services;
using Orchard.ContentManagement;
using Laser.Orchard.HID.Extensions;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Core.Common.Models;

namespace Laser.Orchard.HID.Activities {
    public class IssueCredentialsTask : Task {


        private readonly IHIDAPIService _HIDAPIService;
        private readonly IContentManager _contentManager;

        public IssueCredentialsTask(
            IHIDAPIService hidAPIService,
            IContentManager contentManager) {

            _HIDAPIService = hidAPIService;
            _contentManager = contentManager;
        }

        public Localizer T { get; set; }

        public override string Form {
            get { return Constants.ActivityIssueCredentialsFormName; }
        }

        public override string Name {
            get { return "IssueCredentials"; }
        }

        public override LocalizedString Description {
            get { return T("Issues credentials to the user."); }
        }

        public override LocalizedString Category {
            get { return T("Security"); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {
                T("OK"),
                T("NoIUser"),
                T("UnknownError"),
                T("AuthorizationFailed"),
                T("InvalidParameters"),
                T("UserExists"),
                T("UserHasNoDevices")
            };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {

            // Get the user from the form
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

            // Get the part numbers from the form
            var pnString = activityContext.GetState<string>("PartNumbers");
            var partNumbers = string.IsNullOrWhiteSpace(pnString)
                ? new string[] { }
                : pnString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var hidUser = _HIDAPIService.IssueCredentials(user, partNumbers);
            switch (hidUser.Error) {
                case UserErrors.NoError:
                    yield return T("OK");
                    break;
                case UserErrors.UnknownError:
                    yield return T("UnknownError");
                    break;
                case UserErrors.DoesNotExist:
                    break;
                case UserErrors.AuthorizationFailed:
                    yield return T("AuthorizationFailed");
                    break;
                case UserErrors.InternalServerError:
                    break;
                case UserErrors.InvalidParameters:
                    break;
                case UserErrors.EmailNotUnique:
                    break;
                case UserErrors.PreconditionFailed:
                    break;
                case UserErrors.DoesNotHaveDevices:
                    yield return T("UserHasNoDevices");
                    break;
                default:
                    yield return T("UnknownError");
                    break;
            }

            throw new NotImplementedException();
        }

    }
}
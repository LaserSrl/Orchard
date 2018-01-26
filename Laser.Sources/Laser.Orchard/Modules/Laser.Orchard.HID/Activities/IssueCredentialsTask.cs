﻿using Laser.Orchard.HID.Extensions;
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

namespace Laser.Orchard.HID.Activities {
    public class IssueCredentialsTask : Task {

        private readonly IHIDCredentialsService _HIDCredentialsService;
        private readonly IContentManager _contentManager;
        private readonly IHIDPartNumbersService _HIDPartNumbersService;

        public IssueCredentialsTask(
            IHIDCredentialsService HIDCredentialsService,
            IContentManager contentManager,
            IHIDPartNumbersService HIDPartNumbersService) {

            _HIDCredentialsService = HIDCredentialsService;
            _contentManager = contentManager;
            _HIDPartNumbersService = HIDPartNumbersService;
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
                T("NoPartNumber"),
                T("UnknownError"),
                T("AuthorizationFailed"),
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

            // Validataion for part numbers
            if (partNumbers == null || partNumbers.Length == 0) {
                partNumbers = _HIDPartNumbersService.GetPartNumbersForUser(user);
            }
            if (partNumbers.Length == 0) {
                // No part number found configured, either in this activity's form or in the site settings
                yield return T("NoPartNumber");
            }
            
            // Issue and handle response
            var hidUser = _HIDCredentialsService.IssueCredentials(user, partNumbers);
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
                case UserErrors.DoesNotHaveDevices:
                    yield return T("UserHasNoDevices");
                    break;
                default:
                    yield return T("UnknownError");
                    break;
            }
            
        }

    }
}
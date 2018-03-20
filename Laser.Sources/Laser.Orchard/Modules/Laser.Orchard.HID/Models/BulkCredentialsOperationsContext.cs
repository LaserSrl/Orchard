using Laser.Orchard.HID.Extensions;
using Orchard.Localization;
using Orchard.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser.Orchard.HID.Models {
    public class BulkCredentialsOperationsContext {

        /// <summary>
        /// If true, IssueCredentials take priority, i.e. if for a user a part number is
        /// both in its revoke-list and issue list, it will be removed from the revoke-list.
        /// If false, the behaviour is the opposite.
        /// </summary>
        public bool PrioritizeIssue { get; private set; }

        public Dictionary<int, UserCredentialActions> UserActions { get; set; }

        public Dictionary<int, UserCredentialErrors> UserErrors { get; set; }

        public BulkCredentialsOperationsContext(bool prioritizeIssue = true) {
            PrioritizeIssue = prioritizeIssue;
            UserActions = new Dictionary<int, UserCredentialActions>();
            UserErrors = new Dictionary<int, UserCredentialErrors>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void AddRevokeAction(int userId, string partNumber) {
            if (!UserActions.ContainsKey(userId)) {
                var ua = new UserCredentialActions(userId);
                UserActions.Add(userId, ua);
            }
            UserActions[userId].AddRevoke(partNumber, PrioritizeIssue);
        }

        public void AddRevokeAction(IUser user, string partNumber) {
            AddRevokeAction(user.Id, partNumber);
        }

        public void AddRevokeAction(int userId, IEnumerable<string> partNumbers) {
            if (!UserActions.ContainsKey(userId)) {
                var ua = new UserCredentialActions(userId);
                UserActions.Add(userId, ua);
            }
            foreach (var pn in partNumbers) {
                UserActions[userId].AddRevoke(pn, PrioritizeIssue);
            }
        }

        public void AddRevokeAction(IUser user, IEnumerable<string> partNumbers) {
            AddRevokeAction(user.Id, partNumbers);
        }

        public void AddIssueAction(int userId, string partNumber) {
            if (!UserActions.ContainsKey(userId)) {
                var ua = new UserCredentialActions(userId);
                UserActions.Add(userId, ua);
            }
            UserActions[userId].AddIssue(partNumber, PrioritizeIssue);
        }

        public void AddIssueAction(IUser user, string partNumber) {
            AddIssueAction(user.Id, partNumber);
        }

        public void AddIssueAction(int userId, IEnumerable<string> partNumbers) {
            if (!UserActions.ContainsKey(userId)) {
                var ua = new UserCredentialActions(userId);
                UserActions.Add(userId, ua);
            }
            foreach (var pn in partNumbers) {
                UserActions[userId].AddIssue(pn, PrioritizeIssue);
            }
        }

        public void AddIssueAction(IUser user, IEnumerable<string> partNumbers) {
            AddIssueAction(user.Id, partNumbers);
        }

        public void AddError(IUser user, UserErrors error) {
            var userId = user.Id;
            if (!UserErrors.ContainsKey(userId)) {
                var ue = new UserCredentialErrors(user);
                UserErrors.Add(userId, ue);
            }
            UserErrors[userId].AddError(error);
        }

        public void AddError(IUser user, SearchErrors error) {
            var userId = user.Id;
            if (!UserErrors.ContainsKey(userId)) {
                var ue = new UserCredentialErrors(user);
                UserErrors.Add(userId, ue);
            }
            UserErrors[userId].AddError(error);
        }

        public void ConsolidateDictionary() {
            var usersToRemove = new List<int>();
            foreach (var ua in UserActions) {
                ua.Value.ConsolidateLists(PrioritizeIssue);
                if (!ua.Value.IssueList.Any() && !ua.Value.RevokeList.Any()) {
                    usersToRemove.Add(ua.Key);
                }
            }
            // Remove elements where both lists are empty
            foreach (var userId in usersToRemove) {
                UserActions.Remove(userId);
            }
        }

        public string ErrorSummary() {
            if (!UserErrors.Any()) { // no error
                return string.Empty;
            }

            var sr = new StringBuilder();
            foreach (var uce in UserErrors) {
                sr.AppendLine(T("Errors for user \"{0}\":", uce.Value.User.Email).Text);
                foreach (var se in uce.Value.SearchErrors) {
                    sr.AppendLine(T("\t{0}", se.ToString()).Text);
                }
                foreach (var ue in uce.Value.UserErrors) {
                    sr.AppendLine(T("\t{0}", ue.ToString()).Text);
                }
            }
            return sr.ToString();
        }

        public void PopulateFromRecords(IEnumerable<BulkCredentialsOperationsRecord> records) {

            foreach (var record in records) {
                int userId = record.UserId;
                var issueList = Helpers.NumbersStringToArray(record.SerializedIssueList);
                var revokeList = Helpers.NumbersStringToArray(record.SerializedRevokeList);
                AddIssueAction(userId, issueList);
                AddRevokeAction(userId, revokeList);
            }

            ConsolidateDictionary();
        }
        
        public class UserCredentialActions {
            public int UserId { get; private set; }
            public List<string> RevokeList { get; set; }
            public List<string> IssueList { get; set; }

            public UserCredentialActions(int userId) {
                UserId = userId;
                RevokeList = new List<string>();
                IssueList = new List<string>();
            }

            public void AddRevoke(string partNumber, bool prioritizeIssue) {
                if (prioritizeIssue) {
                    // We may only add the partnumber to the RevokeList if we are not supposed to issue it,
                    // and if we haven't added it already (no sense in having duplicates) 
                    if (!IssueList.Contains(partNumber) 
                        && !RevokeList.Contains(partNumber)) {
                        RevokeList.Add(partNumber);
                    }
                } else {
                    // Revokes have priority
                    if (!RevokeList.Contains(partNumber)) {
                        RevokeList.Add(partNumber); // avoid duplicates
                    }
                    if (IssueList.Contains(partNumber)) {
                        IssueList.Remove(partNumber);
                    }
                }
            }

            public void AddIssue(string partNumber, bool prioritizeIssue) {
                if (prioritizeIssue) {
                    // Issues have priority
                    if (!IssueList.Contains(partNumber)) {
                        IssueList.Add(partNumber); // avoid udplicates
                    }
                    if (RevokeList.Contains(partNumber)) {
                        RevokeList.Remove(partNumber);
                    }
                } else {
                    // Revokes have priority
                    // We may only add the partnumber to the IssueList if we are not supposed to revoke it,
                    // and if we haven't added it already (no sense in having duplicates) 
                    if (!RevokeList.Contains(partNumber) 
                        && !IssueList.Contains(partNumber)) {
                        IssueList.Contains(partNumber);
                    }
                }
            }

            public void ConsolidateLists(bool prioritizeIssue) {
                RevokeList = RevokeList.Distinct().ToList();
                IssueList = IssueList.Distinct().ToList();
                if (prioritizeIssue) {
                    foreach (var pn in IssueList) {
                        if (RevokeList.Contains(pn)) {
                            RevokeList.Remove(pn);
                        }
                    }
                } else {
                    foreach (var pn in RevokeList) {
                        if (IssueList.Contains(pn)) {
                            IssueList.Remove(pn);
                        }
                    }
                }
            }

            public BulkCredentialsOperationsRecord ToRecord(int taskId) {
                return new BulkCredentialsOperationsRecord {
                    TaskId = taskId,
                    UserId = this.UserId,
                    SerializedRevokeList = Helpers.NumbersArrayToString(RevokeList),
                    SerializedIssueList = Helpers.NumbersArrayToString(IssueList)
                };
            }
        }

        public class UserCredentialErrors {
            public IUser User { get; set; }
            public List<SearchErrors> SearchErrors { get; set; }
            public List<UserErrors> UserErrors { get; set; }

            public UserCredentialErrors(IUser user) {
                User = user;
                SearchErrors = new List<SearchErrors>();
                UserErrors = new List<UserErrors>();
            }

            public void AddError(UserErrors error) {
                UserErrors.Add(error);
            }

            public void AddError(SearchErrors error) {
                SearchErrors.Add(error);
            }
        }
    }
}
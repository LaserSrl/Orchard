using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Models {
    public class BulkCredentialsOperationsContext {

        /// <summary>
        /// If true, IssueCredentials take priority, i.e. if for a user a part number is
        /// both in its revoke-list and issue list, it will be removed from the revoke-list.
        /// If false, the behaviour is the opposite.
        /// </summary>
        public bool PrioritizeIssue { get; private set; }

        public Dictionary<int, UserCredentialActions> UserActions { get; set; }

        public BulkCredentialsOperationsContext(bool prioritizeIssue = true) {
            PrioritizeIssue = prioritizeIssue;
            UserActions = new Dictionary<int, UserCredentialActions>();
        }

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

        }

    }
}
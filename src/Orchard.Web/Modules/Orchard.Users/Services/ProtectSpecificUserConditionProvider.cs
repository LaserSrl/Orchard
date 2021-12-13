﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    // [OrchardFeature("AutomatedUserModeration")]?
    public class ProtectSpecificUserConditionProvider : IUserSuspensionConditionProvider {

        // Method to add conditions to the query that fetches the users that we may
        // try to suspend
        public IContentQuery<UserPart> AlterQuery(
            IContentQuery<UserPart> query) {

            // Don't fetch the users that are protected from suspension
            query = query
                .Where<ProtectUserFromSuspensionPartRecord>(pr => !pr.SaveFromSuspension);

            return query;
        }
        
        // Method to tell whether a specific user should be "saved" from suspension
        public bool UserIsProtected(UserPart userPart) {

            return userPart
                .As<ProtectUserFromSuspensionPart>()
                .SaveFromSuspension;
        }
    }
}
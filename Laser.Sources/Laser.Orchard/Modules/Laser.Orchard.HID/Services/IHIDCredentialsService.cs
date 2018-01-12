using Laser.Orchard.HID.Models;
using Orchard;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.HID.Services {
    public interface IHIDCredentialsService : IDependency {
        
        /// <summary>
        /// Issue credentials to the IUser for all the PartNumbers that are passed as parameter.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="partNumbers"></param>
        /// <returns>This always returns an HIDUser object, but we should check its Error field before using it.</returns>
        HIDUser IssueCredentials(IUser user, string[] partNumbers);

        /// <summary>
        /// Issue credentials to the HIDUser for all the PartNumbers that are passed as parameter.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="partNumbers"></param>
        /// <returns>This always returns an HIDUser object, but we should check its Error field before using it.</returns>
        HIDUser IssueCredentials(HIDUser hidUser, string[] partNumbers);
        
        /// <summary>
        /// Revoke credentials from the IUser for all the PartNumbers that are passed as parameter.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="partNumbers"></param>
        /// <returns>This always returns an HIDUser object, but we should check its Error field before using it.</returns>
        HIDUser RevokeCredentials(IUser user, string[] partNumbers);

        /// <summary>
        /// Revoke credentials from the HIDUser for all the PartNumbers that are passed as parameter.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="partNumbers"></param>
        /// <returns>This always returns an HIDUser object, but we should check its Error field before using it.</returns>
        HIDUser RevokeCredentials(HIDUser hidUser, string[] partNumbers);

        /// <summary>
        /// Processes issue/revoke credentials in bulk.
        /// </summary>
        /// <param name="context">An object that contains the information on the issue/revoke 
        /// credential operations to be performed, and those performed.</param>
        void ProcessUserCredentialActions(BulkCredentialsOperationsContext context);

        /// <summary>
        /// Schedules a task that will run as soon as possible to execute the credentials operations
        /// in the context.
        /// </summary>
        /// <param name="context">The object that contains the information on the operations ot perform.</param>
        void ScheduleCredentialActions(BulkCredentialsOperationsContext context);
    }
}

﻿using Laser.Orchard.HID.Models;
using Orchard;
using Orchard.Security;

namespace Laser.Orchard.HID.Services {
    public interface IHIDAPIService : IDependency {

        /// <summary>
        /// Searches the HIDUser corresponding to the IUser passed as parameter.
        /// </summary>
        /// <param name="user">The IUser whose corresponding HIDUser we are looking for.</param>
        /// <returns>An object describing the results of the search, that contains the HIDUser if found.</returns>
        HIDUserSearchResult SearchHIDUser(IUser user);
        /// <summary>
        /// Searches the HIDUser corresponding to the email passed as parameter.
        /// </summary>
        /// <param name="email">The email we will use as key to find the user in HID's systems.</param>
        /// <returns>An object describing the results of the search, that contains the HIDUser if found.
        /// This method fails to return the user if it was not found, or if more than one user matches the
        /// email for the search.</returns>
        HIDUserSearchResult SearchHIDUser(string email);
        /// <summary>
        /// Creates a new HIDUser on the HID servers using the information provided as parameters.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="familyName"></param>
        /// <param name="givenName"></param>
        /// <param name="email">If the email parameter is null, the method will use the IUser's email address.</param>
        /// <returns>This always returns an HIDUser object, but we should check its Error field before using it.</returns>
        HIDUser CreateHIDUser(IUser user, string familyName, string givenName, string email = null);
        /// <summary>
        /// Issue credentials to the IUser for all the PartNumbers that are in the settings.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>This always returns an HIDUser object, but we should check its Error field before using it.</returns>
        HIDUser IssueCredentials(IUser user);
        /// <summary>
        /// Issue credentials to the HIDUser for all the PartNumbers that are in the settings.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>This always returns an HIDUser object, but we should check its Error field before using it.</returns>
        HIDUser IssueCredentials(HIDUser hidUser);
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
        /// Revoke credentials from the IUser for all the PartNumbers that are in the settings.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>This always returns an HIDUser object, but we should check its Error field before using it.</returns>
        HIDUser RevokeCredentials(IUser user);
        /// <summary>
        /// Revoke credentials from the HIDUser for all the PartNumbers that are in the settings.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>This always returns an HIDUser object, but we should check its Error field before using it.</returns>
        HIDUser RevokeCredentials(HIDUser hidUser);
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

    }
}

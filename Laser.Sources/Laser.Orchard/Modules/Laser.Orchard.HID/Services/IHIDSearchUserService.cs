using Laser.Orchard.HID.Models;
using Orchard;

namespace Laser.Orchard.HID.Services {
    public interface IHIDSearchUserService : IDependency {

        /// <summary>
        /// Searches the HIDUser corresponding to the email passed as parameter.
        /// </summary>
        /// <param name="email">The email we will use as key to find the user in HID's systems.</param>
        /// <returns>An object describing the results of the search, that contains the HIDUser if found.
        /// This method fails to return the user if it was not found, or if more than one user matches the
        /// email for the search.</returns>
        HIDUserSearchResult SearchHIDUser(string email);
    }
}

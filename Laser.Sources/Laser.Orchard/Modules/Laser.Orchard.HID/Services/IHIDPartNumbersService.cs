using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.ViewModels;
using Orchard;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.HID.Services {
    public interface IHIDPartNumbersService : IDependency {

        /// <summary>
        /// Call this method as the Settings are being updated to verify Part Numbers
        /// </summary>
        /// <param name="oldNumbers">THe HIDPartNumberSets before the update.</param>
        /// <returns>A PartNumberError describing the result.</returns>
        PartNumberValidationResult TryUpdatePartNumbers(HIDSiteSettingsViewModel settings);

        /// <summary>
        /// Returns the Part Numbers associated with a given user
        /// </summary>
        /// <param name="user">The User</param>
        /// <returns>And array of strings representing the part numbers.</returns>
        string[] GetPartNumbersForUser(IUser user);

        /// <summary>
        /// Returns the Part Numbers associated with a given user
        /// </summary>
        /// <param name="hidUser">The HIDUser</param>
        /// <returns>And array of strings representing the part numbers.</returns>
        string[] GetPartNumbersForUser(HIDUser hidUser);

        /// <summary>
        /// The array of all part numbers we are controlling from the application
        /// </summary>
        string[] PartNumbers { get; }

        /// <summary>
        /// Gets all the HIDPartNumberSetViewModel that are configured in the system.
        /// </summary>
        /// <returns>The IEnumerable of all configured HIDPartNumberSetViewModel.</returns>
        /// <remarks>We return the view model rather than the model, because the former's properties
        /// are more straigthforward.</remarks>
        IEnumerable<HIDPartNumberSetViewModel> GetAllSets();

        /// <summary>
        /// Gets all the HIDPartNumberSetViewModel that are assigned to the User
        /// </summary>
        /// <param name="user">The IUser.</param>
        /// <returns>The IEnumerable of all HIDPartNumberSetViewModel assigned to the IUser.</returns>
        /// <remarks>We return the view model rather than the model, because the former's properties
        /// are more straigthforward.</remarks>
        IEnumerable<HIDPartNumberSetViewModel> GetSets(IUser user);

        /// <summary>
        /// Gets all the HIDPartNumberSetViewModel that are assigned to the User
        /// </summary>
        /// <param name="hidUser">The HIDUser.</param>
        /// <returns>The IEnumerable of all HIDPartNumberSetViewModel assigned to the HIDUser.</returns>
        /// <remarks>We return the view model rather than the model, because the former's properties
        /// are more straigthforward.</remarks>
        IEnumerable<HIDPartNumberSetViewModel> GetSets(HIDUser hidUser);
    }
}

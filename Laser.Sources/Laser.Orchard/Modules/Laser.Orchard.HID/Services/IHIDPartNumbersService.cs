using Laser.Orchard.HID.Models;
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
        PartNumberValidationResult TryUpdatePartNumbers(HIDSiteSettingsPart settings);

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
    }
}

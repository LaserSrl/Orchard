using Laser.Orchard.HID.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Attributes {
    /// <summary>
    /// Validation for sets (IEnumerables) of HIDPartNumberSet. Enforces the constraint that
    /// names should be unique and not null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PartNumberSetsValidationAttribute : ValidationAttribute {

        public override bool IsValid(object value) {
            var set = value as IEnumerable<HIDPartNumberSet>;
            if (set == null) {
                return false;
            }

            var names = set
                .Where(pns => !pns.Delete) //do not consider the ones we'll be deleting
                .Select(pns => pns.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct();

            return names.Count() == set.Count(pns => !pns.Delete);
        }
    }
}
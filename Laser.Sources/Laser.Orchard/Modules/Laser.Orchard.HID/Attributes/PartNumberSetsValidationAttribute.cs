using Laser.Orchard.HID.ViewModels;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Laser.Orchard.HID.Attributes {
    /// <summary>
    /// Validation for sets (IEnumerables) of HIDPartNumberSet. Enforces the constraint that
    /// names should be unique and not null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PartNumberSetsValidationAttribute : ValidationAttribute {


        public PartNumberSetsValidationAttribute() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        public override bool IsValid(object value) {
            var set = value as IEnumerable<HIDPartNumberSetViewModel>;
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

        public override string FormatErrorMessage(string name) {
            return T("Set names must be unique and not null.").Text;
        }
    }
}
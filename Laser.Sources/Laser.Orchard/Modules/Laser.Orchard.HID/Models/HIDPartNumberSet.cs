using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.HID.Models {
    /// <summary>
    /// This class is used to represent sets of PartNumbers to be associated to a single access.
    /// e.g. the idea here is that one object of this class represents a single building and all
    /// the PartNumbers associated with its points of entry.
    /// </summary>
    public class HIDPartNumberSet {

        public HIDPartNumberSet() {
            PartNumbers = new List<string>();
        }

        /// <summary>
        /// The name of this set.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The part numbers associated with this set
        /// </summary>
        public IEnumerable<string> PartNumbers { get; set; }
        /// <summary>
        /// Used to mark sets that we need to remove
        /// </summary>
        public bool Delete { get; set; }

        //public static string NumbersArrayToString(string[] partNumbers) {
        //    //I am putting the Length of the strings in the format, because I don't know if there are invalid characters for the Part Numbers.
        //    //Having the Length of each Part Number helps when parsing the string back into an array.
        //    return String.Join(",", partNumbers.Distinct().Select(pn => String.Format(@"{{{0}}}{1}", pn.Length.ToString(), pn)));
        //}
        //public static string[] NumbersStringToArray(string partNumbers) {
        //    if (string.IsNullOrWhiteSpace(partNumbers)) {
        //        return new string[0];
        //    }
        //    List<string> numbers = new List<string>();
        //    MatchCollection matches = Regex.Matches(partNumbers, @"([{]\d*[}])");
        //    foreach (Match match in matches) {
        //        foreach (Capture cap in match.Captures) {
        //            int pnLength = int.Parse(cap.Value.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' }));
        //            //the following check to be more confident that the sequence we found is actually that of a partNumber
        //            int commaPosition = cap.Index + cap.Length + pnLength;
        //            if (commaPosition == partNumbers.Length || partNumbers.ElementAt(commaPosition) == ',') {
        //                string partNumber = partNumbers.Substring(cap.Index + cap.Length, pnLength);
        //                numbers.Add(partNumber);
        //            }
        //        }
        //    }
        //    return numbers.Distinct().ToArray();
        //}
    }
}
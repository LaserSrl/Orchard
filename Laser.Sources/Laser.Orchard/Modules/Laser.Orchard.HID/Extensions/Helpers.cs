using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.HID.Extensions {
    public static class Helpers {

        public static string NumbersArrayToString(IEnumerable<string> partNumbers) {
            //I am putting the Length of the strings in the format, because I don't know if there are invalid characters for the Part Numbers.
            //Having the Length of each Part Number helps when parsing the string back into an array.
            return String.Join(",", partNumbers.Distinct().Select(pn => String.Format(@"{{{0}}}{1}", pn.Length.ToString(), pn)));
        }

        public static IEnumerable<string> NumbersStringToArray(string partNumbers) {
            if (string.IsNullOrWhiteSpace(partNumbers)) {
                return new string[0];
            }
            List<string> numbers = new List<string>();
            MatchCollection matches = Regex.Matches(partNumbers, @"([{]\d*[}])");
            foreach (Match match in matches) {
                foreach (Capture cap in match.Captures) {
                    int pnLength = int.Parse(cap.Value.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' }));
                    //the following check to be more confident that the sequence we found is actually that of a partNumber
                    int commaPosition = cap.Index + cap.Length + pnLength;
                    if (commaPosition == partNumbers.Length || partNumbers.ElementAt(commaPosition) == ',') {
                        string partNumber = partNumbers.Substring(cap.Index + cap.Length, pnLength);
                        numbers.Add(partNumber);
                    }
                }
            }
            return numbers.Distinct().ToList();
        }
    }
}
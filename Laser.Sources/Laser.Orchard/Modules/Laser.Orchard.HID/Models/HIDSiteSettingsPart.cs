using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Laser.Orchard.HID.Models {
    public class HIDSiteSettingsPart : ContentPart {
        public int CustomerID {
            get { return this.Retrieve(x => x.CustomerID); }
            set { this.Store(x => x.CustomerID, value); }
        }
        public bool UseTestEnvironment {
            get { return this.Retrieve(x => x.UseTestEnvironment); }
            set { this.Store(x => x.UseTestEnvironment, value); }
        }
        public string ClientID {
            get { return this.Retrieve(x => x.ClientID); }
            set { this.Store(x => x.ClientID, value); }
        }
        public string ClientSecret {
            get { return this.Retrieve(x => x.ClientSecret); }
            set { this.Store(x => x.ClientSecret, value); }
        }
        public string _partNumbers { get; set; }
        public string[] PartNumbers {
            get { return NumbersStringToArray(this.Retrieve(x => x._partNumbers)); }
            set { this.Store(x => x._partNumbers, NumbersArrayToString(value)); }
        }
        public string SerializedPartNumbers {
            get { return (PartNumbers != null && PartNumbers.Length > 0) ? String.Join(Environment.NewLine, PartNumbers) : ""; }
            set { PartNumbers = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(pn => pn.Trim()).ToArray(); }
        }


        private static string NumbersArrayToString(string[] partNumbers) {
            //I am putting the Length of the strings in the format, because I don't know if there are invalid characters for the Part Numbers.
            //Having the Length of each Part Number helps when parsing the string back into an array.
            return String.Join(",", partNumbers.Select(pn => String.Format(@"{{{0}}}{1}", pn.Length.ToString(), pn)));
        }
        private static string[] NumbersStringToArray(string partNumbers) {
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
            return numbers.ToArray();
        }
    }
}
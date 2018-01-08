﻿using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Laser.Orchard.HID.Models {
    public class HIDSiteSettingsPart : ContentPart {
        /// <summary>
        /// Numeric identifier of the company in HID's systems
        /// </summary>
        public int CustomerID {
            get { return this.Retrieve(x => x.CustomerID); }
            set { this.Store(x => x.CustomerID, value); }
        }

        public bool UseTestEnvironment {
            get { return this.Retrieve(x => x.UseTestEnvironment); }
            set { this.Store(x => x.UseTestEnvironment, value); }
        }

        /// <summary>
        /// Username of the account responsible to manage credentials.
        /// </summary>
        public string ClientID {
            get { return this.Retrieve(x => x.ClientID); }
            set { this.Store(x => x.ClientID, value); }
        }

        private readonly ComputedField<string> _clientSecret = new ComputedField<string>();
        public ComputedField<string> ClientSecretField {
            get { return _clientSecret; }
        }
        /// <summary>
        /// Password for the account responsible to manage credentials.
        /// </summary>
        public string ClientSecret {
            get { return _clientSecret.Value; }
            set { _clientSecret.Value = value; }
        }

        private IList<HIDPartNumberSet> _partNumberSets;
        public IList<HIDPartNumberSet> PartNumberSets {
            get {
                if (_partNumberSets == null) {
                    var json = Retrieve<string>("PartNumberSetsString");
                    if (json == null) {
                        return new List<HIDPartNumberSet>() { new HIDPartNumberSet { Name = "Default" } };
                    }
                    _partNumberSets = new JavaScriptSerializer().Deserialize<IList<HIDPartNumberSet>>(json);
                }
                if (!_partNumberSets.Any()) {
                    _partNumberSets.Add(new HIDPartNumberSet { Name = "Default" });
                }
                return _partNumberSets;
            }
            set {
                var json = new JavaScriptSerializer().Serialize(value);
                _partNumberSets = value;
                this.Store("PartNumberSetsString", json);
            }
        }


        public string _partNumbers { get; set; }
        /// <summary>
        /// Part numbers managed by the system.
        /// </summary>
        public string[] PartNumbers {
            get {
                return PartNumberSets
                    .Select(pns => pns.PartNumbers.ToList())
                    .Aggregate((first, second) => { first.AddRange(second); return first; })
                    .ToArray();
            }
            // get { return NumbersStringToArray(this.Retrieve(x => x._partNumbers)); }
            set { this.Store(x => x._partNumbers, NumbersArrayToString(value)); }
        }
        public string SerializedPartNumbers {
            get { return (PartNumbers != null && PartNumbers.Length > 0) ? String.Join(Environment.NewLine, PartNumbers) : ""; }
            set { PartNumbers = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(pn => pn.Trim()).ToArray(); }
        }

        public string _appVersionStrings { get; set; }
        /// <summary>
        /// These strings identify the apps connected to the system, and are used in order to avoid doing something
        /// to credential containers that may belong to a user we are managing for our application
        /// </summary>
        public string[] AppVersionStrings {
            get { return NumbersStringToArray(this.Retrieve(x => x._appVersionStrings)); }
            set { this.Store(x => x._appVersionStrings, NumbersArrayToString(value)); }
        }
        public string SerializedAppVersionStrings {
            get { return (AppVersionStrings != null && AppVersionStrings.Length > 0) ? String.Join(Environment.NewLine, AppVersionStrings) : ""; }
            set { AppVersionStrings = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(avs => avs.Trim()).ToArray(); }
        }


        private static string NumbersArrayToString(string[] partNumbers) {
            //I am putting the Length of the strings in the format, because I don't know if there are invalid characters for the Part Numbers.
            //Having the Length of each Part Number helps when parsing the string back into an array.
            return String.Join(",", partNumbers.Distinct().Select(pn => String.Format(@"{{{0}}}{1}", pn.Length.ToString(), pn)));
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
            return numbers.Distinct().ToArray();
        }


    }
}
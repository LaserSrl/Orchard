using Laser.Orchard.HID.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Laser.Orchard.HID.Models {
    public class HIDUser {
        public string Location { get; set; }
        public int Id { get; set; } //id of user in HID systems
        public string ExternalId { get; set; }
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
        public List<string> Emails { get; set; }
        public string Status { get; set; }
        public List<int> InvitationIds { get; set; }
        public List<int> CredentialContainerIds { get; set; }
        public UserErrors Error { get; private set; }

        private readonly IHIDAPIService _HIDService;

        private HIDUser(IHIDAPIService hidService) {
            _HIDService = hidService;
            Emails = new List<string>();
            InvitationIds = new List<int>();
            CredentialContainerIds = new List<int>();
        }

        public static HIDUser GetUser(IHIDAPIService hidService, string location) {
            return new HIDUser(hidService) { Location = location }.GetUser();
        }
        public HIDUser GetUser() {
            HttpWebRequest wr = HttpWebRequest.CreateHttp(Location);
            wr.Method = WebRequestMethods.Http.Get;
            wr.ContentType = "application/vnd.assaabloy.ma.credential-management-1.0+json";
            wr.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        //read the json response
                        using (var reader = new StreamReader(resp.GetResponseStream())) {
                            string respJson = reader.ReadToEnd();
                            var json = JObject.Parse(respJson);
                            Id = int.Parse(json["id"].ToString());
                            ExternalId = json["externalId"].ToString();
                            FamilyName = json["name"]["familyName"].ToString();
                            GivenName = json["name"]["givenName"].ToString();
                            Emails.AddRange(json["emails"].Children().Select(jt => jt["value"].ToString()));
                            Status = json["status"].ToString();
                            InvitationIds.AddRange(json["urn:hid:scim:api:ma:1.0:UserInvitation"].Children().Select(jt => int.Parse(jt["id"].ToString())));
                            CredentialContainerIds.AddRange(json["urn:hid:scim:api:ma:1.0:CredentialContainer"].Children().Select(jt => int.Parse(jt["id"].ToString())));
                            Error = UserErrors.NoError;
                        }
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    switch (resp.StatusCode) {
                        case HttpStatusCode.Unauthorized:
                            //TODO: do login and try again
                            Error = UserErrors.NotAuthorized;
                            break;
                        case HttpStatusCode.NotFound:
                            Error = UserErrors.DoesNotExist;
                            break;
                        default:
                            if (resp.StatusDescription.ToUpperInvariant() == "SERVER ERROR") {
                                Error = UserErrors.InternalServerError;
                            } else {
                                Error = UserErrors.UnknownError;
                            }
                            break;
                    }
                } else {
                    Error = UserErrors.UnknownError;
                }
            }
            return this;
        }
    }
}
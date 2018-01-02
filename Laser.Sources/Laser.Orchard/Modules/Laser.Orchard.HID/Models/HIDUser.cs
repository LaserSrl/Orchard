using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Services;
using Newtonsoft.Json.Linq;
using Orchard.Logging;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        public List<HIDCredentialContainer> CredentialContainers { get; set; }
        public UserErrors Error { get; set; }

        private readonly IHIDAPIService _HIDService;

        public ILogger Logger { get; set; }

        public HIDUser() {
            Emails = new List<string>();
            InvitationIds = new List<int>();
            CredentialContainers = new List<HIDCredentialContainer>();
            Error = UserErrors.UnknownError;
            Logger = NullLogger.Instance;
        }

        private HIDUser(IHIDAPIService hidService)
            : this() {
            _HIDService = hidService;
        }

        public static string GenerateExternalId(int id) {
            return Constants.LocalArea + id.ToString();
        }

        private void PopulateFromJson(JObject json, bool onlyActiveContainers = true) {
            Id = int.Parse(json["id"].ToString()); //no null-checks for required properties
            ExternalId = json["externalId"].ToString();
            FamilyName = json["name"]["familyName"].ToString();
            GivenName = json["name"]["givenName"].ToString();
            Emails.AddRange(json["emails"].Children().Select(jt => jt["value"].ToString()));
            Emails = Emails.Distinct().ToList();
            Status = json["status"] != null ? json["status"].ToString() : "";
            Location = json["meta"]["location"].ToString();
            if (json["urn:hid:scim:api:ma:1.0:UserInvitation"] != null) {
                InvitationIds.AddRange(json["urn:hid:scim:api:ma:1.0:UserInvitation"].Children().Select(jt => int.Parse(jt["id"].ToString())));
                InvitationIds = InvitationIds.Distinct().ToList();
            }
            if (json["urn:hid:scim:api:ma:1.0:CredentialContainer"] != null) {
                CredentialContainers.Clear();
                var avStrings = _HIDService.GetSiteSettings().AppVersionStrings; // used to validate our apps
                CredentialContainers.AddRange(
                    json["urn:hid:scim:api:ma:1.0:CredentialContainer"]
                    .Children()
                    .Select(jt => new HIDCredentialContainer(jt, _HIDService))
                    // we can avoid trying to maange conatiners that have been deleted, or that have not yet been initialized
                    .Where(cc => onlyActiveContainers ? cc.Status == "ACTIVE" : true) 
                    .Where(cc => avStrings.Any(avs => cc.ApplicationVersion.Contains(avs))) //validate apps
                    );
            }
            Error = UserErrors.NoError;
        }

        private void ErrorFromStatusCode(HttpStatusCode sc) {
            switch (sc) {
                case HttpStatusCode.BadRequest:
                    Error = UserErrors.InvalidParameters;
                    break;
                case HttpStatusCode.Conflict:
                    Error = UserErrors.EmailNotUnique;
                    break;
                case HttpStatusCode.InternalServerError:
                    Error = UserErrors.InternalServerError;
                    break;
                case HttpStatusCode.PreconditionFailed:
                    Error = UserErrors.PreconditionFailed;
                    break;
                case HttpStatusCode.NotFound:
                    Error = UserErrors.DoesNotExist;
                    break;
                default:
                    Error = UserErrors.UnknownError;
                    break;
            }
        }

        /// <summary>
        /// Get a specific user from HID's systems.
        /// </summary>
        /// <param name="hidService">The IHIDAPIService implementation to use.</param>
        /// <param name="location">This is the complete endpoint corresponding to the user in HID's systems.</param>
        /// <returns>The HIDUser gotten from HID's systems.</returns>
        public static HIDUser GetUser(IHIDAPIService hidService, string location) {
            return new HIDUser(hidService) { Location = location }.GetUser();
        }

        public HIDUser GetUser() {
            if (!_HIDService.VerifyAuthentication()) {
                Error = UserErrors.AuthorizationFailed;
                return this;
            }

            HttpWebRequest wr = HttpWebRequest.CreateHttp(Location);
            wr.Method = WebRequestMethods.Http.Get;
            wr.ContentType = Constants.DefaultContentType;
            wr.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        //read the json response
                        using (var reader = new StreamReader(resp.GetResponseStream())) {
                            string respJson = reader.ReadToEnd();
                            PopulateFromJson(JObject.Parse(respJson));
                        }
                    }
                }
            } catch (WebException ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)(ex.Response);
                if (resp != null) {
                    if (resp.StatusCode == HttpStatusCode.Unauthorized) {
                        // Authentication could have expired while this method was running
                        if (_HIDService.Authenticate() == AuthenticationErrors.NoError) {
                            return GetUser();
                        }
                        Error = UserErrors.AuthorizationFailed;
                    } else {
                        ErrorFromStatusCode(resp.StatusCode);
                    }
                } else {
                    Error = UserErrors.UnknownError;
                }
            } catch (Exception ex) {
                Error = UserErrors.UnknownError;
                Logger.Error(ex, "Fallback error management.");
            }
            return this;
        }

        private const string UserNameFormat = @"'name':{{ 'familyName': '{0}', 'givenName': '{1}'}}";

        private string UserNameBlock {
            get { return string.Format(UserNameFormat, 
                string.IsNullOrWhiteSpace(FamilyName) ? "FamilyName" : FamilyName, 
                string.IsNullOrWhiteSpace(GivenName) ? "GivenName" : GivenName); }
        }

        private const string UserCreateFormat = @"{{ 'schemas':[ 'urn:hid:scim:api:ma:1.0:UserAction', 'urn:ietf:params:scim:schemas:core:2.0:User' ], 'externalId': '{0}', {1}, 'emails':[ {{ {2} }} ], 'urn:hid:scim:api:ma:1.0:UserAction':{{ 'createInvitationCode':'N', 'sendInvitationEmail':'N', 'assignCredential':'N', 'partNumber':'', 'credential':'' }}, 'meta':{{ 'resourceType':'PACSUser' }} }}";

        private string CreateUserBody {
            get {
                return JObject.Parse(
                    string.Format(UserCreateFormat, 
                        ExternalId, 
                        UserNameBlock, 
                        string.Join(", ", Emails.Select(em => string.Format(@"'value':'{0}'", em.ToLowerInvariant())))))
                    .ToString();
            }
        }

        public static HIDUser CreateUser(IHIDAPIService hidService, IUser oUser, string familyName, string givenName) {
            return CreateUser(hidService, oUser.Id, familyName, givenName, oUser.Email);
        }

        public static HIDUser CreateUser(IHIDAPIService hidService, IUser oUser, string familyName, string givenName, string email) {
            return CreateUser(hidService, oUser.Id, familyName, givenName, email);
        }

        public static HIDUser CreateUser(IHIDAPIService hidService, int id, string familyName, string givenName, string email) {
            return CreateUser(hidService, GenerateExternalId(id), familyName, givenName, email);
        }

        public static HIDUser CreateUser(IHIDAPIService hidService, string extId, string familyName, string givenName, string email) {
            HIDUser user = new HIDUser(hidService) { ExternalId = extId, FamilyName = familyName, GivenName = givenName };
            user.Emails.Add(email);
            return user.CreateUser();
        }

        /// <summary>
        /// This method goes and creates the user information in HID's systems.
        /// </summary>
        /// <returns>This very user.</returns>
        public HIDUser CreateUser() {
            if (!_HIDService.VerifyAuthentication()) {
                Error = UserErrors.AuthorizationFailed;
                return this;
            }

            HttpWebRequest wr = HttpWebRequest.CreateHttp(_HIDService.UsersEndpoint);
            wr.Method = WebRequestMethods.Http.Post;
            wr.ContentType = Constants.DefaultContentType;
            wr.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);
            byte[] bodyData = Encoding.UTF8.GetBytes(CreateUserBody);
            using (Stream reqStream = wr.GetRequestStream()) {
                reqStream.Write(bodyData, 0, bodyData.Length);
            }
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.Created) {
                        using (var reader = new StreamReader(resp.GetResponseStream())) {
                            string respJson = reader.ReadToEnd();
                            // populate the properties of the current HIDUser object with the values coming in the
                            // response from HID's systems.
                            PopulateFromJson(JObject.Parse(respJson)); 
                        }
                    }
                }
            } catch (WebException ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)(ex.Response);
                if (resp != null) {
                    if (resp.StatusCode == HttpStatusCode.Unauthorized) {
                        // Authentication could have expired while this method was running
                        if (_HIDService.Authenticate() == AuthenticationErrors.NoError) {
                            return CreateUser();
                        }
                        Error = UserErrors.AuthorizationFailed;
                    } else {
                        ErrorFromStatusCode(resp.StatusCode);
                    }
                } else {
                    Error = UserErrors.UnknownError;
                }
            } catch (Exception ex) {
                Error = UserErrors.UnknownError;
                Logger.Error(ex, "Fallback error management.");
            }
            return this;
        }

        private const string InvitationCreateFormat = @"{ 'schemas':[ 'urn:hid:scim:api:ma:1.0:UserAction' ], 'urn:hid:scim:api:ma:1.0:UserAction':{ 'createInvitationCode':'Y', 'sendInvitationEmail':'N', 'assignCredential':'N', 'partNumber':'', 'credential':'' }, 'meta':{ 'resourceType':'PACSUser' } }";

        private string CreateInvitationBody {
            get { return JObject.Parse(InvitationCreateFormat).ToString(); }
        }

        public string CreateInvitation() {
            if (!_HIDService.VerifyAuthentication()) {
                Error = UserErrors.AuthorizationFailed;
                return "";
            }
            
            string invitationCode = "";

            HttpWebRequest wr = HttpWebRequest.CreateHttp(Location + "/invitation");
            wr.Method = WebRequestMethods.Http.Post;
            wr.ContentType = Constants.DefaultContentType;
            wr.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);
            byte[] bodyData = Encoding.UTF8.GetBytes(CreateInvitationBody);
            using (Stream reqStream = wr.GetRequestStream()) {
                reqStream.Write(bodyData, 0, bodyData.Length);
            }
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.Created) {
                        using (var reader = new StreamReader(resp.GetResponseStream())) {
                            string respJson = reader.ReadToEnd();
                            JObject json = JObject.Parse(respJson);
                            var invitation = json["urn:hid:scim:api:ma:1.0:UserInvitation"].Children().First();
                            InvitationIds.Add(int.Parse(invitation["id"].ToString()));
                            invitationCode = invitation["invitationCode"].ToString();
                            Error = UserErrors.NoError;
                        }
                    }
                }
            } catch (WebException ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)(ex.Response);
                if (resp != null) {
                    if (resp.StatusCode == HttpStatusCode.Unauthorized) {
                        // Authentication could have expired while this method was running
                        if (_HIDService.Authenticate() == AuthenticationErrors.NoError) {
                            return CreateInvitation();
                        }
                        Error = UserErrors.AuthorizationFailed;
                    } else {
                        ErrorFromStatusCode(resp.StatusCode);
                    }
                } else {
                    Error = UserErrors.UnknownError;
                }
            } catch (Exception ex) {
                Error = UserErrors.UnknownError;
                Logger.Error(ex, "Fallback error management.");
            }
            //a valid invitation code is in the form ABCD-EFGH-ILMN-OPQR
            //16 useful characters with an hyphen separator
            const string pattern = @"(\w){4}-(\w){4}-(\w){4}-(\w){4}";
            if (new Regex(pattern).Match(invitationCode).Success) {
                return invitationCode;
            }
            Error = UserErrors.UnknownError;
            return "";
        }

        private string IssueCredentialEndpointFormat {
            get { return string.Format(HIDAPIEndpoints.IssueCredentialEndpointFormat, _HIDService.BaseEndpoint, @"{0}"); }
        }

        private const string IssueCredentialBodyFormat = @"{{ 'schemas':[ 'urn:hid:scim:api:ma:1.0:UserAction' ], 'urn:hid:scim:api:ma:1.0:UserAction':{{ 'assignCredential':'Y', 'partNumber':'{0}', 'credential':'' }} }}";
        private string IssueCredentialBody(string pn) {
            return JObject.Parse(string.Format(IssueCredentialBodyFormat, pn)).ToString();
        }

        /// <summary>
        /// Task HID's systems with issueing a credential for the given part number
        /// </summary>
        /// <param name="partNumber">The Part Number for which we are going to issue the credential.</param>
        /// <param name="onlyLatestContainer">Tells wether to attept issueing credentials only for the most recent 
        /// container for each of the user's devices.</param>
        /// <returns></returns>
        public HIDUser IssueCredential(string partNumber, bool onlyLatestContainer = true) {
            if (!_HIDService.VerifyAuthentication()) {
                Error = UserErrors.AuthorizationFailed;
                return this;
            }

            if (CredentialContainers.Count == 0) {
                Error = UserErrors.DoesNotHaveDevices;
            }
            if (onlyLatestContainer && CredentialContainers.Count > 1) {
                //IEnumerable<T>.Distinct should preserve the ordering, but it is not actually guaranteed to
                CredentialContainers = CredentialContainers
                    .GroupBy(cc => cc.Manufacturer)
                    .SelectMany(group => {
                        return group
                            .GroupBy(cc => cc.Model)
                            .Select(sub => sub.OrderByDescending(cc => cc.Id).First());
                    }).ToList();
            }
            foreach (var credentialContainer in CredentialContainers) {
                credentialContainer.IssueCredential(partNumber, this, _HIDService);
                //error handling:
                switch (credentialContainer.Error) {
                    case CredentialErrors.NoError:
                        Error = UserErrors.NoError;
                        break;
                    case CredentialErrors.UnknownError:
                        Error = UserErrors.UnknownError;
                        break;
                    case CredentialErrors.CredentialDeliveredAlready:
                        Error = UserErrors.NoError;
                        break;
                    case CredentialErrors.AuthorizationFailed:
                        // Authentication could have expired while this method was running
                        if (_HIDService.Authenticate() == AuthenticationErrors.NoError) {
                            return IssueCredential(partNumber);
                        }
                        Error = UserErrors.AuthorizationFailed;
                        break;
                    default:
                        break;
                }
            }

            return this;
        }

        private string GetCredentialContainerEndpointFormat {
            get { return String.Format(HIDAPIEndpoints.GetCredentialContainerEndpointFormat, _HIDService.BaseEndpoint, @"{0}"); }
        }

        private string RevokeCredentialEndpointFormat {
            get { return string.Format(HIDAPIEndpoints.RevokeCredentialEndpointFormat, _HIDService.BaseEndpoint, @"{0}"); }
        }

        public HIDUser RevokeCredential(string partNumber = "") {
            if (!_HIDService.VerifyAuthentication()) {
                Error = UserErrors.AuthorizationFailed;
                return this;
            }

            foreach (var credentialContainer in CredentialContainers) {
                //TODO: move this functionality to a method of HIDCredentialContainer, such as credentialContainer.RevokeCredential(partNumber),
                // like we did for IssueCredential
                HttpWebRequest wr = HttpWebRequest.CreateHttp(string.Format(GetCredentialContainerEndpointFormat, credentialContainer.Id));
                wr.Method = WebRequestMethods.Http.Get;
                wr.ContentType = Constants.DefaultContentType;
                wr.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);
                //get this container
                try {
                    using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                        if (resp.StatusCode == HttpStatusCode.OK) {
                            using (var reader = new StreamReader(resp.GetResponseStream())) {
                                string respJson = reader.ReadToEnd();
                                JObject json = JObject.Parse(respJson);
                                credentialContainer.UpdateContainer(json["urn:hid:scim:api:ma:1.0:CredentialContainer"].Children().First(), _HIDService);
                                var credentialsToRevoke = credentialContainer.Credentials.Where(cred => cred.Status.ToUpperInvariant() != "REVOKING" && cred.Status.ToUpperInvariant() != "REVOKE_INITIATED");
                                if (!string.IsNullOrWhiteSpace(partNumber)) {
                                    credentialsToRevoke = credentialContainer.Credentials.Where(cred => cred.PartNumber == partNumber);
                                }
                                foreach (var credential in credentialsToRevoke) {
                                    //TODO: move this functionality to a method of the HIDCredential class, so we do it like credential.Revoke()
                                    HttpWebRequest wrRevoke = HttpWebRequest.CreateHttp(string.Format(RevokeCredentialEndpointFormat, credential.Id));
                                    wrRevoke.Method = "DELETE";
                                    wrRevoke.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);
                                    try {
                                        using (HttpWebResponse respRevoke = wrRevoke.GetResponse() as HttpWebResponse) {
                                            if (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK) {
                                                Error = UserErrors.NoError;
                                                credential.Status = "REVOKING";
                                            } else {
                                                Error = UserErrors.UnknownError;
                                            }
                                        }
                                    } catch (WebException ex) {
                                        HttpWebResponse respRevoke = (System.Net.HttpWebResponse)(ex.Response);
                                        if (respRevoke != null) {
                                            if (respRevoke.StatusCode == HttpStatusCode.Unauthorized) {
                                                // Authentication could have expired while this method was running
                                                if (_HIDService.Authenticate() == AuthenticationErrors.NoError) {
                                                    return RevokeCredential(partNumber);
                                                }
                                                Error = UserErrors.AuthorizationFailed;
                                            } else {
                                                ErrorFromStatusCode(respRevoke.StatusCode);
                                            }
                                            if (Error == UserErrors.PreconditionFailed) {
                                                Error = UserErrors.NoError; //we are already revoking credentials
                                            }
                                        } else {
                                            Error = UserErrors.UnknownError;
                                        }
                                    } catch (Exception ex) {
                                        Error = UserErrors.UnknownError;
                                        Logger.Error(ex, "Fallback error management.");
                                    }
                                }
                            }
                        }
                    }
                } catch (WebException ex) {
                    HttpWebResponse resp = (System.Net.HttpWebResponse)(ex.Response);
                    if (resp != null) {
                        if (resp.StatusCode == HttpStatusCode.Unauthorized) {
                            if (_HIDService.Authenticate() == AuthenticationErrors.NoError) {
                                return RevokeCredential(partNumber);
                            }
                            Error = UserErrors.AuthorizationFailed;
                        } else {
                            ErrorFromStatusCode(resp.StatusCode);
                        }
                    } else {
                        Error = UserErrors.UnknownError;
                    }
                } catch (Exception ex) {
                    Error = UserErrors.UnknownError;
                    Logger.Error(ex, "Fallback error management.");
                }
                if (Error != UserErrors.NoError && Error != UserErrors.PreconditionFailed) {
                    credentialContainer.Error = CredentialErrors.UnknownError;
                }
            }
            return this;
        }
    }
}
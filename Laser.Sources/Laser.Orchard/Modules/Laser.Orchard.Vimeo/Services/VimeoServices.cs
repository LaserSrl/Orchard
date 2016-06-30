using Laser.Orchard.Vimeo.Extensions;
using Laser.Orchard.Vimeo.Models;
using Laser.Orchard.Vimeo.ViewModels;
using Orchard;
using Orchard.Data;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Laser.Orchard.Vimeo.Services {
    public class VimeoServices : IVimeoServices {

        private readonly IRepository<VimeoSettingsPartRecord> _repository;
        private readonly IOrchardServices _orchardServices;

        public VimeoServices(IRepository<VimeoSettingsPartRecord> repository, IOrchardServices orchardServices) {
            _repository = repository;
            _orchardServices = orchardServices;
        }

        /// <summary>
        /// Creates a new entry in the db for the vimeo Settings. Only one entry may exist.
        /// </summary>
        /// <param name="aToken">The Access Token string to associate.</param>
        /// <returns><value>true</value> if it was able to create the Settings Part. <value>false</value> if it fails.</returns>
        public bool Create(VimeoSettingsPartViewModel settings) {
            //check whether there already is an entry in the db
            if (_repository.Table.Count() > 0)
                return false;

            //since there was no entry, create a new one
            _repository.Create(new VimeoSettingsPartRecord {
                AccessToken = settings.AccessToken,
                ChannelName = settings.ChannelName,
                GroupName = settings.GroupName,
                AlbumName = settings.AlbumName
            });
            return true;
        }

        /// <summary>
        /// Gets the settings corresponding to the specified Access Token
        /// </summary>
        /// <param name="aToken">The Access Token</param>
        /// <returns><value>null</value> if no entry is found with the given Access Token. The ViewModel of the settings object otherwise.</returns>
        public VimeoSettingsPartViewModel GetByToken(string aToken) {
            VimeoSettingsPartRecord rec = _repository.Get(r => r.AccessToken == aToken);
            if (rec == null)
                return null;

            return new VimeoSettingsPartViewModel {
                AccessToken = rec.AccessToken,
                ChannelName = rec.ChannelName,
                GroupName = rec.GroupName,
                AlbumName = rec.AlbumName
            };
        }

        /// <summary>
        /// Gets the existing Vimeo settings.
        /// </summary>
        /// <returns><value>null</value> if no settings were found. The settings' ViewModel if found.</returns>
        public VimeoSettingsPartViewModel Get() {
            if (_repository.Table.Count() == 0)
                return null;

            VimeoSettingsPartRecord rec = _repository.Table.FirstOrDefault();
            if (rec == null)
                return null;

            return new VimeoSettingsPartViewModel {
                AccessToken = rec.AccessToken,
                ChannelName = rec.ChannelName,
                GroupName = rec.GroupName,
                AlbumName = rec.AlbumName
            };
        }

        /// <summary>
        /// Update values in the actual settings based off what is in the ViewModel.
        /// </summary>
        /// <param name="vm">The ViewModel coming from the form.</param>
        public void UpdateSettings(VimeoSettingsPartViewModel vm) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            settings.AccessToken = vm.AccessToken;
            settings.AlbumName = vm.AlbumName;
            settings.GroupName = vm.GroupName;
            settings.ChannelName = vm.ChannelName;
        }

        /// <summary>
        /// Verifies whether the token in the ViewModel is valid by attempting an API request
        /// </summary>
        /// <param name="vm">The settings ViewModel to test.</param>
        /// <returns><value>true</value> if the access token is authenticated and valid. <value>false</value> otherwise.</returns>
        public bool TokenIsValid(VimeoSettingsPartViewModel vm) {
            return !string.IsNullOrWhiteSpace(vm.AccessToken) && this.TokenIsValid(vm.AccessToken);
        }
        /// <summary>
        /// Verifies whether the token is valid by attempting an API request
        /// </summary>
        /// <param name="aToken">The Access Token to test.</param>
        /// <returns><value>true</value> if the access token is authenticated and valid. <value>false</value> otherwise.</returns>
        public bool TokenIsValid(string aToken) {
            HttpWebRequest wr = VimeoCreateRequest(aToken, VimeoEndpoints.Me);

            bool ret = false;
            try {
                using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                    ret = resp.StatusCode == HttpStatusCode.OK;
                }
            } catch (Exception ex) {
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Verifies whether the Group Name in the ViewModel is valid by attempting an API request
        /// </summary>
        /// <param name="vm">The settings ViewModel to test.</param>
        /// <returns><value>true</value> if the authenticated user has joined the given group. <value>false</value> otherwise.</returns>
        public bool GroupIsValid(VimeoSettingsPartViewModel vm) {
            return !string.IsNullOrWhiteSpace(vm.GroupName) && this.GroupIsValid(vm.GroupName, vm.AccessToken);
        }
        /// <summary>
        /// Verifies whether the Group Name is valid by attempting an API request
        /// </summary>
        /// <param name="gName">The Group Name to test.</param>
        /// <param name="aToken">The Access Token.</param>
        /// <returns><value>true</value> if the authenticated user has joined the given group. <value>false</value> otherwise.</returns>
        public bool GroupIsValid(string gName, string aToken) {

            //we only care for the album names, so we use Vimeo's JSON filter options
            //and add "?fields=name" to the querystring
            //On top of that, we have a specific name to search.
            //we can do that by adding "query=gName" to the querystring
            string queryString = "?fields=name&query=" + gName;

            HttpWebRequest wr = VimeoCreateRequest(aToken, VimeoEndpoints.MyGroups, qString: queryString);

            bool ret = false;
            try {
                bool morePages = false;
                do {
                    using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                        if (resp.StatusCode == HttpStatusCode.OK) {
                            using (var reader = new System.IO.StreamReader(resp.GetResponseStream())) {
                                string vimeoJson = reader.ReadToEnd();
                                //The Json contains what we got back from Vimeo
                                //In general, it has paging information and data
                                //The paging information tells us how many results are there in total, and how many we got from this request.
                                //we use this information to decide whether we have to fetch more stuff from the API.
                                VimeoPager pager = JsonConvert.DeserializeObject<VimeoPager>(vimeoJson);
                                if (pager.total > 0) {
                                    //check the data to make sure that the name corresponds
                                    //check the data we have here
                                    JObject json = JObject.Parse(vimeoJson);
                                    IList<JToken> res = json["data"].Children().ToList();
                                    foreach (JToken result in res) {
                                        VimeoGroup gr = JsonConvert.DeserializeObject<VimeoGroup>(result.ToString());
                                        if (gr.name == gName) { //if the album is found, exit the do-while
                                            ret = true;
                                            break;
                                        }
                                    }
                                    if (pager.total > pager.per_page * pager.page) {
                                        morePages = true;
                                        //generate a new request
                                        string pageQuery = "page=" + (pager.page + 1).ToString();
                                        wr = VimeoCreateRequest(aToken, VimeoEndpoints.MyAlbums, qString: queryString + "&" + pageQuery);
                                    }
                                }
                            }
                        }
                    }
                } while (morePages);
            } catch (Exception ex) {
                ret = false;
            }
            return ret;
        }
        /// <summary>
        /// Verifies whether the Album Name in the ViewModel is valid by attempting an API request
        /// </summary>
        /// <param name="vm">The settings ViewModel to test.</param>
        /// <returns><value>true</value> if the authenticated user has access to the given album. <value>false</value> otherwise.</returns>
        public bool AlbumIsValid(VimeoSettingsPartViewModel vm) {
            return !string.IsNullOrWhiteSpace(vm.AlbumName) && this.AlbumIsValid(vm.AlbumName, vm.AccessToken);
        }
        /// <summary>
        /// Verifies whether the Album Name is valid by attempting an API request
        /// </summary>
        /// <param name="aName">The Album Name to test</param>
        /// <param name="aToken">The Access Token.</param>
        /// <returns><value>true</value> if the authenticated user has access to the given album. <value>false</value> otherwise.</returns>
        public bool AlbumIsValid(string aName, string aToken) {

            //we only care for the album names, so we use Vimeo's JSON filter options
            //and add "?fields=name" to the querystring
            //On top of that, we have a specific name to search.
            //we can do that by adding "query=aName" to the querystring
            string queryString = "?fields=name&query=" + aName;

            HttpWebRequest wr = VimeoCreateRequest(aToken, VimeoEndpoints.MyAlbums, qString: queryString);

            bool ret = false;
            try {
                bool morePages = false;
                do {
                    using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                        if (resp.StatusCode == HttpStatusCode.OK) {
                            using (var reader = new System.IO.StreamReader(resp.GetResponseStream())) {
                                string vimeoJson = reader.ReadToEnd();
                                //The Json contains what we got back from Vimeo
                                //In general, it has paging information and data
                                //The paging information tells us how many results are there in total, and how many we got from this request.
                                //we use this information to decide whether we have to fetch more stuff from the API.
                                VimeoPager pager = JsonConvert.DeserializeObject<VimeoPager>(vimeoJson);
                                if (pager.total > 0) {
                                    //check the data to make sure that the name corresponds
                                    //check the data we have here
                                    JObject json = JObject.Parse(vimeoJson);
                                    IList<JToken> res = json["data"].Children().ToList();
                                    foreach (JToken result in res) {
                                        VimeoAlbum al = JsonConvert.DeserializeObject<VimeoAlbum>(result.ToString());
                                        if (al.name == aName) { //if the album is found, exit the do-while
                                            ret = true;
                                            break;
                                        }
                                    }
                                    if (pager.total > pager.per_page * pager.page) {
                                        morePages = true;
                                        //generate a new request
                                        string pageQuery = "page=" + (pager.page + 1).ToString();
                                        wr = VimeoCreateRequest(aToken, VimeoEndpoints.MyAlbums, qString: queryString + "&" + pageQuery);
                                    }
                                }
                            }
                        }
                    }
                } while (morePages);
            } catch (Exception ex) {
                ret = false;
            }
            return ret;
        }
        /// <summary>
        /// Verifies whether the Channel Name in the ViewModel is valid by attempting an API request
        /// </summary>
        /// <param name="vm">The settings ViewModel to test.</param>
        /// <returns><value>true</value> if the authenticated user has access to the given channel. <value>false</value> otherwise.</returns>
        public bool ChannelIsValid(VimeoSettingsPartViewModel vm) {
            return !string.IsNullOrWhiteSpace(vm.AlbumName) && this.ChannelIsValid(vm.ChannelName, vm.AccessToken);
        }
        /// <summary>
        /// Verifies whether the Channel Name is valid by attempting an API request
        /// </summary>
        /// <param name="aName">The Channel Name to test</param>
        /// <param name="aToken">The Access Token.</param>
        /// <returns><value>true</value> if the authenticated user has access to the given Channel. <value>false</value> otherwise.</returns>
        public bool ChannelIsValid(string cName, string aToken) {

            //we only care for the album names, so we use Vimeo's JSON filter options
            //and add "?fields=name" to the querystring
            //On top of that, we have a specific name to search.
            //we can do that by adding "query=cName" to the querystring
            string queryString = "?fields=name&query=" + cName;

            HttpWebRequest wr = VimeoCreateRequest(aToken, VimeoEndpoints.MyChannels, qString: queryString);

            bool ret = false;
            try {
                bool morePages = false;
                do {
                    using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                        if (resp.StatusCode == HttpStatusCode.OK) {
                            using (var reader = new System.IO.StreamReader(resp.GetResponseStream())) {
                                string vimeoJson = reader.ReadToEnd();
                                //The Json contains what we got back from Vimeo
                                //In general, it has paging information and data
                                //The paging information tells us how many results are there in total, and how many we got from this request.
                                //we use this information to decide whether we have to fetch more stuff from the API.
                                VimeoPager pager = JsonConvert.DeserializeObject<VimeoPager>(vimeoJson);
                                if (pager.total > 0) {
                                    //check the data to make sure that the name corresponds
                                    //check the data we have here
                                    JObject json = JObject.Parse(vimeoJson);
                                    IList<JToken> res = json["data"].Children().ToList();
                                    foreach (JToken result in res) {
                                        VimeoChannel ch = JsonConvert.DeserializeObject<VimeoChannel>(result.ToString());
                                        if (ch.name == cName) { //if the album is found, exit the do-while
                                            ret = true;
                                            break;
                                        }
                                    }
                                    if (pager.total > pager.per_page * pager.page) {
                                        morePages = true;
                                        //generate a new request
                                        string pageQuery = "page=" + (pager.page + 1).ToString();
                                        wr = VimeoCreateRequest(aToken, VimeoEndpoints.MyAlbums, qString: queryString + "&" + pageQuery);
                                    }
                                }
                            }
                        }
                    }
                } while (morePages);
            } catch (Exception ex) {
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Check the quota available for upload
        /// </summary>
        /// <returns>A <type>VimeoUploadQuota</type> object containing upload quota information. Returns <value>null</value> in case of error.</returns>
        public VimeoUploadQuota CheckQuota() {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            string queryString = "?fields=upload_quota";
            HttpWebRequest wr = VimeoCreateRequest(settings.AccessToken, VimeoEndpoints.Me, qString: queryString);
            VimeoUploadQuota quotaInfo = null;
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        using (var reader = new System.IO.StreamReader(resp.GetResponseStream())) {
                            string vimeoJson = reader.ReadToEnd();
                            quotaInfo = JsonConvert.DeserializeObject<VimeoUploadQuota>(vimeoJson);
                        }
                    }
                }
            } catch (Exception ex) {
                return null;
            }
            return quotaInfo;
        }
        /// <summary>
        /// Checks the number of Bytes used of the upload quota.
        /// </summary>
        /// <returns>The number of bytes used, or <value>-1</value> in case of error</returns>
        public int UsedQuota() {
            VimeoUploadQuota quotaInfo = CheckQuota();
            return quotaInfo != null ? quotaInfo.space.used : -1;
        }
        /// <summary>
        /// Checks the number of Bytes available of the upload quota.
        /// </summary>
        /// <returns>The number of available bytes, or <value>-1</value> in case of error</returns>
        public int FreeQuota() {
            VimeoUploadQuota quotaInfo = CheckQuota();
            return quotaInfo != null ? quotaInfo.space.free : -1;
        }
        
        /// <summary>
        /// Creates a default HttpWebRequest Using the Access Token and endpoint provided. By default, the Http Method is GET.
        /// </summary>
        /// <param name="aToken">The Authorized Access Token.</param>
        /// <param name="endpoint">The API endpoint for the request.</param>
        /// <param name="method">The Http Method for the request. <default>GET</default></param>
        /// <returns>An <type>HttpWebRequest</type> object, whose header is preset to the defaults for Vimeo.</returns>
        private HttpWebRequest VimeoCreateRequest(string aToken, string endpoint, string method = WebRequestMethods.Http.Get, string qString = null) {
            Uri targetUri;
            if (string.IsNullOrWhiteSpace(qString)) {
                targetUri = new Uri(endpoint);
            } else {
                targetUri = new Uri(endpoint + qString);
            }
            HttpWebRequest wr = HttpWebRequest.CreateHttp(targetUri);
            wr.Accept = Constants.HeaderAcceptValue;
            wr.Headers.Add(HttpRequestHeader.Authorization, Constants.HeaderAuthorizationValue + aToken);
            wr.Method = method;
            return wr;
        }
    }
}
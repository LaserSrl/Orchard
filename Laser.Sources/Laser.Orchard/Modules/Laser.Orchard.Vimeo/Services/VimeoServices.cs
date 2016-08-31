using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Laser.Orchard.Vimeo.Controllers;
using Laser.Orchard.Vimeo.Extensions;
using Laser.Orchard.Vimeo.Models;
using Laser.Orchard.Vimeo.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;




namespace Laser.Orchard.Vimeo.Services {
    public class VimeoServices : IVimeoTaskServices, IVimeoAdminServices, IVimeoUploadServices, IVimeoContentServices {

        private readonly IRepository<VimeoSettingsPartRecord> _repositorySettings;
        private readonly IRepository<UploadsInProgressRecord> _repositoryUploadsInProgress;
        private readonly IRepository<UploadsCompleteRecord> _repositoryUploadsComplete;
        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IContentManager _contentManager;
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly ShellSettings _shellSettings;

        public Localizer T { get; set; }

        public VimeoServices(IRepository<VimeoSettingsPartRecord> repositorySettings,
            IRepository<UploadsInProgressRecord> repositoryUploadsInProgress,
            IRepository<UploadsCompleteRecord> repositoryUploadsComplete,
            IOrchardServices orchardServices,
            IScheduledTaskManager taskManager,
            IContentManager contentManager,
            IMediaLibraryService mediaLibraryService,
            ShellSettings shellSettings) {

            _repositorySettings = repositorySettings;
            _repositoryUploadsInProgress = repositoryUploadsInProgress;
            _repositoryUploadsComplete = repositoryUploadsComplete;
            _orchardServices = orchardServices;
            _taskManager = taskManager;
            _contentManager = contentManager;
            _mediaLibraryService = mediaLibraryService;
            _shellSettings = shellSettings;

            T = NullLocalizer.Instance;
        }

        //NOTE: These two methods are not really needed right now, because we "create" a settings object
        //for the instance in the settings handler
        /// <summary>
        /// Creates a new entry in the db for the vimeo Settings. Only one entry may exist.
        /// </summary>
        /// <param name="aToken">The Access Token string to associate.</param>
        /// <returns><value>true</value> if it was able to create the Settings Part. <value>false</value> if it fails.</returns>
        public bool Create(VimeoSettingsPartViewModel settings) {
            //check whether there already is an entry in the db
            if (_repositorySettings.Table.Count() > 0)
                return false;

            //since there was no entry, create a new one
            _repositorySettings.Create(new VimeoSettingsPartRecord {
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
            VimeoSettingsPartRecord rec = _repositorySettings.Get(r => r.AccessToken == aToken);
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
        public VimeoSettingsPartViewModel GetSettingsVM() {

            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();

            return new VimeoSettingsPartViewModel(settings);
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
            settings.AccessToken = vm.AccessToken ?? "";
            settings.AlbumName = vm.AlbumName ?? "";
            settings.GroupName = vm.GroupName ?? "";
            settings.ChannelName = vm.ChannelName ?? "";

            settings.AlwaysUploadToGroup = vm.AlwaysUploadToGroup;
            settings.AlwaysUploadToAlbum = vm.AlwaysUploadToAlbum;
            settings.AlwaysUploadToChannel = vm.AlwaysUploadToChannel;

            //verify group, channel and album.
            //if they do not exist, try to create them.
            if (TokenIsValid(settings.AccessToken)) {
                //group
                if (!string.IsNullOrWhiteSpace(settings.GroupName)) {
                    if (GroupIsValid(settings.GroupName, settings.AccessToken)) {
                        _orchardServices.Notifier.Information(T("Group Name valid"));
                    } else {
                        string res = CreateNewGroup(settings.AccessToken, settings.GroupName);
                        if (res == "OK") {
                            _orchardServices.Notifier.Information(T("Group created"));
                        } else {
                            _orchardServices.Notifier.Error(T("Failed to create group. Internal message: {0}", res));
                            settings.AlwaysUploadToGroup = false;

                        }
                    }
                }
                //channel
                if (!string.IsNullOrWhiteSpace(settings.ChannelName)) {
                    if (ChannelIsValid(settings.ChannelName, settings.AccessToken)) {
                        _orchardServices.Notifier.Information(T("Channel Name valid"));
                    } else {
                        string res = CreateNewChannel(settings.AccessToken, settings.ChannelName);
                        if (res == "OK") {
                            _orchardServices.Notifier.Information(T("Channel created"));
                        } else {
                            _orchardServices.Notifier.Error(T("Failed to create channel. Internal message: {0}", res));
                            settings.AlwaysUploadToChannel = false;
                        }
                    }
                }
                //album
                if (!string.IsNullOrWhiteSpace(settings.AlbumName)) {
                    if (AlbumIsValid(settings.AlbumName, settings.AccessToken)) {
                        _orchardServices.Notifier.Information(T("Album Name valid"));
                    } else {
                        string res = CreateNewAlbum(settings.AccessToken, settings.AlbumName);
                        if (res == "OK") {
                            _orchardServices.Notifier.Information(T("Album created"));
                        } else {
                            _orchardServices.Notifier.Error(T("Failed to create album. Internal message: {0}", res));
                            settings.AlwaysUploadToAlbum = false;
                        }
                    }
                }
            } else {
                _orchardServices.Notifier.Error(T("Access token not valid"));
            }

            settings.License = vm.License ?? "";
            settings.Privacy = vm.Privacy;
            settings.Password = vm.Password ?? "";
            if (vm.Privacy.view == "password" && string.IsNullOrWhiteSpace(vm.Password)) {
                _orchardServices.Notifier.Error(T("The password must not be an empty string."));
            }
            settings.ReviewLink = vm.ReviewLink;
            settings.Locale = vm.Locale ?? "";
            settings.ContentRatings = vm.ContentRatingsSafe ?
                new string[] { "safe" }.ToList() :
                vm.ContentRatingsUnsafe.Where(cr => cr.Value == true).Select(cr => cr.Key).ToList();
            settings.Whitelist = string.IsNullOrWhiteSpace(vm.Whitelist) ? new List<string>() : vm.Whitelist.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            RetrieveAccountType(settings);
            settings.LastTimeAccountTypeWasChecked = DateTime.UtcNow;
        }

        /// <summary>
        /// This method makes an API call to Vimeo to retrieve acount information (type and userId).
        /// </summary>
        /// <param name="settings">The part containing the vimeo Settings</param>
        private void RetrieveAccountType(VimeoSettingsPart settings) {
            //make the API call to check the account
            HttpWebRequest userCall = VimeoCreateRequest(
                aToken: settings.AccessToken,
                endpoint: VimeoEndpoints.Me,
                method: "GET",
                qString: "?fields=account,uri"
                );
            try {
                using (HttpWebResponse resp = userCall.GetResponse() as HttpWebResponse) {
                    UpdateAPIRateLimits(settings, resp);
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        string json = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                        var parsed = JObject.Parse(json);
                        if (parsed["account"] != null) {
                            settings.AccountType = parsed["account"].ToString();
                        }
                        if (parsed["uri"] != null) { //uri is in the form "/users/USERID"
                            string uid = parsed["uri"].ToString();
                            settings.UserId = uid.Remove(0, uid.LastIndexOf("/") + 1);
                        }
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(settings, resp);
                }
            }

        }

        /// <summary>
        /// This method check the headers in the response we receive from Vimeo to update the information on the rate limits.
        /// It should be called EVERY TIME a request is made to the Vimeo API. Even (especially) when the response code is not 200.
        /// </summary>
        /// <param name="settings">The part containing the vimeo Settings.</param>
        /// <param name="resp">The response we received from the API.</param>
        private void UpdateAPIRateLimits(VimeoSettingsPart settings, HttpWebResponse resp) {
            var heads = resp.Headers;
            int tmp;
            if (int.TryParse(heads["X-RateLimit-Limit"], out tmp)) {
                settings.RateLimitLimit = tmp;
            }
            if (int.TryParse(heads["X-RateLimit-Remaining"], out tmp)) {
                settings.RateLimitRemaining = tmp;
            }
            DateTime temp;
            if (DateTime.TryParse(heads["X-RateLimit-Reset"], out temp)) {
                settings.RateLimitReset = temp.ToUniversalTime();
            }
        }
        private void UpdateAPIRateLimits(HttpWebResponse resp) {
            var settings = _orchardServices
               .WorkContext
               .CurrentSite
               .As<VimeoSettingsPart>();
            UpdateAPIRateLimits(settings, resp);
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
            HttpWebRequest wr = VimeoCreateRequest(
                aToken: aToken, 
                endpoint: VimeoEndpoints.Me,
                qString: "?fields=name" //using a json filter to receive less data and allow a higher api rate limit
                ); 

            bool ret = false;
            try {
                using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                    UpdateAPIRateLimits(resp);
                    ret = resp.StatusCode == HttpStatusCode.OK;
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(resp);
                }
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
                        UpdateAPIRateLimits(resp);
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
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(resp);
                }
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
                        UpdateAPIRateLimits(resp);
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
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(resp);
                }
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
                        UpdateAPIRateLimits(resp);
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
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(resp);
                }
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Make the request to Vimeo to create a new Group. Since this can be called while updating the settings, 
        /// we have to pass Access Token and Group Name directly, rather than read them from settings.
        /// </summary>
        /// <param name="aToken">The access Token</param>
        /// <param name="gName">The name for the group</param>
        /// <param name="gDesc">A description for the group</param>
        /// <returns>A <type>string</type> with the response received.</returns>
        public string CreateNewGroup(string aToken, string gName, string gDesc = "") {
            HttpWebRequest wr = VimeoCreateRequest(
                aToken: aToken,
                endpoint: VimeoEndpoints.Groups,
                method: "POST",
                qString: "?name=" + gName + "&description=" + (string.IsNullOrWhiteSpace(gDesc) ? gName : gDesc)
                );
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    UpdateAPIRateLimits(resp);
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        return "OK";
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(resp);
                    if (resp.StatusCode == HttpStatusCode.BadRequest) {
                        return "Bad Request: one of the parameters is invalid. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    } else if (resp.StatusCode == HttpStatusCode.Forbidden) {
                        return "Access Denied: user is not allowed to create a Group. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    }
                }

            }
            return "Unknown error";
        }
        /// <summary>
        /// Make the request to Vimeo to create a new Channel. Since this can be called while updating the settings, 
        /// we have to pass Access Token and Channel Name directly, rather than read them from settings.
        /// </summary>
        /// <param name="aToken">The access Token</param>
        /// <param name="cName">The name for the Channel</param>
        /// <param name="cDesc">A description for the Channel</param>
        /// <param name="cPrivacy">The privacy level for the Channel (defaults at user only)</param>
        /// <returns>A <type>string</type> with the response received.</returns>
        public string CreateNewChannel(string aToken, string cName, string cDesc = "", string cPrivacy = "user") {
            if (cPrivacy != "user" && cPrivacy != "anybody")
                cPrivacy = "user";
            HttpWebRequest wr = VimeoCreateRequest(
                aToken: aToken,
                endpoint: VimeoEndpoints.Channels,
                method: "POST",
                qString: "?name=" + cName + "&description=" + (string.IsNullOrWhiteSpace(cDesc) ? cName : cDesc)
                );
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    UpdateAPIRateLimits(resp);
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        return "OK";
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(resp);
                    if (resp.StatusCode == HttpStatusCode.BadRequest) {
                        return "Bad Request: one of the parameters is invalid. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    } else if (resp.StatusCode == HttpStatusCode.Forbidden) {
                        return "Access Denied: user is not allowed to create a channel. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    }
                }
            }
            return "Unknown error";
        }
        /// <summary>
        /// Make the request to Vimeo to create a new album. Since this can be called while updating the settings, 
        /// we have to pass Access Token and Album Name directly, rather than read them from settings.
        /// </summary>
        /// <param name="aToken">The access Token</param>
        /// <param name="aName">The name for the album</param>
        /// <param name="aDesc">A description for the album</param>
        /// <returns>A <type>string</type> with the response received.</returns>
        public string CreateNewAlbum(string aToken, string aName, string aDesc = "") {
            HttpWebRequest wr = VimeoCreateRequest(
                aToken: aToken,
                endpoint: VimeoEndpoints.MyAlbums,
                method: "POST",
                qString: "?name=" + aName + "&description=" + (string.IsNullOrWhiteSpace(aDesc) ? aName : aDesc)
                );
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    UpdateAPIRateLimits(resp);
                    if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created) {
                        return "OK";
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(resp);
                    if (resp.StatusCode == HttpStatusCode.BadRequest) {
                        return "Bad Request: on of the parameters is invalid. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    } else if (resp.StatusCode == HttpStatusCode.Forbidden) {
                        return "Access Denied: user is not allowed to create an album. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    }
                }
            }
            return "Unknown error";
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
            VimeoUploadQuota quotaInfo = null;
            //Only check the quota if we have not checked it in a while
            if (DateTime.UtcNow < settings.LastTimeQuotaWasChecked.Value.AddHours(24)) {
                string queryString = "?fields=upload_quota";
                HttpWebRequest wr = VimeoCreateRequest(settings.AccessToken, VimeoEndpoints.Me, qString: queryString);
                try {
                    using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                        UpdateAPIRateLimits(settings, resp);
                        if (resp.StatusCode == HttpStatusCode.OK) {
                            using (var reader = new System.IO.StreamReader(resp.GetResponseStream())) {
                                string vimeoJson = reader.ReadToEnd();
                                JObject json = JObject.Parse(vimeoJson);
                                quotaInfo = new VimeoUploadQuota();
                                quotaInfo = JsonConvert.DeserializeObject<VimeoUploadQuota>(json["upload_quota"].ToString());
                                settings.UploadQuotaSpaceFree = quotaInfo.space.free;
                                settings.UploadQuotaSpaceMax = quotaInfo.space.max;
                                settings.UploadQuotaSpaceUsed = quotaInfo.space.used;
                                settings.LastTimeQuotaWasChecked = DateTime.UtcNow;
                            }
                        }
                    }
                } catch (Exception ex) {
                    HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                    if (resp != null) {
                        UpdateAPIRateLimits(settings, resp);
                    }
                    quotaInfo = null;
                }
            } else {
                quotaInfo = new VimeoUploadQuota(settings.UploadQuotaSpaceFree, settings.UploadQuotaSpaceMax, settings.UploadQuotaSpaceUsed);
            }
            
            return quotaInfo;
        }
        /// <summary>
        /// Checks the number of Bytes used of the upload quota.
        /// </summary>
        /// <returns>The number of bytes used, or <value>-1</value> in case of error</returns>
        public Int64 UsedQuota() {
            VimeoUploadQuota quotaInfo = CheckQuota();
            return quotaInfo != null ? quotaInfo.space.used : -1;
        }
        /// <summary>
        /// Checks the number of Bytes available of the upload quota.
        /// </summary>
        /// <returns>The number of available bytes, or <value>-1</value> in case of error</returns>
        public Int64 FreeQuota() {
            VimeoUploadQuota quotaInfo = CheckQuota();
            return quotaInfo != null ? quotaInfo.space.free : -1;
        }

        /// <summary>
        /// Verifies that there is enough quota available.
        /// </summary>
        /// <param name="fileSize">The size of the file we would like to start uploading.</param>
        /// <returns>An id of an UploadsInProgressRecord corresponding to the upload we are starting if we have enough quota available for that upload. <value>-1</value> otherwise</returns>
        public int IsValidFileSize(Int64 fileSize) {
            //this method, as it is, does not handle concurrent upload attempts very well.
            //We leave with Vimeo the responsiiblity for the final check on the upload size.

            //the information about the uploads in progress is in the UploadsInProgressRecord table.
            //We check the free quota with what we are trying to upload
            Int64 quotaBeingUploaded = 0;
            if (_repositoryUploadsInProgress.Table.Count() > 0)
                quotaBeingUploaded = _repositoryUploadsInProgress.Table.Sum(u => u.UploadSize) - _repositoryUploadsInProgress.Table.Sum(u => u.UploadedSize);
            Int64 remoteSpace = this.FreeQuota();
            if (remoteSpace - quotaBeingUploaded < fileSize) {
                return -1; //there is not enough space
            }

            //Add the file we want to upload to it, as a "temporary" upload
            UploadsInProgressRecord entity = new UploadsInProgressRecord();
            entity.UploadSize = fileSize;
            entity.CreatedTime = DateTime.UtcNow;
            _repositoryUploadsInProgress.Create(entity);
            ScheduleUploadVerification();
            int recordId = entity.Id;

            return recordId;
        }

        /// <summary>
        /// Generates an upload ticket for a given upload attempt
        /// </summary>
        /// <param name="uploadId">The Id of the record created for the upload we are attempting.</param>
        /// <returns>The Url where the client may upload the file.</returns>
        public string GenerateUploadTicket(int uploadId) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            HttpWebRequest wr = VimeoCreateRequest(
                    settings.AccessToken,
                    VimeoEndpoints.VideoUpload,
                    method: WebRequestMethods.Http.Post,
                    qString: "?type=streaming"
                );
            string uploadUrl = "";
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    UpdateAPIRateLimits(settings, resp);
                    if (resp.StatusCode == HttpStatusCode.Created) {
                        using (var reader = new System.IO.StreamReader(resp.GetResponseStream())) {
                            string vimeoJson = reader.ReadToEnd();
                            JObject json = JObject.Parse(vimeoJson);
                            UploadsInProgressRecord entity = _repositoryUploadsInProgress
                                .Get(uploadId);
                            entity.CompleteUri = json["complete_uri"].ToString();
                            entity.TicketId = json["ticket_id"].ToString();
                            entity.UploadLinkSecure = json["upload_link_secure"].ToString();
                            entity.Uri = json["uri"].ToString();
                            //_repositoryUploadsInProgress.Update(entity);
                            uploadUrl = entity.UploadLinkSecure;
                        }
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(settings, resp);
                }
                return "";
            }
            return uploadUrl;
        }

        /// <summary>
        /// Generate a MediaPart that can be embedded in contents and that will contain the vimeo video we are uploading.
        /// </summary>
        /// <param name="uploadId">The Id of the upload in progress</param>
        /// <returns>The Id of the new MediaPart, or <value>-1</value> in case of error.</returns>
        public int GenerateNewMediaPart(int uploadId) {
            UploadsInProgressRecord uIP = _repositoryUploadsInProgress.Get(uploadId);
            if (uIP == null) return -1;
            return GenerateNewMediaPart(uIP);
        }
        /// <summary>
        /// Generate a MediaPart that can be embedded in contents and that will contain the vimeo video we are uploading.
        /// </summary>
        /// <param name="uploadId">The record of the upload in progress</param>
        /// <returns>The Id of the new MediaPart, or <value>-1</value> in case of error.</returns>
        public int GenerateNewMediaPart(UploadsInProgressRecord uIP) {
            if (uIP == null) return -1;

            //create new mediapart
            //The MimeType should be text/html, because the type of the media part is oEmbed.
            //We use oEmbed because that makes it easy to use it in a web client, once we can whitelist domains.
            //in the handlers, we find that the oEmebd provider is Vimeo, if the request comes from a mobile,
            //we can fiddle with things to avoid sending the oEmbed and instead sending astream URL
            var oEmbedType = _mediaLibraryService.GetMediaTypes().Where(t => t.Name == "OEmbed").SingleOrDefault();
            var mFolders = _mediaLibraryService.GetMediaFolders(null).Where(mf => mf.Name == "VimeoVideos");
            if (mFolders.Count() == 0)
                _mediaLibraryService.CreateFolder(null, "VimeoVideos");

            var part = _contentManager.New<MediaPart>("OEmbed");
            part.MimeType = "text/html";
            part.FolderPath = "VimeoVideos";
            part.LogicalType = "OEmbed";

            var oembedPart = part.As<OEmbedPart>();

            if (oembedPart != null) {
                //here we cannot fill and properly initialize the OEmbedPart, because the video does not exist yet
                _contentManager.Create(oembedPart);
                uIP.MediaPartId = part.Id;
                oembedPart["type"] = "video";
                oembedPart["provider_name"] = "Vimeo";
                oembedPart["provider_url"] = "https://vimeo.com/";
                //Make it so the MediaPart cannot be seen in the MediaLibrary until it is finished
                part.ContentItem.VersionRecord.Published = false;
                part.ContentItem.VersionRecord.Latest = false;
                //_contentManager.Unpublish(oembedPart.ContentItem);
                return part.Id;
            }


            return -1;
        }

        /// <summary>
        /// This method verifies in our records to check the state of an upload.
        /// </summary>
        /// <param name="uploadId">The Id of the MediaPart containing the video whose upload we want to check</param>
        /// <returns>A value describing the state of the upload.</returns>
        public VerifyUploadResult VerifyUpload(int mediaPartId) {
            var mps = _contentManager.GetAllVersions(mediaPartId);
            var it = mps.Where(ci => ci.VersionRecord.Id == mps.Max(cv => cv.VersionRecord.Id)).SingleOrDefault();

            MediaPart mp = it.As<MediaPart>(); // _contentManager.GetLatest(mediaPartId).As<MediaPart>();
            if (mp == null || mp.As<OEmbedPart>() == null)
                return VerifyUploadResult.NeverExisted;
            UploadsInProgressRecord entity = _repositoryUploadsInProgress.Get(e => e.MediaPartId == mediaPartId);
            if (entity == null) {
                //could not find and Upload in progress with the given Id
                //since the media part exists, it either means the upload is complete, or that the mediaPart is of a different kind
                UploadsCompleteRecord ucr = GetByMediaId(mediaPartId);
                if (ucr == null) {
                    //the record gets deleted only after we have set things in the OEmbedPart
                    if (mp.As<OEmbedPart>()["provider_name"] == "Vimeo") {
                        return VerifyUploadResult.CompletedAlready;
                    }
                    return VerifyUploadResult.NeverExisted;
                }
                return VerifyUploadResult.CompletedAlready;
            }
            return VerifyUpload(entity);
        }
        /// <summary>
        /// This method verifies in our records to check the state of an upload.
        /// </summary>
        /// <param name="uploadId">The record corresponding to the upload</param>
        /// <returns>A value describing the state of the upload.</returns>
        public VerifyUploadResult VerifyUpload(UploadsInProgressRecord entity) {

            HttpWebRequest wr = VimeoCreateRequest(
                    endpoint: entity.UploadLinkSecure,
                    method: WebRequestMethods.Http.Put
                );
            wr.Headers.Add("Content-Range: bytes */*");
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    //if we end up here, something went really wrong
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null && resp.StatusDescription == "Resume Incomplete") {
                    //we actually expect status code 308, but that fires an exception, so we have to handle things here
                    //Check that everything has been sent, by reading from the "Range" Header of the response.
                    var range = resp.Headers["Range"];
                    Int64 sent;
                    if (Int64.TryParse(range.Substring(range.IndexOf('-') + 1), out sent)) {
                        if (sent == entity.UploadSize) {
                            //Upload finished
                            return VerifyUploadResult.Complete;
                        } else {
                            //update the uploaded size
                            Int64 old = entity.UploadedSize;
                            entity.UploadedSize = sent;
                            return old == sent ? VerifyUploadResult.Incomplete : VerifyUploadResult.StillUploading;
                            //if the upload has been going on for some time, we may decide to destroy its information.
                            //The distinction between Incomplete and StillUploading helps us understand whether it is
                            //just a slow/long upload, or the upload actualy stopped and we may discard it safely.
                        }
                    }
                }
            }
            return VerifyUploadResult.Error; //something went rather wrong
        }
        /// <summary>
        /// Get the Upload Link to target with PUT requests to upload the video.
        /// </summary>
        /// <param name="mediaPartId">The Id of the MediaPart containing the video whose upload we want to check</param>
        /// <returns>The Upload Url, or <value>null</value> in case of error</returns>
        public string GetUploadUrl(int mediaPartId){
            var mps = _contentManager.GetAllVersions(mediaPartId);
            var it = mps.Where(ci => ci.VersionRecord.Id == mps.Max(cv => cv.VersionRecord.Id)).SingleOrDefault();

            MediaPart mp = it.As<MediaPart>(); // _contentManager.GetLatest(mediaPartId).As<MediaPart>();
            if (mp == null || mp.As<OEmbedPart>() == null)
                return null;
            UploadsInProgressRecord entity = _repositoryUploadsInProgress.Get(e => e.MediaPartId == mediaPartId);
            if (entity == null || string.IsNullOrWhiteSpace(entity.UploadLinkSecure))
                return null;
            return entity.UploadLinkSecure;
        }

        /// <summary>
        /// Gets the UploadComplete corresponding to the upload in progress given by input
        /// </summary>
        /// <param name="pId">The Id of the upload in progress</param>
        /// <returns>The <type>UploadsCompleteRecord</type>.</returns>
        private UploadsCompleteRecord GetByProgressId(int pId) {
            return _repositoryUploadsComplete.Get(r => r.ProgressId == pId);
        }
        /// <summary>
        /// Gets the UploadComplete corresponding to the MediaPart with the given Id
        /// </summary>
        /// <param name="mId">The Id of the MediaPart</param>
        /// <returns>The <type>UploadsCompleteRecord</type>.</returns>
        private UploadsCompleteRecord GetByMediaId(int mId) {
            return _repositoryUploadsComplete.Get(r => r.MediaPartId == mId);
        }

        /// <summary>
        /// We terminate the Vimeo upload stream.
        /// </summary>
        /// <param name="uploadId">The Id we have been using internally to identify the MediaPart whose video's upload is in progress.</param>
        /// <returns><value>true</value> in case of success.<value>false</value> in case of errors.</returns>
        public bool TerminateUpload(int mediaPartId) {
            UploadsCompleteRecord ucr = GetByMediaId(mediaPartId);
            if (ucr != null)
                return true;
            UploadsInProgressRecord entity = _repositoryUploadsInProgress.Get(e => e.MediaPartId == mediaPartId);
            if (entity == null)
                return true;
            return TerminateUpload(entity) > 0;
        }
        /// <summary>
        /// We terminate the Vimeo upload stream.
        /// </summary>
        /// <param name="entity">The Upload in progress that we wish to terminate.</param>
        /// <returns>The Id of the UploadCompleted, which we'll need to patch and publish the video.<value>-1</value> in case of errors.</returns>
        public int TerminateUpload(UploadsInProgressRecord entity) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            //Make the DELETE call to terminate the upload: this gives us the video URI
            HttpWebRequest del = VimeoCreateRequest(
                aToken: settings.AccessToken,
                endpoint: VimeoEndpoints.APIEntry + entity.CompleteUri,
                method: "DELETE"
                );
            try {
                using (HttpWebResponse resp = del.GetResponse() as HttpWebResponse) {
                    UpdateAPIRateLimits(settings, resp);
                    if (resp.StatusCode == HttpStatusCode.Created) {
                        //this is the success condition for this call:
                        //the response contains the video location in its "location" header
                        //create an entry of UploadsCompletedRecord for this upload
                        var ucr = new UploadsCompleteRecord();
                        ucr.Uri = resp.Headers["Location"];
                        ucr.ProgressId = entity.Id;
                        ucr.CreatedTime = DateTime.UtcNow;
                        ucr.MediaPartId = entity.MediaPartId;
                        _repositoryUploadsComplete.Create(ucr);
                        ScheduleVideoCompletion();
                        //delete the entry from uploads in progress
                        _repositoryUploadsInProgress.Delete(entity);
                        return ucr.Id;
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(settings, resp);
                }
            }
            return -1;
        }

        /// <summary>
        /// This method patches the information about a video on the vimeo servers.
        /// </summary>
        /// <param name="ucId">The Id of the COmplete upload we want to patch</param>
        /// <param name="name">The title we want to assign to the video</param>
        /// <param name="description">the description for the video</param>
        /// <returns>A <type>string</type> that contains the response to the patch request.</returns>
        public string PatchVideo(int ucId, string name = "", string description = "") {
            UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
            if (ucr != null) {
                return PatchVideo(ucr, name, description);
            }
            return "Record is null";
        }
        /// <summary>
        /// This method patches the information about a video on the vimeo servers.
        /// </summary>
        /// <param name="ucr">The COmplete upload we want to patch</param>
        /// <param name="name">The title we want to assign to the video</param>
        /// <param name="description">the description for the video</param>
        /// <returns>A <type>string</type> that contains the response to the patch request.</returns>
        public string PatchVideo(UploadsCompleteRecord ucr, string name = "", string description = "") {
            if (ucr == null) return "Record is null";

            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            //The things we want to change of the video go in the request body as a JSON.
            VimeoPatch patchData = new VimeoPatch {
                name = string.IsNullOrWhiteSpace(name) ? "title" : name,
                description = string.IsNullOrWhiteSpace(description) ? "description" : description,
                license = settings.License,
                privacy = settings.Privacy,
                password = settings.Password,
                review_link = settings.ReviewLink,
                locale = settings.Locale,
                content_rating = settings.ContentRatings
            };
            var json = JsonConvert.SerializeObject(patchData);
            //We must set the request header
            // "Content-Type" to "application/json"
            HttpWebRequest wr = VimeoCreateRequest(
                    aToken: settings.AccessToken,
                    endpoint: VimeoEndpoints.APIEntry + ucr.Uri,
                    method: "PATCH"
                    );
            wr.ContentType = "application/json";
            using (StreamWriter bWriter = new StreamWriter(wr.GetRequestStream())) {
                bWriter.Write(json);
            }
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    UpdateAPIRateLimits(settings, resp);
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        ucr.Patched = true;
                        return "OK";
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                UpdateAPIRateLimits(settings, resp);
                //if some parameter is wrong in the patch, we get status code 400 Bad Request
                if (resp != null && resp.StatusCode == HttpStatusCode.BadRequest) {
                    return new StreamReader(resp.GetResponseStream()).ReadToEnd();
                }
                return resp.StatusCode.ToString() + " " + resp.StatusDescription;
            }
            return "Unknown error";
        }

        /// <summary>
        /// Get from Vimeo the Id corresponding to the group set in the settings.
        /// </summary>
        /// <returns>The Id as a <type>string</type>, or null if the group cannot be found.</returns>
        public string GetGroupId() {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            HttpWebRequest wr = VimeoCreateRequest(
                aToken: settings.AccessToken,
                endpoint: VimeoEndpoints.MyGroups,
                method: "GET",
                qString: "?query=" + settings.GroupName + "&fields=name,uri"
                );
            try {
                bool morePages = false;
                do {
                    using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                        UpdateAPIRateLimits(settings, resp);
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
                                    JObject json = JObject.Parse(vimeoJson);
                                    IList<JToken> res = json["data"].Children().ToList();
                                    foreach (JToken result in res) {
                                        VimeoGroup gr = JsonConvert.DeserializeObject<VimeoGroup>(result.ToString());
                                        if (gr.name == settings.GroupName) {
                                            //extract the Id from the uri
                                            return gr.uri.Substring(gr.uri.LastIndexOf("/") + 1);
                                        }
                                    }
                                    if (pager.total > pager.per_page * pager.page) {
                                        morePages = true;
                                        //generate a new request
                                        string pageQuery = "page=" + (pager.page + 1).ToString();
                                        wr = VimeoCreateRequest(
                                            aToken: settings.AccessToken,
                                            endpoint: VimeoEndpoints.MyGroups,
                                            method: "GET",
                                            qString: "?query=" + settings.GroupName + "&fields=name,uri&" + pageQuery
                                            );
                                    }
                                }
                            }
                        }
                    }
                } while (morePages);
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(settings, resp);
                }
            }

            return null;
        }
        /// <summary>
        /// Get from vimeo the id corresponding to the channel set in settings.
        /// </summary>
        /// <returns>The Id as a <type>string</type>, or null if the channel cannot be found.</returns>
        public string GetChannelId() {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            HttpWebRequest wr = VimeoCreateRequest(
                aToken: settings.AccessToken,
                endpoint: VimeoEndpoints.MyChannels,
                method: "GET",
                qString: "?query=" + settings.ChannelName + "&fields=name,uri"
                );
            try {
                bool morePages = false;
                do {
                    using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                        UpdateAPIRateLimits(settings, resp);
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
                                    JObject json = JObject.Parse(vimeoJson);
                                    IList<JToken> res = json["data"].Children().ToList();
                                    foreach (JToken result in res) {
                                        VimeoChannel ch = JsonConvert.DeserializeObject<VimeoChannel>(result.ToString());
                                        if (ch.name == settings.ChannelName) {
                                            //extract the id from the uri
                                            return ch.uri.Substring(ch.uri.LastIndexOf("/") + 1);
                                        }
                                    }
                                    if (pager.total > pager.per_page * pager.page) {
                                        morePages = true;
                                        //generate a new request
                                        string pageQuery = "page=" + (pager.page + 1).ToString();
                                        wr = VimeoCreateRequest(
                                            aToken: settings.AccessToken,
                                            endpoint: VimeoEndpoints.MyChannels,
                                            method: "GET",
                                            qString: "?query=" + settings.ChannelName + "&fields=name,uri&" + pageQuery
                                            );
                                    }
                                }
                            }
                        }
                    }
                } while (morePages);
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(settings, resp);
                }
            }

            return null;
        }
        /// <summary>
        /// Get from vimeo the id corresponding to the album set in settings.
        /// </summary>
        /// <returns>The Id as a <type>string</type>, or null if the album cannot be found.</returns>
        public string GetAlbumId() {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            HttpWebRequest wr = VimeoCreateRequest(
                aToken: settings.AccessToken,
                endpoint: VimeoEndpoints.MyAlbums,
                method: "GET",
                qString: "?query=" + settings.AlbumName + "&fields=name,uri"
                );
            try {
                bool morePages = false;
                do {
                    using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                        UpdateAPIRateLimits(settings, resp);
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
                                    JObject json = JObject.Parse(vimeoJson);
                                    IList<JToken> res = json["data"].Children().ToList();
                                    foreach (JToken result in res) {
                                        VimeoAlbum al = JsonConvert.DeserializeObject<VimeoAlbum>(result.ToString());
                                        if (al.name == settings.AlbumName) {
                                            return al.uri.Substring(al.uri.LastIndexOf("/") + 1);
                                        }
                                    }
                                    if (pager.total > pager.per_page * pager.page) {
                                        morePages = true;
                                        //generate a new request
                                        string pageQuery = "page=" + (pager.page + 1).ToString();
                                        wr = VimeoCreateRequest(
                                            aToken: settings.AccessToken,
                                            endpoint: VimeoEndpoints.MyAlbums,
                                            method: "GET",
                                            qString: "?query=" + settings.AlbumName + "&fields=name,uri&" + pageQuery
                                            );
                                    }
                                }
                            }
                        }
                    }
                } while (morePages);
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(settings, resp);
                }
            }

            return null;
        }

        /// <summary>
        /// If we set things to automatically add videos to a group, it does so.
        /// </summary>
        /// <param name="ucId">The Id of a completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string TryAddVideoToGroup(int ucId) {
            var settings = _orchardServices
                        .WorkContext
                        .CurrentSite
                        .As<VimeoSettingsPart>();
            if (settings.AlwaysUploadToGroup) {
                return AddVideoToGroup(ucId);
            }
            return "Did not have to add.";
        }
        /// <summary>
        /// Add the video corresponding to the Completed Upload whose id is passed to the group stored in the settings
        /// </summary>
        /// <param name="ucId">The Id of a completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string AddVideoToGroup(int ucId) {
            UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
            if (ucr != null) {
                return AddVideoToGroup(ucr);
            } else {
                return "Cannot identify video";
            }
        }
        /// <summary>
        /// Add the video corresponding to the Completed Upload that is passed to the group stored in the settings
        /// </summary>
        /// <param name="ucId">The completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string AddVideoToGroup(UploadsCompleteRecord ucr) {
            if (ucr == null) return "Cannot identify video";

            string groupId = GetGroupId();
            if (!string.IsNullOrWhiteSpace(groupId)) {
                var settings = _orchardServices
                        .WorkContext
                        .CurrentSite
                        .As<VimeoSettingsPart>();
                HttpWebRequest wr = VimeoCreateRequest(
                    aToken: settings.AccessToken,
                    endpoint: VimeoEndpoints.Groups + "/" + groupId + ucr.Uri,
                    method: "PUT"
                    );
                try {
                    using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                        UpdateAPIRateLimits(settings, resp);
                        if (resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.NoContent) {
                            ucr.UploadedToGroup = true;
                            return "OK";
                        }
                    }
                } catch (Exception ex) {
                    HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                    if (resp != null) {
                        UpdateAPIRateLimits(settings, resp);
                        if (resp.StatusCode == HttpStatusCode.Forbidden) {
                            return "Access Denied: cannot add video. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                        }
                    }
                }
            } else {
                return "Cannot access group";
            }

            return "Unknown error";
        }

        /// <summary>
        /// If we set things to automatically add videos to a channel, it does so.
        /// </summary>
        /// <param name="ucId">The Id of a completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string TryAddVideoToChannel(int ucId) {
            var settings = _orchardServices
                        .WorkContext
                        .CurrentSite
                        .As<VimeoSettingsPart>();
            if (settings.AlwaysUploadToChannel) {
                return AddVideoToChannel(ucId);
            }
            return "Did not have to add.";
        }
        /// <summary>
        /// Add the video corresponding to the Completed Upload whose id is passed to the channel stored in the settings
        /// </summary>
        /// <param name="ucId">The Id of a completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string AddVideoToChannel(int ucId) {
            UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
            if (ucr != null) {
                return AddVideoToChannel(ucr);
            } else {
                return "Cannot identify video";
            }
        }
        /// <summary>
        /// Add the video corresponding to the Completed Upload that is passed to the channel stored in the settings
        /// </summary>
        /// <param name="ucId">The completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string AddVideoToChannel(UploadsCompleteRecord ucr) {
            if (ucr == null) return "Cannot identify video";

            string chanId = GetChannelId();
            if (!string.IsNullOrWhiteSpace(chanId)) {
                var settings = _orchardServices
                        .WorkContext
                        .CurrentSite
                        .As<VimeoSettingsPart>();
                HttpWebRequest wr = VimeoCreateRequest(
                    aToken: settings.AccessToken,
                    endpoint: VimeoEndpoints.Channels + "/" + chanId + ucr.Uri,
                    method: "PUT"
                    );
                try {
                    using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                        UpdateAPIRateLimits(settings, resp);
                        if (resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.NoContent) {
                            ucr.UploadedToChannel = true;
                            return "OK";
                        }
                    }
                } catch (Exception ex) {
                    HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                    if (resp != null) {
                        UpdateAPIRateLimits(settings, resp);
                        if (resp.StatusCode == HttpStatusCode.Forbidden) {
                            return "Access Denied: cannot add video. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                        } else if (resp.StatusCode == HttpStatusCode.NotFound) {
                            return "Resource not found. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                        }
                    }
                }
            } else {
                return "Cannot access channel";
            }

            return "Unknown error";
        }

        /// <summary>
        /// If we set things to automatically add videos to an album, it does so.
        /// </summary>
        /// <param name="ucId">The Id of a completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string TryAddVideoToAlbum(int ucId) {
            var settings = _orchardServices
                        .WorkContext
                        .CurrentSite
                        .As<VimeoSettingsPart>();
            if (settings.AlwaysUploadToAlbum) {
                return AddVideoToAlbum(ucId);
            }
            return "Did not have to add.";
        }
        /// <summary>
        /// Add the video corresponding to the Completed Upload whose id is passed to the album stored in the settings
        /// </summary>
        /// <param name="ucId">The Id of a completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string AddVideoToAlbum(int ucId) {
            UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
            if (ucr != null) {
                return AddVideoToAlbum(ucr);
            } else {
                return "Cannot identify video";
            }
        }
        /// <summary>
        /// Add the video corresponding to the Completed Upload that is passed to the album stored in the settings
        /// </summary>
        /// <param name="ucId">The completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string AddVideoToAlbum(UploadsCompleteRecord ucr) {
            if (ucr == null) return "Cannot identify video";

            string alId = GetAlbumId();
            if (!string.IsNullOrWhiteSpace(alId)) {
                var settings = _orchardServices
                        .WorkContext
                        .CurrentSite
                        .As<VimeoSettingsPart>();
                HttpWebRequest wr = VimeoCreateRequest(
                    aToken: settings.AccessToken,
                    endpoint: VimeoEndpoints.MyAlbums + "/" + alId + ucr.Uri,
                    method: "PUT"
                    );
                try {
                    using (HttpWebResponse resp = (HttpWebResponse)wr.GetResponse()) {
                        UpdateAPIRateLimits(settings, resp);
                        if (resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.NoContent) {
                            ucr.UploadedToAlbum = true;
                            return "OK";
                        }
                    }
                } catch (Exception ex) {
                    HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                    if (resp != null) {
                        UpdateAPIRateLimits(settings, resp);
                        if (resp.StatusCode == HttpStatusCode.Forbidden) {
                            return "Access Denied: cannot add video. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                        } else if (resp.StatusCode == HttpStatusCode.NotFound) {
                            return "Resource not found. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                        }
                    }
                }
            } else {
                return "Cannot access album";
            }

            return "Unknown error";
        }

        /// <summary>
        /// This method extract a video stream's url. It is only able to do so for videos we own if
        /// the video is not private and it can be embedded or we have a PRO account.
        /// </summary>
        /// <param name="ucId">The Id of the complete upload to test.</param>
        /// <returns>The URL of the video stream.</returns>
        public string ExtractVimeoStreamURL(int mediaPartId) {
            var mps = _contentManager.GetAllVersions(mediaPartId);
            var it = mps.Where(ci => ci.VersionRecord.Id == mps.Max(cv => cv.VersionRecord.Id)).SingleOrDefault();

            OEmbedPart part = it.As<MediaPart>().As<OEmbedPart>();
            //OEmbedPart part = _contentManager.GetLatest(mediaPartId).As<MediaPart>().As<OEmbedPart>();
            return part == null ? null : ExtractVimeoStreamURL(part);
        }
        public string ExtractVimeoStreamURL(OEmbedPart part) {
            var settings = _orchardServices
                        .WorkContext
                        .CurrentSite
                        .As<VimeoSettingsPart>();
            string uri = part["uri"];
            string vUri = part["uri"].Remove(part["uri"].IndexOf("video") + 5, 1);//ucr.Uri.Remove(ucr.Uri.IndexOf("video") + 5, 1); //the original uri is /videos/ID, but we want /video/ID
            //is this a pro account?
            //we can either store this information in the settings, or make an API call to /me and check the account field of the user object            
            //only verify the account status if more than 24 hours have passed. Otherwise we use the cached info.
            if (DateTime.UtcNow < settings.LastTimeAccountTypeWasChecked.Value.AddHours(24)) {
                RetrieveAccountType(settings);
            }
            bool proAccount = settings.AccountType ==  "pro";
            string uId = settings.UserId; //user id on vimeo

            //NOTE: remember to verify if we own the video
            //make an API call to get the info for this video
            HttpWebRequest apiCall = VimeoCreateRequest(
                aToken: settings.AccessToken,
                endpoint: VimeoEndpoints.APIEntry + part["uri"], //NOTE: this is not /me/videos
                //endpoint: VimeoEndpoints.Me + part["uri"], //me/videos/id
                method: "GET",
                qString: "?fields=files,user.uri,privacy" //the "files" field is an array with the info for the different resolutions available
                );
            JObject videoJsonTree = null;
            try {
                using (HttpWebResponse resp = apiCall.GetResponse() as HttpWebResponse) {
                    UpdateAPIRateLimits(settings, resp);
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        string data = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                        videoJsonTree = JObject.Parse(data);
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(settings, resp);
                }
                return ex.Message;
            }

            //If we have access to the files, we can get the stream url from those
            if (proAccount && videoJsonTree["files"] != null) {
                //if we have a pro account, we can get a static stream url
                //go through the files in the json
                int width = 0;
                int height = 0;
                int pp = width * height;
                string url = "";
                //get the highest resolution file
                foreach (var file in videoJsonTree["files"].Children()) {
                    if (file["quality"].ToString() != "hls") {
                        int w = int.Parse(file["width"].ToString());
                        int h = int.Parse(file["height"].ToString());
                        if (w * h > pp) {
                            width = w;
                            height = h;
                            pp = w * h;
                            url = file["link_secure"].ToString();
                        }
                    }
                }
                //The url we get here is in the form http://player.vimeo.com/external/videoid.sd.mp4?stuff
                //The players on mobile are not able to do anything with it, so we have to get the actual stream url out.
                //By testing on fiddler, we saw that the initial response from a call to that url has no body, status 302, and a Location header
                // for the redirect, which contains the stream url we want to pass to the apps.
                //Let's get that.
                HttpWebRequest playerCall = HttpWebRequest.CreateHttp(url);
                playerCall.Method = "GET";
                try {
                    using (HttpWebResponse resp = playerCall.GetResponse() as HttpWebResponse) {
                        //we get redirected, so our request gets changed
                        url = playerCall.Address.AbsoluteUri;
                    }
                } catch (Exception ex) {
                }
                return url;
            } else {
                //if our account is not pro, under some conditions we may be able to extract a stream's url
                //NOTE: this url expires after a while (it's not clear how long), and has to be reextracted
                //We are also here for public videos we do not own, even with a pro account
                //To get the stream's url, the video must be embeddable in our current domain
                bool embeddable = false;
                try {
                    VimeoVideoPrivacy videoPrivacy = JsonConvert.DeserializeObject<VimeoVideoPrivacy>(videoJsonTree["privacy"].ToString());
                    if (videoPrivacy.view == "anybody") {
                        embeddable = videoPrivacy.embed == "public";
                        if (!embeddable) {
                            //check if we are in a whitelisted domain
                            //if we are not in a whitelisted domain, there is no way to get the stream's url
                            //verify this, because baseUrl does not seem to contain the site name
                            string myDomain = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                            embeddable = settings.Whitelist.Contains(myDomain);
                        }
                    }
                } catch (Exception ex) {
                    return ex.Message;
                }
                if (embeddable) {
                    //here we make a call as if we were a browser
                    HttpWebRequest wr = VimeoCreateRequest(
                        aToken: settings.AccessToken,
                        endpoint: VimeoEndpoints.PlayerEntry + vUri + "/config",
                        method: "GET"
                        );
                    wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
                    wr.Accept = "*/*";
                    try {
                        using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                            if (resp.StatusCode == HttpStatusCode.OK) {
                                string data = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                                var jsonTree = JObject.Parse(data);
                                IList<JToken> res = jsonTree["request"]["files"]["progressive"].Children().ToList();
                                int quality = 0;
                                string url = "";
                                foreach (var item in res) {
                                    int q = int.Parse(item["quality"].ToString().Trim(new char[] { 'p' }));
                                    if (q > quality) {
                                        quality = q;
                                        url = item["url"].ToString();
                                    }
                                }
                                return url;
                            }
                        }
                    } catch (Exception ex) {
                        return ex.Message;
                    }
                }
            }

            return null;
        }
        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Get from Vimeo the processing status of the video.
        /// </summary>
        /// <param name="ucId">The id of the record that contains the information on the video we are checking</param>
        /// <returns>A string describing the video'sprocessing status in Vimeo's servers.</returns>
        public string GetVideoStatus(int ucId) {
            UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
            return GetVideoStatus(ucr);
        }
        public string GetVideoStatus(UploadsCompleteRecord ucr) {
            if (ucr == null) return "Record is null";

            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            HttpWebRequest wr = VimeoCreateRequest(
                    aToken: settings.AccessToken,
                    endpoint: VimeoEndpoints.APIEntry + ucr.Uri,
                    method: "GET",
                    qString: "?fields=status"
                    );
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    UpdateAPIRateLimits(settings, resp);
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        using (var reader = new System.IO.StreamReader(resp.GetResponseStream())) {
                            string vimeoJson = reader.ReadToEnd();
                            //todo: "FILL THIS METHOD IN";
                            return JObject.Parse(vimeoJson)["status"].ToString();
                        }
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    UpdateAPIRateLimits(settings, resp);
                }
                return ex.Message;
            }
            return "Unknown error";
        }

        /// <summary>
        /// Fill in the MediaPart information by pretending to embed the video.
        /// </summary>
        /// <param name="ucId">The id of the record that contains the information on the video we are checking</param>
        public void FinishMediaPart(int ucId) {
            UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
            if (ucr != null) FinishMediaPart(ucr);
        }
        public void FinishMediaPart(UploadsCompleteRecord ucr) {
            string vId = ucr.Uri.Substring(ucr.Uri.LastIndexOf("/") + 1);
            string url = "https://vimeo.com/" + vId;
            var mps = _contentManager.GetAllVersions(ucr.MediaPartId);
            var it = mps.Where(ci => ci.VersionRecord.Id == mps.Max(cv => cv.VersionRecord.Id)).SingleOrDefault();

            MediaPart mPart = it.As<MediaPart>();
            //MediaPart mPart = _contentManager.GetLatest(ucr.MediaPartId).As<MediaPart>();
            var oembedPart = mPart.As<OEmbedPart>();
            //I took this code from Orchard.MediaLibrary.Controllers.OEmbedController
            //(the example there is actually an embed of a vimeo video)
            XDocument oeContent = null;
            var wClient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
            try {
                var source = wClient.DownloadString(url);
                var oembedSignature = source.IndexOf("type=\"text/xml+oembed\"", StringComparison.OrdinalIgnoreCase);
                if (oembedSignature == -1) {
                    oembedSignature = source.IndexOf("type=\"application/xml+oembed\"", StringComparison.OrdinalIgnoreCase);
                }
                if (oembedSignature != -1) {
                    var tagStart = source.Substring(0, oembedSignature).LastIndexOf('<');
                    var tagEnd = source.IndexOf('>', oembedSignature);
                    var tag = source.Substring(tagStart, tagEnd - tagStart);
                    var matches = new Regex("href=\"([^\"]+)\"").Matches(tag);
                    if (matches.Count > 0) {
                        var href = matches[0].Groups[1].Value;
                        try {
                            var content = wClient.DownloadString(HttpUtility.HtmlDecode(href));
                            oeContent = XDocument.Parse(content);
                        } catch {
                            //bubble
                        }
                    }
                }
            } catch (Exception ex) {

            }
            if (oembedPart != null) {
                //These steps actually fill the stuff in the OEMbedPart.
                //oembedPart.Source = url;

                if (oeContent != null) {
                    var oembed = oeContent.Root;
                    if (oembed.Element("title") != null) {
                        mPart.Title = oembed.Element("title").Value;
                    } else {
                        mPart.Title = oembed.Element("url").Value;
                    }
                    if (oembed.Element("description") != null) {
                        mPart.Caption = oembed.Element("description").Value;
                    }
                    foreach (var element in oembed.Elements()) {
                        oembedPart[element.Name.LocalName] = element.Value;
                    }
                } else {
                    //oeContent == null means we were not able to parse that stuff above. Maybe the video is set to private
                    //or whitelisted. Anyway, we can get most of that stuff by calling the vimeo API.
                    oembedPart["type"] = "video";
                    oembedPart["provider_name"] = "Vimeo";
                    oembedPart["provider_url"] = "https://vimeo.com/";

                    oembedPart["uri"] = ucr.Uri;
                    oembedPart["video_id"] = vId;
                    oembedPart["version"] = "1.0";
                    //call the API to get info on this video
                    var settings = _orchardServices
                        .WorkContext
                        .CurrentSite
                        .As<VimeoSettingsPart>();
                    HttpWebRequest wr = VimeoCreateRequest(
                        aToken: settings.AccessToken,
                        endpoint: VimeoEndpoints.Me + ucr.Uri,
                        qString: "?fields=name,uri,description,duration,width,height,release_time,embed.html,pictures.sizes,user.name,user.uri,user.account"
                        );
                    try {
                        using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                            UpdateAPIRateLimits(settings, resp);
                            if (resp.StatusCode == HttpStatusCode.OK) {
                                using (var reader = new System.IO.StreamReader(resp.GetResponseStream())) {
                                    string vimeoJson = reader.ReadToEnd();
                                    VimeoVideo video = JsonConvert.DeserializeObject<VimeoVideo>(vimeoJson);
                                    oembedPart["title"] = video.name;
                                    oembedPart["description"] = video.description;
                                    oembedPart["duration"] = video.duration.ToString();
                                    oembedPart["width"] = video.width.ToString();
                                    oembedPart["height"] = video.height.ToString();
                                    oembedPart["upload_date"] = video.release_time;
                                    //embed section
                                    if (video.embed.html != null) {
                                        oembedPart["html"] = video.embed.html;
                                    } else {
                                        //this correctly populates the html. However, if the video embed settings are "private" it will still not be possible
                                        //to watch it in the embed object
                                        string iframe = string.Format("<iframe src=\"https://player.vimeo.com{0}\" width=\"{1}\" height=\"{2}\" frameborder=\"0\" title=\"title\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>", ucr.Uri.Remove(ucr.Uri.IndexOf("video") + 5, 1), video.width.ToString(), video.height.ToString());
                                        oembedPart["html"] = iframe;
                                    }

                                    //pictures section (get the ones that are closer in size to the video)
                                    int picIndex = 0;
                                    double sizeDistance = double.MaxValue;
                                    int i = 0;
                                    foreach (var aPic in video.pictures.sizes) {
                                        double dist = (aPic.width - video.width) * (aPic.width - video.width) + (aPic.height - video.height) * (aPic.height - video.height);
                                        if (dist < sizeDistance) {
                                            picIndex = i;
                                            sizeDistance = dist;
                                        }
                                        i++;
                                    }
                                    var myPic = video.pictures.sizes.ElementAt(picIndex);
                                    oembedPart["thumbnail_url"] = myPic.link;
                                    oembedPart["thumbnail_width"] = myPic.width.ToString();
                                    oembedPart["thumbnail_height"] = myPic.height.ToString();
                                    oembedPart["thumbnail_url_with_play_button"] = myPic.link_with_play_button;

                                    //user section
                                    oembedPart["author_name"] = video.user.name;
                                    oembedPart["author_url"] = video.user.uri;
                                    oembedPart["is_plus"] = video.user.account == "plus" ? "1" : "0";
                                }
                            }
                        }
                    } catch (Exception ex) {
                        HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                        if (resp != null) {
                            UpdateAPIRateLimits(settings, resp);
                        }
                    }
                }

                //put in the Source a string "Vimeo: {0}", replacing into {0} the encrypted stream's URL
                oembedPart.Source = "Vimeo|" + EncryptedVideoUrl(ExtractVimeoStreamURL(oembedPart));
                _contentManager.Publish(mPart.ContentItem);
                mPart.ContentItem.VersionRecord.Latest = true;
            }
        }

        /// <summary>
        /// Takes a string and encrypts it using the AES algorithm.
        /// </summary>
        /// <param name="url">The url string we want to encrypt</param>
        /// <returns>A string representing the encrypted representation of the input url.</returns>
        public string EncryptedVideoUrl(string url) {
            byte[] mykey = _shellSettings
                    .EncryptionKey
                    .ToByteArray();
            byte[] iv = GetRandomIV();
            //string testUrl = "https://fpdl.vimeocdn.com/vimeo-prod-skyfire-std-us/01/4783/6/173915862/562965183.mp4?token=5784fa6a_0xe50aa62dee225b04625601b6854294244ebdbe94";
            var encryptedUrl = Convert.ToBase64String(EncryptURL(url, mykey, iv));
            //NOTE: iv is 16 bytes long, so its base64 string representation has 4*ceiling(16/3) = 24 characters
            return Convert.ToBase64String(iv) + encryptedUrl;
        }
        private byte[] EncryptURL(string url, byte[] Key, byte[] IV) {
            // Check arguments.
            if (url == null || url.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = CreateCryptoService(Key, IV)) {

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream()) {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {

                            //Write all data to the stream.
                            swEncrypt.Write(url);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
        private AesCryptoServiceProvider CreateCryptoService(byte[] key, byte[] iv) {
            AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider();
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            return aesAlg;
        }
        private byte[] GetRandomIV() {
            string iv = string.Format("{0}{0}", DateTime.UtcNow.ToString("ddMMyyyy").Substring(0, 8));
            return Encoding.UTF8.GetBytes(iv);
        }

        /// <summary>
        /// Destroy all records related to the upload of a video for a given MediaPart. Moreover, destroy the MediaPart. 
        /// Thisshould be called in error handling, for instance when a client tells us that the upload it was trying
        /// to perform has been interrupted and may not be resumed.
        /// </summary>
        /// <param name="mediaPartId">The Id of the MediaPart that was created to contain the video.</param>
        public string DestroyUpload(int mediaPartId) {
            StringBuilder str = new StringBuilder();
            str.AppendLine(T("Clearing references to uploads for MediaPart {0}", mediaPartId).ToString());
            //destroy the in progress record, if any
            UploadsInProgressRecord upr = _repositoryUploadsInProgress.Get(u => u.MediaPartId == mediaPartId);
            if (upr != null) {
                _repositoryUploadsInProgress.Delete(upr);
                str.AppendLine(T("Cleared records of uploads in progress").ToString());
            }
            //destroy the completed upload, if any
            UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(u => u.MediaPartId == mediaPartId);
            if (ucr != null) {
                //destroy the video on Vimeo
                var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
                HttpWebRequest wr = VimeoCreateRequest(
                    aToken: settings.AccessToken,
                    endpoint: VimeoEndpoints.APIEntry + ucr.Uri,
                    method: "DELETE"
                    );
                try {
                    using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                        UpdateAPIRateLimits(settings, resp);
                        if (resp.StatusCode == HttpStatusCode.NoContent) {
                            //success
                            str.AppendLine(T("Removed video on Vimeo.com").ToString());
                        } else {
                            str.AppendLine(T("Failed to remove video on Vimeo.com").ToString());
                        }
                    }
                } catch (Exception ex) {
                    HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                    if (resp != null) {
                        UpdateAPIRateLimits(settings, resp);
                    }
                    str.AppendLine(T("Failed to remove video on Vimeo.com").ToString());
                }
                //destroy the record
                _repositoryUploadsComplete.Delete(ucr);
                str.AppendLine(T("Cleared records of complete uploads").ToString());
            }
            //destroy the MediaPart, if any
            var mps = _contentManager.GetAllVersions(mediaPartId);
            var it = mps.Where(ci => ci.VersionRecord.Id == mps.Max(cv => cv.VersionRecord.Id)).SingleOrDefault();

            MediaPart mp = it.As<MediaPart>();
            //MediaPart mp = _contentManager.GetLatest(mediaPartId).As<MediaPart>();
            if (mp != null) {
                OEmbedPart ep = mp.As<OEmbedPart>();
                if (ep != null) {
                    if (!string.IsNullOrWhiteSpace(ep["provider_name"]) && ep["provider_name"] == "Vimeo") {
                        //NOTE: this is a soft delete
                        _contentManager.Remove(mp.ContentItem);
                        str.AppendLine(T("Removed MediaPart").ToString());
                    }
                }
            }
            return str.ToString();
        }

        /// <summary>
        /// This method is only used in debug and testing. It allows clearing the uploads' repositories
        /// </summary>
        public void ClearRepositoryTables() {
            foreach (var u in _repositoryUploadsInProgress.Table.ToList()) {
                _repositoryUploadsInProgress.Delete(u);
            }
            foreach (var u in _repositoryUploadsComplete.Table.ToList()) {
                _repositoryUploadsComplete.Delete(u);
            }
        }

        #region Code for tasks
        public void ScheduleUploadVerification() {
            string taskTypeStr = Constants.TaskTypeBase + Constants.TaskSubtypeInProgress;
            if (_taskManager.GetTasks(taskTypeStr).Count() == 0)
                _taskManager.CreateTask(taskTypeStr, DateTime.UtcNow.AddMinutes(1), null);
        }
        public void ScheduleVideoCompletion() {
            string taskTypeStr = Constants.TaskTypeBase + Constants.TaskSubtypeComplete;
            if (_taskManager.GetTasks(taskTypeStr).Count() == 0)
                _taskManager.CreateTask(taskTypeStr, DateTime.UtcNow.AddMinutes(1), null);
        }
        /// <summary>
        /// Verifies the state of all uploads in progress.
        /// </summary>
        /// <returns>The number of uploads in progress.</returns>
        public int VerifyAllUploads() {
            foreach (var uip in _repositoryUploadsInProgress.Table.ToList()) {
                switch (VerifyUpload(uip)) {
                    case VerifyUploadResult.CompletedAlready:
                        break;
                    case VerifyUploadResult.Complete:
                        try {
                            TerminateUpload(uip);
                        } catch (Exception ex) {
                            //we might end up here if the termination was called at the same time from here and the controller
                        }
                        break;
                    case VerifyUploadResult.Incomplete:
                        //if more than 24 hours have passed since the beginning of the upload, cancel it
                        if (DateTime.UtcNow > uip.CreatedTime.Value.AddDays(1)) {
                            DestroyUpload(uip.MediaPartId);
                        }
                        break;
                    case VerifyUploadResult.StillUploading:
                        break;
                    case VerifyUploadResult.NeverExisted:
                        break;
                    case VerifyUploadResult.Error:
                        break;
                    default:
                        break;
                }
            }
            return _repositoryUploadsInProgress.Table.Count();
        }
        /// <summary>
        /// Performs the operations required to close the processing of uploaded videos.
        /// </summary>
        /// <returns>The number of videos still to be closed.</returns>
        public int TerminateUploads() {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            foreach (UploadsCompleteRecord ucr in _repositoryUploadsComplete.Table.ToList()) {
                if (!ucr.Patched) {
                    PatchVideo(ucr);
                }
                if (settings.AlwaysUploadToGroup && !ucr.UploadedToGroup) {
                    AddVideoToGroup(ucr);
                }
                if (settings.AlwaysUploadToChannel && !ucr.UploadedToChannel) {
                    AddVideoToChannel(ucr);
                }
                if (settings.AlwaysUploadToAlbum && !ucr.UploadedToAlbum) {
                    AddVideoToAlbum(ucr);
                }
                if (!ucr.IsAvailable) {
                    ucr.IsAvailable = GetVideoStatus(ucr) == "available";
                }

                if (ucr.Patched
                    && ucr.IsAvailable
                    && (ucr.UploadedToGroup || !settings.AlwaysUploadToGroup)
                    && (ucr.UploadedToChannel || !settings.AlwaysUploadToChannel)
                    && (ucr.UploadedToAlbum || !settings.AlwaysUploadToAlbum)) {
                    //Update the MediaPart
                    FinishMediaPart(ucr);
                    //We finished everything for this video upload, so we may remove its record
                    _repositoryUploadsComplete.Delete(ucr);
                }
            }
            return _repositoryUploadsComplete.Table.Count();
        }

        #endregion

        /// <summary>
        /// Creates a default HttpWebRequest Using the Access Token and endpoint provided. By default, the Http Method is GET.
        /// </summary>
        /// <param name="aToken">The Authorized Access Token.</param>
        /// <param name="endpoint">The API endpoint for the request.</param>
        /// <param name="method">The Http Method for the request. <default>GET</default></param>
        /// <returns>An <type>HttpWebRequest</type> object, whose header is preset to the defaults for Vimeo.</returns>
        private HttpWebRequest VimeoCreateRequest(string aToken = "", string endpoint = VimeoEndpoints.Me, string method = WebRequestMethods.Http.Get, string qString = null) {
            Uri targetUri;
            if (string.IsNullOrWhiteSpace(qString)) {
                targetUri = new Uri(endpoint);
            } else {
                targetUri = new Uri(endpoint + qString);
            }
            HttpWebRequest wr = HttpWebRequest.CreateHttp(targetUri);
            wr.Accept = Constants.HeaderAcceptValue;
            if (!string.IsNullOrWhiteSpace(aToken))
                wr.Headers.Add(HttpRequestHeader.Authorization, Constants.HeaderAuthorizationValue + aToken);
            wr.Method = method;
            return wr;
        }
    }
}
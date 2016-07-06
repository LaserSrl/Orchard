using Laser.Orchard.Vimeo.Extensions;
using Laser.Orchard.Vimeo.Models;
using Laser.Orchard.Vimeo.ViewModels;
using Orchard;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.UI.Notify;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Tasks.Scheduling;
using Orchard.Localization;

namespace Laser.Orchard.Vimeo.Services {
    public class VimeoServices : IVimeoServices {

        private readonly IRepository<VimeoSettingsPartRecord> _repositorySettings;
        private readonly IRepository<UploadsInProgressRecord> _repositoryUploadsInProgress;
        private readonly IRepository<UploadsCompleteRecord> _repositoryUploadsComplete;
        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _taskManager;

        public Localizer T { get; set; }

        public VimeoServices(IRepository<VimeoSettingsPartRecord> repositorySettings,
            IRepository<UploadsInProgressRecord> repositoryUploadsInProgress,
            IRepository<UploadsCompleteRecord> repositoryUploadsComplete,
            IOrchardServices orchardServices,
            IScheduledTaskManager taskManager) {

            _repositorySettings = repositorySettings;
            _repositoryUploadsInProgress = repositoryUploadsInProgress;
            _repositoryUploadsComplete = repositoryUploadsComplete;
            _orchardServices = orchardServices;
            _taskManager = taskManager;

            T = NullLocalizer.Instance;
        }

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
        public VimeoSettingsPartViewModel Get() {
            if (_repositorySettings.Table.Count() == 0)
                return null;

            VimeoSettingsPartRecord rec = _repositorySettings.Table.FirstOrDefault();
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
            settings.AccessToken = vm.AccessToken ?? "";
            settings.AlbumName = vm.AlbumName ?? "";
            settings.GroupName = vm.GroupName ?? "";
            settings.ChannelName = vm.ChannelName ?? "";

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
                        }
                    }
                }
            }

            settings.License = vm.License ?? "";
            settings.Privacy = vm.Privacy;
            settings.Password = vm.Password ?? "";
            settings.ReviewLink = vm.ReviewLink;
            settings.Locale = vm.Locale ?? "";
            settings.ContentRatings = vm.ContentRatingsSafe ?
                new string[] { "safe" }.ToList() :
                vm.ContentRatingsUnsafe.Where(cr => cr.Value == true).Select(cr => cr.Key).ToList();
            settings.Whitelist = vm.Whitelist.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            settings.AlwaysUploadToGroup = vm.AlwaysUploadToGroup;
            settings.AlwaysUploadToAlbum = vm.AlwaysUploadToAlbum;
            settings.AlwaysUploadToChannel = vm.AlwaysUploadToChannel;
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
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        return "OK";
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    if (resp.StatusCode == HttpStatusCode.BadRequest) {
                        return "Bad Request: on of the parameters is invalid. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
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
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        return "OK";
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    if (resp.StatusCode == HttpStatusCode.BadRequest) {
                        return "Bad Request: on of the parameters is invalid. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
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
                    if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created) {
                        return "OK";
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
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
            string queryString = "?fields=upload_quota";
            HttpWebRequest wr = VimeoCreateRequest(settings.AccessToken, VimeoEndpoints.Me, qString: queryString);
            VimeoUploadQuota quotaInfo = new VimeoUploadQuota();
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        using (var reader = new System.IO.StreamReader(resp.GetResponseStream())) {
                            string vimeoJson = reader.ReadToEnd();
                            JObject json = JObject.Parse(vimeoJson);
                            quotaInfo = JsonConvert.DeserializeObject<VimeoUploadQuota>(json["upload_quota"].ToString());
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
        /// Verifies that there is enough quota available.
        /// </summary>
        /// <param name="fileSize">The size of the file we would like to start uploading.</param>
        /// <returns>An id of an UploadsInProgressRecord corresponding to the upload we are starting if we have enough quota available for that upload. <value>-1</value> otherwise</returns>
        public int IsValidFileSize(int fileSize) {
            //this method, as it is, does not handle concurrent upload attempts very well.
            //We leave with Vimeo the responsiiblity for the final check on the upload size.

            //the information about the uploads in progress is in the UploadsInProgressRecord table.
            //We check the free quota with what we are trying to upload
            int quotaBeingUploaded = 0;
            if (_repositoryUploadsInProgress.Table.Count() > 0)
                quotaBeingUploaded = _repositoryUploadsInProgress.Table.Sum(u => u.UploadSize) - _repositoryUploadsInProgress.Table.Sum(u => u.UploadedSize);
            int remoteSpace = this.FreeQuota();
            if (remoteSpace - quotaBeingUploaded < fileSize) {
                return -1; //there is not enough space
            }

            //Add the file we want to upload to it, as a "temporary" upload
            UploadsInProgressRecord entity = new UploadsInProgressRecord();
            entity.UploadSize = fileSize;
            entity.CreatedTime = DateTime.UtcNow;
            _repositoryUploadsInProgress.Create(entity);
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
                return "";
            }
            return uploadUrl;
        }

        /// <summary>
        /// This method verifies in our records to check the state of an upload.
        /// </summary>
        /// <param name="uploadId">The Id of the upload we want to check</param>
        /// <returns>A value describing the state of the upload.</returns>
        public VerifyUploadResults VerifyUpload(int uploadId) {
            UploadsInProgressRecord entity = _repositoryUploadsInProgress.Get(uploadId);
            if (entity == null) {
                //could not find and Upload in progress with the given Id
                //Chek to see if the uplaod is complete
                UploadsCompleteRecord ucr = GetByProgressId(uploadId);
                if (ucr == null) {
                    //no, the upload actually never existed
                    return VerifyUploadResults.NeverExisted;
                }
                return VerifyUploadResults.CompletedAlready;
            }
            return VerifyUpload(entity);
        }

        /// <summary>
        /// This method verifies in our records to check the state of an upload.
        /// </summary>
        /// <param name="uploadId">The record corresponding to the upload</param>
        /// <returns>A value describing the state of the upload.</returns>
        public VerifyUploadResults VerifyUpload(UploadsInProgressRecord entity) {

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
                    int sent;
                    if (int.TryParse(range.Substring(range.IndexOf('-') + 1), out sent)) {
                        if (sent == entity.UploadSize) {
                            //Upload finished
                            return VerifyUploadResults.Complete;
                        } else {
                            //update the uploaded size
                            entity.UploadedSize = sent;
                            return VerifyUploadResults.Incomplete;
                        }
                    }
                }
            }
            return VerifyUploadResults.Error; //something went rather wrong
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
        /// We terminate the Vimeo upload stream.
        /// </summary>
        /// <param name="uploadId">The Id we have been using internally to identify the upload in progress.</param>
        /// <returns>The Id of the UploadCompleted, which we'll need to patch and publish the video.<value>-1</value> in case of errors.</returns>
        public int TerminateUpload(int uploadId) {
            UploadsCompleteRecord ucr = GetByProgressId(uploadId);
            if (ucr != null)
                return ucr.Id;
            UploadsInProgressRecord entity = _repositoryUploadsInProgress
                .Get(uploadId);
            return TerminateUpload(entity);
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
                    if (resp.StatusCode == HttpStatusCode.Created) {
                        //this is the success condition for this call:
                        //the response contains the video location in its "location" header
                        //create an entry of UploadsCompletedRecord for this upload
                        var ucr = new UploadsCompleteRecord();
                        ucr.Uri = resp.Headers["Location"];
                        ucr.ProgressId = entity.Id;
                        ucr.CreatedTime = DateTime.UtcNow;
                        _repositoryUploadsComplete.Create(ucr);
                        //delete the entry from uploads in progress
                        _repositoryUploadsInProgress.Delete(entity);
                        return ucr.Id;
                    }
                }
            } catch (Exception ex) {

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
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();
            //The things we want to change of the video go in the request body as a JSON.
            VimeoPatch patchData = new VimeoPatch {
                name = name,
                description = description,
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
            UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
            if (ucr != null) {
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
                        if (resp.StatusCode == HttpStatusCode.OK) {
                            return "OK";
                        }
                    }
                } catch (Exception ex) {
                    HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                    //if some parameter is wrong in the patch, we get status code 400 Bad Request
                    if (resp != null && resp.StatusCode == HttpStatusCode.BadRequest) {
                        return new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    }
                    return resp.StatusCode.ToString() + " " + resp.StatusDescription;
                }
            }
            return "Record is null";
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
                                            return gr.uri.Substring(gr.uri.IndexOf("/", 1) + 1);
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
                                            return ch.uri.Substring(ch.uri.IndexOf("/", 1) + 1);
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
                                        if(al.name==settings.AlbumName){
                                            return al.uri.Substring(al.uri.IndexOf("/", 1) + 1);
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
                
            }

            return null;
        }

        /// <summary>
        /// Add the video corresponding to the Completed Upload whose id is passed to the group stored in the settings
        /// </summary>
        /// <param name="ucId">The Id of a completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string AddVideoToGroup(int ucId) {
            string groupId = GetGroupId();
            if (!string.IsNullOrWhiteSpace(groupId)) {
                UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
                if (ucr != null) {
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
                            if (resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.NoContent) {
                                return "OK";
                            }
                        }
                    } catch (Exception ex) {
                        HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                        if (resp != null) {
                            if (resp.StatusCode == HttpStatusCode.Forbidden) {
                                return "Access Denied: cannot add video. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                            }
                        }
                    }
                } else {
                    return "Cannot identify video";
                }
            } else {
                return "Cannot access group";
            }
            return "Unknown error";
        }
        /// <summary>
        /// Add the video corresponding to the Completed Upload whose id is passed to the channel stored in the settings
        /// </summary>
        /// <param name="ucId">The Id of a completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string AddVideoToChannel(int ucId) {
            string chanId = GetChannelId();
            if (!string.IsNullOrWhiteSpace(chanId)) {
                UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
                if (ucr != null) {
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
                            if (resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.NoContent) {
                                return "OK";
                            }
                        }
                    } catch (Exception ex) {
                        HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                        if (resp != null) {
                            if (resp.StatusCode == HttpStatusCode.Forbidden) {
                                return "Access Denied: cannot add video. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                            } else if (resp.StatusCode == HttpStatusCode.NotFound) {
                                return "Resource not found. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                            }
                        }
                    }
                } else {
                    return "Cannot identify video";
                }
            } else {
                return "Cannot access channel";
            }
            return "Unknown error";
        }
        /// <summary>
        /// Add the video corresponding to the Completed Upload whose id is passed to the album stored in the settings
        /// </summary>
        /// <param name="ucId">The Id of a completed upload</param>
        /// <returns>A <type>string</type> describing the result of the operation. <value>"OK"</value> in case of success.</returns>
        public string AddVideoToAlbum(int ucId) {
            string alId = GetAlbumId();
            if (!string.IsNullOrWhiteSpace(alId)) {
                UploadsCompleteRecord ucr = _repositoryUploadsComplete.Get(ucId);
                if (ucr != null) {
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
                            if (resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.NoContent) {
                                return "OK";
                            }
                        }
                    } catch (Exception ex) {
                        HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                        if (resp != null) {
                            if (resp.StatusCode == HttpStatusCode.Forbidden) {
                                return "Access Denied: cannot add video. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                            } else if (resp.StatusCode == HttpStatusCode.NotFound) {
                                return "Resource not found. " + new StreamReader(resp.GetResponseStream()).ReadToEnd();
                            }
                        }
                    }
                } else {
                    return "Cannot identify video";
                }
            } else {
                return "Cannot access album";
            }
            return "Unknown error";
        }

        //TODO: CODE FOR TASKS
        public void VerifyAllUploads() {
            foreach (var uip in _repositoryUploadsInProgress.Table.ToList()) {
                switch (VerifyUpload(uip)) {
                    case VerifyUploadResults.CompletedAlready:
                        break;
                    case VerifyUploadResults.Complete:
                        TerminateUpload(uip);
                        break;
                    case VerifyUploadResults.Incomplete:
                        break;
                    case VerifyUploadResults.NeverExisted:
                        break;
                    case VerifyUploadResults.Error:
                        break;
                    default:
                        break;
                }
            }
        }

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
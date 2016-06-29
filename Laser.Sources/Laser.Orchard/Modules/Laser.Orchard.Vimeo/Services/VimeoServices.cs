using Laser.Orchard.Vimeo.Extensions;
using Laser.Orchard.Vimeo.Models;
using Laser.Orchard.Vimeo.ViewModels;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Laser.Orchard.Vimeo.Services {
    public class VimeoServices : IVimeoServices {

        private readonly IRepository<VimeoSettingsPartRecord> _repository;

        public VimeoServices(IRepository<VimeoSettingsPartRecord> repository) {
            _repository = repository;
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
            if(_repository.Table.Count()==0)
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
        /// Verifies whether the token in the ViewModel is valid by attempting an API request
        /// </summary>
        /// <param name="vm">The settings ViewModel to test.</param>
        /// <returns><value>true</value> if the access token is authenticated and valid. <value>false</value> otherwise.</returns>
        public bool TokenIsValid(VimeoSettingsPartViewModel vm) {
            return this.TokenIsValid(vm.AccessToken);
        }
        /// <summary>
        /// Verifies whether the token is valid by attempting an API request
        /// </summary>
        /// <param name="aToken">The Access Token to test.</param>
        /// <returns><value>true</value> if the access token is authenticated and valid. <value>false</value> otherwise.</returns>
        public bool TokenIsValid(string aToken) {
            HttpWebRequest wr = HttpWebRequest.CreateHttp(VimeoEndpoints.Me); //WebRequest.Create(VimeoEndpoints.Me);
            wr.Accept = Constants.HeaderAcceptValue;
            wr.Headers.Add(HttpRequestHeader.Authorization, Constants.HeaderAuthorizationValue + aToken);
            wr.Method = WebRequestMethods.Http.Get;

            bool ret = false;
            try {
                HttpWebResponse resp = (HttpWebResponse)wr.GetResponse();
                ret = resp.StatusCode == HttpStatusCode.OK;
            } catch (Exception ex) {
                ret = false;
            }
            
            
            return ret;
        }

         
    }
}
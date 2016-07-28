using System.Collections.Generic;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard;
using Orchard.Data;
using Orchard.Security;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IUserProviderServices : IDependency {
        UserProviderRecord Get(string providerName, string providerUserId);
        void Create(string providerName, string providerUserId, UserAccountLogin user);
        void Update(string providerName, string providerUserId, UserAccountLogin user);
        IEnumerable<UserProviderRecord> Get(IUser user);
        IEnumerable<UserProviderRecord> Get(int userId);
    }

    public class UserProviderServices : IUserProviderServices {
        private readonly IRepository<UserProviderRecord> _repository;

        public UserProviderServices(IRepository<UserProviderRecord> repository) {
            _repository = repository;
        }

        public UserProviderRecord Get(string providerName, string providerUserId) {
            return _repository.Get(o => o.ProviderName == providerName && o.ProviderUserId == providerUserId);
        }

        public IEnumerable<UserProviderRecord> Get(IUser user) {
            return Get(user.Id);
        }

        public IEnumerable<UserProviderRecord> Get(int userId) {
            return _repository.Fetch(o => o.UserId == userId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="providerUserId"></param>
        /// <param name="user"></param>
        public void Create(string providerName, string providerUserId, UserAccountLogin user) {

            Dictionary<string, string> userParn = new Dictionary<string, string>();
            userParn = GetListValue(user);

            var serializer = new JavaScriptSerializer();
            var serializedResult = serializer.Serialize(userParn);

            var record = new UserProviderRecord
                {
                    UserId = user.Id,
                    ProviderName = providerName,
                    ProviderUserId = providerUserId,
                    ProviderUserData = serializedResult
                    //ProviderUserData = ((user.UserName != string.Empty) ? user.UserName : "") + " " + ((user.FirstName != string.Empty) ? user.FirstName : "") +
                    //                    " " + ((user.Name != string.Empty) ? user.Name : "") + " " + ((user.Email != string.Empty) ? user.Email : "") + " " + ((user.Sesso != string.Empty) ? user.Sesso : "")
                };

            _repository.Create(record);
        }

        public void Update(string providerName, string providerUserId, UserAccountLogin user) {
            var record = Get(providerName, providerUserId);

            record.UserId = user.Id;

            Dictionary<string, string> userParn = new Dictionary<string, string>();
            userParn= GetListValue(user);

            var serializer = new JavaScriptSerializer();
            var serializedResult = serializer.Serialize(userParn);

            //record.ProviderUserData = ((user.UserName != string.Empty) ? user.UserName : "") + " " + ((user.FirstName != string.Empty) ? user.FirstName : "") +
            //                            " " + ((user.Name != string.Empty) ? user.Name : "") + " " + ((user.Email != string.Empty) ? user.Email : "") + " " + ((user.Sesso != string.Empty) ? user.Sesso : "");
            record.ProviderUserData = serializedResult;
            _repository.Update(record);
        }


        public Dictionary<string, string> GetListValue(UserAccountLogin user) {
            Dictionary<string, string> userPar = new Dictionary<string, string>();
            userPar.Add("FirstName",user.FirstName);
            userPar.Add("Name", user.Name);
            userPar.Add("UserName",user.UserName);
            userPar.Add("Email", user.Email);
            userPar.Add("Sesso", user.Sesso);
            return userPar;
        }

    }
}
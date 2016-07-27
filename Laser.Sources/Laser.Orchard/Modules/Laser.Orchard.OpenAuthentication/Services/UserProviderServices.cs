using System.Collections.Generic;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard;
using Orchard.Data;
using Orchard.Security;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IUserProviderServices : IDependency {
        UserProviderRecord Get(string providerName, string providerUserId);
        void Create(string providerName, string providerUserId, UserAccountLogin user);
        void Update(string providerName, string providerUserId, UserAccountLogin user);
        IEnumerable<UserProviderRecord> Get(UserAccountLogin user);
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

        public IEnumerable<UserProviderRecord> Get(UserAccountLogin user) {
            return Get(user.Id);
        }

        public IEnumerable<UserProviderRecord> Get(int userId) {
            return _repository.Fetch(o => o.UserId == userId);
        }

        public void Create(string providerName, string providerUserId, UserAccountLogin user) {
            var record = new UserProviderRecord
                {
                    UserId = user.Id,
                    ProviderName = providerName,
                    ProviderUserId = providerUserId,
                    ProviderUserData = ((user.UserName != string.Empty) ? user.UserName : "") + " " + ((user.FirstName != string.Empty) ? user.FirstName : "") +
                                        " " + ((user.Name != string.Empty) ? user.Name : "") + " " + ((user.Email != string.Empty) ? user.Email : "") + " " + ((user.Sesso != string.Empty) ? user.Sesso : "")
                };

            _repository.Create(record);
        }

        public void Update(string providerName, string providerUserId, UserAccountLogin user) {
            var record = Get(providerName, providerUserId);

            record.UserId = user.Id;
            record.ProviderUserData = ((user.UserName != string.Empty) ? user.UserName : "") + " " + ((user.FirstName != string.Empty) ? user.FirstName : "") +
                                        " " + ((user.Name != string.Empty) ? user.Name : "") + " " + ((user.Email != string.Empty) ? user.Email : "") + " " + ((user.Sesso != string.Empty) ? user.Sesso : "");
            _repository.Update(record);
        }
    }
}
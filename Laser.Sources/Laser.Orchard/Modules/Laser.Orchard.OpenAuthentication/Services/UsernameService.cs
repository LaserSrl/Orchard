using Laser.Orchard.OpenAuthentication.Extensions;
using Orchard;
using Orchard.Security;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IUsernameService : IDependency {
        string Calculate(string currentValue, string provider);
    }

    public class UsernameService : IUsernameService {
        private readonly IMembershipService _membershipService;

        public UsernameService(IMembershipService membershipService) {
            _membershipService = membershipService;
        }

        
        public string Calculate(string currentValue, string provider) {
            /* I Dont want to user an email address as a Username...*/
            string userName = string.Empty;

            if (provider == "facebook" || provider=="google") {
                userName = currentValue.IsEmailAddress() ? currentValue.Substring(0, currentValue.IndexOf('@')) : currentValue;
            } 
            else 
            {
                userName = currentValue;
            }

            int uniqueValue = 0;

            string newUniqueUserName = userName;

            while (true) {
                if (_membershipService.GetUser(newUniqueUserName) == null)
                    break;

                newUniqueUserName = string.Format("{0}{1}", userName, uniqueValue);

                uniqueValue++;
            }

            return newUniqueUserName;
        }
    }
}
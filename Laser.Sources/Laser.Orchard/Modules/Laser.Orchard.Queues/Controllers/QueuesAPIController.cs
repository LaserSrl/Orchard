using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Models;
using System.Web.Mvc;

namespace Laser.Orchard.Queues.Controllers
{
    public class QueuesAPIController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IQueuesService _queuesService;

        public QueuesAPIController(IAuthenticationService authenticationService, IQueuesService queuesService)
        {
            _authenticationService = authenticationService;
            _queuesService = queuesService;
        }

        [System.Web.Mvc.HttpGet]
        public void RegisterNumber(string queueName, int number)
        {
            try
            {
                var authenticatedUser = _authenticationService.GetAuthenticatedUser();
                if (authenticatedUser != null)
                    _queuesService.RegisterNumberForUser(authenticatedUser.As<UserPart>(), queueName, number);
            }
            catch
            { }
        }
    }
}
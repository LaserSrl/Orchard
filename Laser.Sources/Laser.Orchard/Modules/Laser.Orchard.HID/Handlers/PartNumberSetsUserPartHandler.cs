using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Users.Models;

namespace Laser.Orchard.HID.Handlers {
    public class PartNumberSetsUserPartHandler : ContentHandler {

        private readonly IHIDPartNumbersService _HIDPartNumbersService;
        private readonly IHIDCredentialsService _HIDCredentialsService;

        public PartNumberSetsUserPartHandler(
            IRepository<PartNumberSetsUserPartRecord> repository,
            IHIDPartNumbersService HIDPartNumbersService,
            IHIDCredentialsService HIDCredentialsService) {

            _HIDPartNumbersService = HIDPartNumbersService;
            _HIDCredentialsService = HIDCredentialsService;

            Filters.Add(StorageFilter.For(repository));

            // sneakily attach the part to users
            Filters.Add(new ActivatingFilter<PartNumberSetsUserPart>("User"));

            // When a user is deleted, revoke all its credentials
            OnRemoving<PartNumberSetsUserPart>(RevokeAllCredentials);
        }

        public void RevokeAllCredentials(RemoveContentContext context, PartNumberSetsUserPart part) {
            var user = part.As<UserPart>();
            if (user != null) {
                _HIDCredentialsService.RevokeCredentials(user, _HIDPartNumbersService.GetPartNumbersForUser(user));
            }
        }
    }
}
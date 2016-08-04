using Laser.Orchard.UserReactions.Fields;
using Laser.Orchard.UserReactions.Settings;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Drivers {
    public class UserReactionsFieldDriver : ContentFieldDriver<UserReactionsField> {

        public Localizer T { get; set; }
        private readonly IOrchardServices _orchardServices;

        public UserReactionsFieldDriver(IOrchardServices orchardServices) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
        }


        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Value"), T("The string associated with the field."))
                .Enumerate<UserReactionsField>(() => field => new[] { field.Value });
        }

    }
}
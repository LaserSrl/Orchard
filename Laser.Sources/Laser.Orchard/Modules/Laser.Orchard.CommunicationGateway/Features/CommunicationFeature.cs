using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment;
using Orchard.Localization;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.Features {

    public class CommunicationFeature : IFeatureEventHandler{
        private readonly IOrchardServices _orchardServices;
        private readonly IContentExtensionsServices _contentExtensionsServices;

        public CommunicationFeature(IOrchardServices orchardServices, IContentExtensionsServices contentExtensionsServices) {
            _orchardServices = orchardServices;
            _contentExtensionsServices = contentExtensionsServices;
        }

        public void Disabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Disabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Enabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            if (feature.Descriptor.Name == "Laser.Orchard.CommunicationGateway") {
                //             // Copy all profile from users to contact
                List<Int32> contactsUsers = new List<int>();
                var users = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().List();
                //TODO test with 1 Communication inserted
                if (_orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Count() > 0) {
                    contactsUsers = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().List().Select(y => y.As<CommunicationContactPart>().UserIdentifier).ToList();
                }
                var userWithNoConcat = users.Where(x => !contactsUsers.Contains(x.Id));
                foreach (var user in userWithNoConcat) {
                    ContentItem Contact = _orchardServices.ContentManager.New("CommunicationContact");
              
                    Contact.As<TitlePart>().Title = user.Email + " " + user.UserName;
                    bool asProfilePart = false;

                    try {
                        var profpart = ((dynamic)user).ContentItem.ProfilePart;
                        asProfilePart = true;
                    }
                    catch { asProfilePart = false; }
                    _orchardServices.ContentManager.Create(Contact);
               
                    dynamic mypart = (((dynamic)Contact).CommunicationContactPart);
                    mypart.GetType().GetProperty("UserIdentifier").SetValue(mypart, user.Id, null);
                  //  Contact.As<CommunicationContactPart>().User_Id = user.Id;
                    if (asProfilePart) {
                        List<ContentPart> Lcp = new List<ContentPart>();
                        Lcp.Add(((ContentPart)((dynamic)Contact).ProfilePart));
                        foreach (dynamic cf in ((dynamic)user).ProfilePart.Fields) {
                            _contentExtensionsServices.StoreInspectExpandoFields(Lcp, ((string)((dynamic)cf).Name), ((object)(((dynamic)cf).Value)), Contact);
                        }
                    }
                }
            }
        }

        public void Enabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Installed(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Installing(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Uninstalled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Uninstalling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

    }
}
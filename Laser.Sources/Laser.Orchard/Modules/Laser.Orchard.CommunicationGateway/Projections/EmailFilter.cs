using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.StartupConfig.Projections;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.FilterEditors;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.CommunicationGateway.Projections {
    public class EmailFilter : IFilterProvider {

        //private StringFilterEditor _stringFilterEditor;

        public EmailFilter(StringFilterEditor stringFilterEditor) {
            //_stringFilterEditor = stringFilterEditor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("CommunicationContacts", T("Communication Contacts"), T("Communication Contacts"))
                .Element("Email", T("Email"), T("Contacts with the specified email"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "StringFilter"
                );
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;

            string subquery = @"SELECT contact.EmailContactPartRecord_Id as contactId
                                FROM Laser.Orchard.CommunicationGateway.Models.CommunicationEmailRecord AS contact
                                WHERE contact.Email = :email";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("email", context.State.Value);

            context.Query = query.Where(x => x.ContentPartRecord<CommunicationContactPartRecord>(), x => x.InSubquery("Id", subquery, parameters));
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Filter contacts with the specified email.");
        }
    }
}
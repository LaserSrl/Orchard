using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.CommunicationGateway.Navigation {

    public class AdminMenu : INavigationProvider {

        public string MenuName {
            get { return "admin"; }
        }

        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(item => item
                .Caption(T("Communication"))
                .Position("1.06")
                      .LinkToFirstChild(false)
                .AddImageSet("CommunicationGateway")
                //
                 .Add(sub1 => sub1
                     .Caption(T("Campaign"))
                          .Position("1.061")
                            .Action("Index", "CampaignAdmin", new { area = "Laser.Orchard.CommunicationGateway" })
                //   .LocalNav()
                 )
                .Add(sub2 => sub2
                     .Caption(T("Flash advertising"))
                          .Position("1.062")
                            .Action("Index", "AdvertisingAdmin", new { area = "Laser.Orchard.CommunicationGateway", id = -10 })
                //   .LocalNav()
                 )
                 .Add(sub3 => sub3
                     .Caption(T("Contacts"))
                          .Position("1.063")
                            .Action("Index", "ContactsAdmin", new { area = "Laser.Orchard.CommunicationGateway" })
                //   .LocalNav()
                 )
                    .Add(sub4 => sub4
                     .Caption(T("Settings"))
                          .Position("1.064")
                // .Action("Index", "ContactsAdmin", new { area = "Laser.Orchard.CommunicationGateway" })
                //   .LocalNav()
                 )
                // .Add(sub2 => sub2
                //     .Caption(T("Setting"))
                //          .Position("1.064")
                //            .Action("Facebook", "ActivatingSocial", new { area = "Laser.Orchard.CommunicationGateway" })
                ////   .LocalNav()
                // )

       );
        }
    }
}
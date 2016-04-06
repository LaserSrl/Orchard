using Laser.Orchard.CommunicationGateway.ViewModels;
namespace Laser.Orchard.CommunicationGateway {

    public class SearchVM {
        public string Expression { get; set; }
        public SearchFieldEnum Field { get; set; }

        public SearchVM() {
            Field = SearchFieldEnum.Name; // default value
        }
    }
}
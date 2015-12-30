using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.ViewModels {
    public class View_EmailVM {
        public List<View_EmailVM_element> Elenco { get; set; }
        public View_EmailVM() {
            Elenco = new List<View_EmailVM_element>();
        }
    }
    public class View_EmailVM_element {
        public int Id { get; set; }
        public int CommunicationContactPartRecord_Id { get; set; }
        public string Language { get; set; }
        public bool Validated { get; set; }
        public DateTime DataInserimento { get; set; }
        public DateTime DataModifica { get; set; }
        public string Email { get; set; }
        public bool Produzione { get; set; }
    }
}
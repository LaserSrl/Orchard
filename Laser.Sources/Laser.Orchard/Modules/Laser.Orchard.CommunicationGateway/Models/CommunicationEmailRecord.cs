using System;

namespace Laser.Orchard.CommunicationGateway.Models {

    public class CommunicationEmailRecord {
        public virtual int Id { get; set; }
        public virtual int CommunicationContactPartRecord_Id { get; set; }
        public virtual string Language { get; set; }
        public virtual bool Validated { get; set; }
        public virtual DateTime DataInserimento { get; set; }
        public virtual DateTime DataModifica { get; set; }
        public virtual string Email { get; set; }
        public virtual bool Produzione { get; set; }
    }
}
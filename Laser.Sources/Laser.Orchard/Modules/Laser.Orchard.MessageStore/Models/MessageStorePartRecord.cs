using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;

namespace Laser.Orchard.MessageStore.Models {
    public class MessageStorePartRecord : ContentPartRecord {
        public MessageStorePartRecord() {
            FilterTest = "";
            MessageDate = DateTime.Now;
        }
        [StringLengthMax]
        public virtual string MessageText { get; set; }
        public virtual int Gruppoid { get; set; }
        public virtual string MessageFrom { get; set; }
        public virtual string MessageTo { get; set; }
        public virtual DateTime MessageDate { get; set; }
        public virtual bool MarkRead { get; set; }
        public virtual bool MarkMessage { get; set; }
        public virtual string FilterTest { get; set; }
        [StringLengthMax]
        public virtual string MessageTextHtml { get; set; }
        public virtual string MessageObject { get; set; }
    }
}
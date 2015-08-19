using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.MessageStore.ViewModels {
    public class MessageStoreEditModel {
        public virtual int Id { get; set; }
        [StringLengthMax]
        public virtual string MessageText { get; set; }

        public virtual int Gruppoid { get; set; }

        public virtual string MessageFrom { get; set; }

        public virtual string MessageTo { get; set; }

        public virtual DateTime MessageDate { get; set; }

        public virtual bool MarkRead { get; set; }

        public virtual string FilterTest { get; set; }
        public virtual bool MarkMessage { get; set; }
        [StringLengthMax]
        public virtual string MessageTextHtml { get; set; }
        public virtual string MessageObject { get; set; }
        public virtual bool showOnlyMessage { get; set; } // usato solo per l'edit
        public virtual bool CloseWindow { get; set; } // usato solo per l'edit
        public virtual bool Send { get; set; }
        public virtual List<string> Attachment{get; set; }
    }
}

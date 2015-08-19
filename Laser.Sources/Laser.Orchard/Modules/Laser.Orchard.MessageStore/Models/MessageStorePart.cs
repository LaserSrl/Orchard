using Orchard.ContentManagement;
using Orchard.Data.Conventions;
using System;

namespace Laser.Orchard.MessageStore.Models {
    public class MessageStorePart : ContentPart<MessageStorePartRecord> {
        [StringLengthMax]
        public string MessageText {
            get { return Record.MessageText; }
            set { Record.MessageText = value; }
        }
        public int Gruppoid {
            get { return Record.Gruppoid; }
            set { Record.Gruppoid = value; }
        }
        public string MessageFrom {
            get { return Record.MessageFrom; }
            set { Record.MessageFrom = value; }
        }
        public string MessageTo {
            get { return Record.MessageTo; }
            set { Record.MessageTo = value; }
        }
        public DateTime MessageDate {
            get { return Record.MessageDate; }
            set { Record.MessageDate = value; }
        }
        public bool MarkRead {
            get { return Record.MarkRead; }
            set { Record.MarkRead = value; }
        }
        public bool MarkMessage {
            get { return Record.MarkMessage; }
            set { Record.MarkMessage = value; }
        }
        public string FilterTest {
            get { return Record.FilterTest; }
            set { Record.FilterTest = value; }
        }
        [StringLengthMax]
        public string MessageTextHtml {
            get { return Record.MessageTextHtml; }
            set { Record.MessageTextHtml = value; }
        }
        public string MessageObject {
            get { return Record.MessageObject; }
            set { Record.MessageObject = value; }
        }
    }
}
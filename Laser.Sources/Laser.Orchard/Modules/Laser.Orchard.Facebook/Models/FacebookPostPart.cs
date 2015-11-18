using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.ComponentModel;
using System.Linq;

namespace Laser.Orchard.Facebook.Models {

    public class FacebookPostPart : ContentPart<FacebookPostPartRecord> {

        [DisplayName("Facebook")]
        public string FacebookMessage {
            get { return this.Retrieve(r => r.FacebookMessage); }
            set { this.Store(r => r.FacebookMessage, value); }
        }

        public bool FacebookMessageSent {
            get { return this.Retrieve(r => r.FacebookMessageSent); }
            set { this.Store(r => r.FacebookMessageSent, value); }
        }

        public string FacebookCaption {
            get { return this.Retrieve(r => r.FacebookCaption); }
            set { this.Store(r => r.FacebookCaption, value); }
        }

        public string FacebookDescription {
            get { return this.Retrieve(r => r.FacebookDescription); }
            set { this.Store(r => r.FacebookDescription, value); }
        }

        public string FacebookName {
            get { return this.Retrieve(r => r.FacebookName); }
            set { this.Store(r => r.FacebookName, value); }
        }

        public string FacebookPicture {
            get { return this.Retrieve(r => r.FacebookPicture); }
            set { this.Store(r => r.FacebookPicture, value); }
        }

        public string FacebookLink {
            get { return this.Retrieve(r => r.FacebookLink); }
            set { this.Store(r => r.FacebookLink, value); }
        }

        public Int32[] AccountList {
            get {
                try {
                    string tmp = this.Retrieve(r => r.AccountList);
                    return (tmp ?? "").Split(',').Select(Int32.Parse).ToArray();
                }
                catch (Exception ex) {
                    return new Int32[] { };
                }
            }
            set { this.Store(r => r.AccountList, string.Join(",", value)); }
        }
    }

    public class FacebookPostPartRecord : ContentPartRecord {

        [StringLengthMax]
        public virtual string FacebookMessage { get; set; }

        public virtual bool FacebookMessageSent { get; set; }
        public virtual string FacebookCaption { get; set; }

        [StringLengthMax]
        public virtual string FacebookDescription { get; set; }

        public virtual string FacebookName { get; set; }
        public virtual string FacebookPicture { get; set; }
        public virtual string FacebookLink { get; set; }
        public virtual string AccountList { get; set; }
    }
}
using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Models {
    /// <summary>
    /// This class basically maps BulkCredentialsOperationsContext.UserCredentialsActions
    /// to a record.
    /// </summary>
    public class BulkCredentialsOperationsRecord {
        public virtual int Id { get; set; } // Primary key
        public virtual int TaskId { get; set; } // we fetch based on this, so it may make sense to index it
        public virtual int UserId { get; set; }
        [StringLengthMax]
        public virtual string SerializedRevokeList { get; set; }
        [StringLengthMax]
        public virtual string SerializedIssueList { get; set; }
    }
}
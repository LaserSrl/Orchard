using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.DynamicTablePart")]
    public class DynamicTablePart : ContentPart<DynamicTablePartRecord> {
        public string TableData {
            get { return Retrieve(r => r.TableData); }
            set { Store(r => r.TableData, value); }
        }
        public IEnumerable<object> DataList {
            get {
                var result = new List<object>();
                result.Add(new { Name = "Pippo", Value = 34, Start = new DateTime(2018, 2, 3) });
                result.Add(new { Name = "Pluto", Value = 45, Start = new DateTime(2017, 12, 13) });
                result.Add(new { Name = "Paperino", Value = 78, Start = new DateTime(2018, 1, 31) });
                return result;
            }
        }
    }
    [OrchardFeature("Laser.Orchard.StartupConfig.DynamicTablePart")]
    public class DynamicTablePartRecord : ContentPartVersionRecord {
        public virtual string TableData { get; set; }
    }
}
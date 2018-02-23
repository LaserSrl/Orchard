using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Helpers;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.DynamicTablePart")]
    public class DynamicTablePart : ContentPart<DynamicTablePartRecord> {
        public string TableData {
            get {
                var aux = Retrieve(r => r.TableData);
                if (string.IsNullOrWhiteSpace(aux)) {
                    aux = "[]";
                }
                return aux;
            }
            set {
                if(string.IsNullOrWhiteSpace(value) == false) {
                    Store(r => r.TableData, value);
                }
            }
        }
        public DynamicJsonArray DataList {
            get {
                DynamicJsonObject djo = null;
                var arr = new List<object>();
                var aux = JArray.Parse(TableData);
                foreach(var el in aux) {
                    var a1 = el.ToObject<Dictionary<string, object>>();
                    arr.Add(new DynamicJsonObject(a1));
                }
                return new DynamicJsonArray(arr.ToArray());
            }
        }
    }
    [OrchardFeature("Laser.Orchard.StartupConfig.DynamicTablePart")]
    public class DynamicTablePartRecord : ContentPartVersionRecord {
        private string _tableData = "[]"; // default value
        public virtual string TableData {
            get {
                return _tableData;
            }
            set {
                if(string.IsNullOrWhiteSpace(value) == false) {
                    _tableData = value;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Fields {
    public interface ICustomField {
        List<CustomFieldValue> FieldValueList { get; }
        void SetFieldValue(string valueName, object value);
    }
}
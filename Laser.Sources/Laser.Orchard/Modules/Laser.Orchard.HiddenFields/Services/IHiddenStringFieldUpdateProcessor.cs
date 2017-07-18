using Laser.Orchard.HiddenFields.Fields;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.HiddenFields.Services {
    public enum HiddenStringFieldUpdateProcessVariant {
        None,
        All,
        Empty
    }
    public interface IHiddenStringFieldUpdateProcessor : IDependency {

        void Process(IEnumerable<HiddenStringField> fields);
    }
}

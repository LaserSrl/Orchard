namespace Laser.Orchard.StartupConfig.Models {
    public class DynamicTablePartSettings {
        private string _columnsDefinition = "[{field:'name',title:'Name'},{field:'description',title:'Description'}]"; // default value
        private string _uniqueId = "id"; // default value
        public string ColumnsDefinition {
            get {
                return _columnsDefinition;
            }
            set {
                if(string.IsNullOrWhiteSpace(value) == false) {
                    _columnsDefinition = value;
                }
            }
        }
        public string UniqueId {
            get {
                return _uniqueId;
            }
            set {
                if(string.IsNullOrWhiteSpace(value) == false) {
                    _uniqueId = value;
                }
            }
        }
        public bool CardView { get; set; }
    }
}
using Laser.Orchard.ExternalContent.Settings;
using Orchard.ContentManagement;


namespace Laser.Orchard.ExternalContent.Fields {
    public class FieldExternal : ContentField {
        private dynamic _contentObject;
        private dynamic _ContentUrl;
        public string ExternalUrl {
            get { return Storage.Get<string>("ExternalUrl"); }
            set { Storage.Set("ExternalUrl", value); }
        }



        public dynamic ContentObject {
            get { return _contentObject; }
            set { _contentObject = value; }
        }
        public FieldExternalSetting Setting {
            get {return this.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>(); }
            
        }
        public dynamic ContentUrl{
            get { return _ContentUrl; }
            set { _ContentUrl = value; }
        }
    }
}
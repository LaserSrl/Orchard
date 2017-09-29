using System.Collections.Generic;

namespace Laser.Orchard.AppDirect.ViewModels {

    public class ListAppDirectSettingVM {
        public IEnumerable<AppDirectSettingVM> ListAppDirectSetting { get; set; }
    }
    public class AppDirectSettingVM {
        public virtual int Id { get; set; }
        public virtual string Key { get; set; }
        public virtual string ConsumerKey { get; set; }
        public virtual string ConsumerSecret { get; set; }
        public virtual bool Delete { get; set; }
    }
}
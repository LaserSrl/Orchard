using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using System.Web;

namespace Laser.Orchard.Mobile.Services {
    public interface IMylogService : IDependency {
        void WriteLog(string text);
    }
    public class MylogService : IMylogService {
        private string _tenant;
        private string _url;
        public ILogger Logger { get; set; }
        public MylogService(ShellSettings shellSetting) {
            Logger = NullLogger.Instance;
            if (shellSetting != null) {
                _tenant = shellSetting.Name;
            }
            if (HttpContext.Current != null) {
                _url = HttpContext.Current.Request.Url.ToString();
            }
        }

        public void WriteLog(string text) {
            log4net.ThreadContext.Properties["Tenant"] = _tenant;
            log4net.ThreadContext.Properties["Url"] = _url;
            Logger.Error(text);
        }
    }
}

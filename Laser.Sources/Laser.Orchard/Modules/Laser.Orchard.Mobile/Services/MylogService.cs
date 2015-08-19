using Orchard;
using Orchard.Logging;

namespace Laser.Orchard.Mobile.Services {
    public interface IMylogService : IDependency {
        void WriteLog(string text);
    }
    public class MylogService : IMylogService {
        public ILogger Logger { get; set; }
        public MylogService() {
            Logger = NullLogger.Instance;
        }

        public void WriteLog(string text) {
            Logger.Error(text);
        }
    }
}

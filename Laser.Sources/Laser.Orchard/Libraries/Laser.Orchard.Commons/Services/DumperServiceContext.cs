using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Laser.Orchard.Commons.Services {
    public class DumperServiceContext {
        /// <summary>
        /// The content we are dumping
        /// </summary>
        public IContent Content { get; set; }

        /// <summary>
        /// The list of all the lists taht we dumped
        /// </summary>
        public List<string> ContentLists { get; set; }

        public Func<ObjectDumper> GetDumper { get; set; }

        public bool Minified { get; set; }
        public bool RealFormat { get; set; }

        public DumperServiceContext(
            IContent content,
            Func<ObjectDumper> dumperConstructor, 
            bool minified,
            bool realformat) {

            Content = content;
            GetDumper = dumperConstructor;
            Minified = minified;
            RealFormat = realformat;

            ContentLists = new List<string>();
        }

        public void ConvertToJson(XElement x, StringBuilder sb) {
            JsonConverter.ConvertToJSon(x, sb, Minified, RealFormat);
        }
    }
}

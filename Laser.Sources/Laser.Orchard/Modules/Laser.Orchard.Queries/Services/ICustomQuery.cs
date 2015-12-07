using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Queries.Helpers;


namespace Laser.Orchard.Queries.Services {
    public interface ICustomQuery : IDependency {
        Dictionary<String, Int32> Get(string option);
    }
    public class CustomQuery : ICustomQuery {
        private readonly IOrchardServices _orchardServices;
        public CustomQuery(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public Dictionary<String, Int32> Get(string option) {
            Dictionary<String, Int32> Listquery = new Dictionary<string, int>();
            IEnumerable<ContentItem> enumCI = _orchardServices.ContentManager.Query().ForType("MyCustomQuery").List().Where(x => ((dynamic)x).MyCustomQueryPart.Options.Value.ToString().Contains(option));
            foreach (ContentItem ci in enumCI) {
                Listquery.Add(ci.As<TitlePart>().Title, ci.Id);
            }
            return Listquery;
        }
    }
}

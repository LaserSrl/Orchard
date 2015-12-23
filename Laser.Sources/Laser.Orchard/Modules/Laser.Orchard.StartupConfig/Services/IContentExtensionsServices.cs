using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Dynamic;

namespace Laser.Orchard.StartupConfig.Services {

    public interface IContentExtensionsServices : IDependency {

        IEnumerable<ParentContent> ContentPickerParents(int contentId, string[] contentTypes);

        Response StoreInspectExpando(ExpandoObject theExpando, ContentItem TheContentItem);
        void StoreInspectExpandoFields(List<ContentPart> listpart, string key, object value, ContentItem theContentItem);
   

        bool FileAllowed(string filename);
    }
}
using Laser.Orchard.Vimeo.Models;
using Laser.Orchard.Vimeo.ViewModels;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.Vimeo.Services {
    public interface IVimeoServices : IDependency {
        bool Create(VimeoSettingsPartViewModel settings);
        VimeoSettingsPartViewModel GetByToken(string aToken);
        VimeoSettingsPartViewModel Get();
        void UpdateSettings(VimeoSettingsPartViewModel vm);

        bool TokenIsValid(VimeoSettingsPartViewModel vm);
        bool TokenIsValid(string aToken);

        bool GroupIsValid(VimeoSettingsPartViewModel vm);
        bool GroupIsValid(string gName, string aToken);
        bool AlbumIsValid(VimeoSettingsPartViewModel vm);
        bool AlbumIsValid(string aName, string aToken);
        bool ChannelIsValid(VimeoSettingsPartViewModel vm);
        bool ChannelIsValid(string cName, string aToken);

        VimeoUploadQuota CheckQuota();
        int UsedQuota();
        int FreeQuota();
    }

}

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

        bool TokenIsValid(VimeoSettingsPartViewModel vm);
        bool TokenIsValid(string aToken);
    }

}

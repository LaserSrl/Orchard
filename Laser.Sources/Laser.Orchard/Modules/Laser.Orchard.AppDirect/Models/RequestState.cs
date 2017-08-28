using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AppDirect.Models {
    public enum RequestState {
        [Display(Name = "To Create")]
        ToCreate,
        Created
    }
}
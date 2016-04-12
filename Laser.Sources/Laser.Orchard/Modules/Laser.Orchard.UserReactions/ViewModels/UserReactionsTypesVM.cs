using Laser.Orchard.UserReactions.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.ViewModels {

    public class UserReactionsTypes {
        public UserReactionsTypes() {
            UserReactionsType = new List<UserReactionsTypeVM>();
        }

        public List<UserReactionsTypeVM> UserReactionsType { get; set; }
    }

    public class UserReactionsTypeVM {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string TypeCssClass { get; set; }
        public int Priority { get; set; }
        public bool Delete { get; set; }
    }
}
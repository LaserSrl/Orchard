using Laser.Orchard.Questionnaires.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class RankingTemplateVM {

        public Int32 Point { get; set; }
        public string Identifier { get; set; }
        public string UsernameGameCenter { get; set; }
        public TipoDispositivo Device { get; set; }
        public Int32 ContentIdentifier { get; set; }
        public string name { get; set; }
        public bool AccessSecured { get; set; }
        public DateTime RegistrationDate { get; set; }

    }



    public class DisplaRankingTemplateVM {
        public DisplaRankingTemplateVM() {
            ListRank = new List<RankingTemplateVM>();
        }
        public string Title { get; set; }
        public string Device { get; set; }
        public List<RankingTemplateVM> ListRank { get; set; }
    }


}
using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Laser.Orchard.UserReactions.Drivers {
    public class UserReactionsPartDriver : ContentPartDriver<UserReactionsPart> {
        
        //richiamo il service per settaggio dati 
        //Definisci una variabile settata alla interfaccia
        private readonly IUserReactionsService _userReactionService;   

        //Crea nel costruttore il settaggio alla var che esegue una select (quando si istanzia la classe si settano i dati nel costruttore) 
        public UserReactionsPartDriver(IUserReactionsService userReactionService) {
            _userReactionService = userReactionService;
        }


        //Evento display 
        protected override DriverResult Display(UserReactionsPart part, string displayType, dynamic shapeHelper) {
            
            //Gestione visualizzazione amministratore
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_UserReactions_SummaryAdmin", () => shapeHelper
                    .Parts_UserReactions_SummaryAdmin(UserReaction: part));
            }

            var viewmodel = _userReactionService.GetTot(part);
            //Passare la view model da definire 
            if (displayType == "Detail") {
                return ContentShape("Parts_UserReactions_Detail", () => shapeHelper
                   .Parts_UserReactions_Detail(UserReaction: viewmodel));

            }

            //Passare la view model da definire 
            if (displayType == "Summary") {
                return ContentShape("Parts_UserReactions_Summary", () => shapeHelper
                   .Parts_UserReactions_Summary(UserReaction: viewmodel));

            }

            return null;


        }


        ///// <summary>
        ///// GET Editor.
        ///// </summary>   
        //Evento Edit
        protected override DriverResult Editor(UserReactionsPart part, dynamic shapeHelper) {
            
            var viewmodel=_userReactionService.GetTot(part);
           
            //Trasformali in un array
            //var ids = viewmodel.Select(s => s.TypeId).ToArray();

            ////_userReactionService sono i dati 
            //var listType = _userReactionService.GetTypesTable().Where(w => !(ids.Contains(w.Id)))
            //    .Select(x => new UserReactionsVM {
            //        Id = 0,
            //        Quantity = 0,
            //        TypeName = x.TypeName,
            //        TypeId = x.Id
            //    }).ToList();

            //viewmodel = viewmodel.Concat(listType).ToList(); //TODO .OrderBy(o=>o.Priority);

            return ContentShape("Parts_UserReactions_Edit", () => shapeHelper.EditorTemplate(
                                  TemplateName: "Parts/UserReactionsEdit",
                                  Model: viewmodel,
                                  Prefix: Prefix));


        }


    }
}
using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard.Localization;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Collections;


namespace Laser.Orchard.UserReactions.Drivers {
    public class UserReactionsPartDriver : ContentPartDriver<UserReactionsPart> {
        
        //richiamo il service per settaggio dati 
        //Definisci una variabile settata alla interfaccia
        private readonly IOrchardServices _orchardServices;
        private readonly IUserReactionsService _userReactionService;   

        //Crea nel costruttore il settaggio alla var che esegue una select (quando si istanzia la classe si settano i dati nel costruttore) 
        public UserReactionsPartDriver(IUserReactionsService userReactionService, IOrchardServices orchardServices) {
            _userReactionService = userReactionService;
            _orchardServices = orchardServices;
        }

        public Localizer T { get; set; }
        //Evento display 
        protected override DriverResult Display(UserReactionsPart part, string displayType, dynamic shapeHelper) {

            IList<UserReactionsVM> viewmodel = _userReactionService.GetTot(part, false); ;
            
            //Gestione visualizzazione amministratore
            if (displayType == "SummaryAdmin")                 
            {
                viewmodel = _userReactionService.GetTot(part, false);
                return ContentShape("Parts_UserReactions_SummaryAdmin", () => shapeHelper
                    .Parts_UserReactions_SummaryAdmin(UserReaction: viewmodel));
            }

            
            //Passare la view model da definire 
            if (displayType == "Detail") 
            {

                UserReactionsPartSettings settings = part.TypePartDefinition.Settings.GetModel<UserReactionsPartSettings>();
           
                UserReactionsPartSettings newPartsSetting = new UserReactionsPartSettings();

                bool FilterApplied = settings.Filtering;
               
                if (FilterApplied == true) 
                {
                    viewmodel= _userReactionService.GetTot(part, true);
                }
                
                    //List<UserReactionsSettingTypesSel> SettingType = new List<UserReactionsSettingTypesSel>();

                    //if (part.Settings.Count > 0) 
                    //{
                    //    SettingType = new JavaScriptSerializer().Deserialize<List<UserReactionsSettingTypesSel>>(part.Settings.Values.ElementAt(1));
                    //    newPartsSetting.TypeReactionsPartsSelected = SettingType.Where(z=>z.checkReaction==true).ToList();
                    //    newPartsSetting.Filtering = true;
                    //}                   
                   // var viewmodel = _userReactionService.GetTot(newPartsSetting);

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
        protected override DriverResult Editor(UserReactionsPart part, dynamic shapeHelper) 
        {

            IList<UserReactionsVM> viewmodel = _userReactionService.GetTot(part, false);
 
            return ContentShape("Parts_UserReactions_Edit", () => shapeHelper.EditorTemplate(
                                  TemplateName: "Parts/UserReactionsEdit",
                                  Model: viewmodel,
                                  Prefix: Prefix));

        }



    }
}
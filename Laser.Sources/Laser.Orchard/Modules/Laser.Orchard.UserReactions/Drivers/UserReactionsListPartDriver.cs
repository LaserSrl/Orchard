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
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.UserReactions.Drivers {
    public class UserReactionsListPartDriver : ContentPartDriver<UserReactionsListPart> {

        //richiamo il service per settaggio dati 
        //Definisci una variabile settata alla interfaccia
        private readonly IOrchardServices _orchardServices;
        private readonly IUserReactionsService _userReactionService;

        //Crea nel costruttore il settaggio alla var che esegue una select (quando si istanzia la classe si settano i dati nel costruttore) 
        public UserReactionsListPartDriver(IUserReactionsService userReactionService, IOrchardServices orchardServices) {
            _userReactionService = userReactionService;
            _orchardServices = orchardServices;
        }

        public Localizer T { get; set; }
        private const string TemplateName = "Parts/UserReactionsListForm";


        protected override DriverResult Display(UserReactionsListPart part, string displayType, dynamic shapeHelper) {
            List<UserReactionsTypeVM> reactionType = GetTypesTable();
            return ContentShape("Parts_UserReactionsList",
                               () => shapeHelper.DisplayTemplate(TemplateName: TemplateName, 
                                                                 Model: reactionType, Prefix: Prefix));
        }



        protected override DriverResult Editor(UserReactionsListPart part, dynamic shapeHelper) {

            return ContentShape("Parts_UserReactionsList",
                       () => shapeHelper.EditorTemplate(
                           TemplateName: TemplateName,
                           Model: part,
                           Prefix: Prefix));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<UserReactionsTypeVM> GetTypesTable() {
           
            var reactionSettings = _orchardServices.WorkContext.CurrentSite.As<UserReactionsSettingsPart>();
            var userRT = new UserReactionsTypes();

            userRT.UserReactionsType = _userReactionService.GetTypesTable().Select(r => new UserReactionsTypeVM {
                Id = r.Id,
                TypeName = r.TypeName,

            }).ToList();

            return userRT.UserReactionsType;
        }

    }
}
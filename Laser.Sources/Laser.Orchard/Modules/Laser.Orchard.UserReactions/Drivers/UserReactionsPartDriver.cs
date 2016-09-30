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

            IList<UserReactionsVM> viewmodel = null;           

            //Gestione visualizzazione amministratore
            if (displayType == "SummaryAdmin") {
                viewmodel = _userReactionService.GetTot(part);
                return ContentShape("Parts_UserReactions_SummaryAdmin", () => shapeHelper
                    .Parts_UserReactions_SummaryAdmin(UserReaction: viewmodel));
            }


            //Passare la view model da definire 
            if (displayType == "Detail") {
                viewmodel = _userReactionService.GetTot(part);
                bool authorized = _userReactionService.HasPermission(part.ContentItem.ContentType);
                bool requireLogin = false;
                if(authorized == false && _orchardServices.WorkContext.CurrentUser == null) {
                    requireLogin = true;
                }
                return ContentShape("Parts_UserReactions_Detail", () => shapeHelper
                    .Parts_UserReactions_Detail(UserReaction: viewmodel, Authorized: authorized, RequireLogin: requireLogin));
            }

            //Passare la view model da definire 
            if (displayType == "Summary") {
                viewmodel = _userReactionService.GetTot(part);
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

            IList<UserReactionsVM> viewmodel = _userReactionService.GetTot(part);

            return ContentShape("Parts_UserReactions_Edit", () => shapeHelper.EditorTemplate(
                                  TemplateName: "Parts/UserReactionsEdit",
                                  Model: viewmodel,
                                  Prefix: Prefix));

        }



        protected override void Importing(UserReactionsPart part, ImportContentContext context) {

            var root = context.Data.Element(part.PartDefinition.Name);

            foreach (UserReactionsSummaryRecord recReaction in part.Reactions) {
                recReaction.Id = int.Parse(root.Element("Id").Value);
                recReaction.Quantity = int.Parse(root.Element("Quantity").Value);

                recReaction.UserReactionsTypesRecord.Id = int.Parse(root.Attribute("UserReactionsTypesRecord").Parent.Element("Id").Value);
                recReaction.UserReactionsTypesRecord.TypeName = root.Attribute("UserReactionsTypesRecord").Parent.Element("TypeName").Value;
                recReaction.UserReactionsTypesRecord.TypeCssClass = root.Attribute("UserReactionsTypesRecord").Parent.Element("TypeCssClass").Value;
                recReaction.UserReactionsTypesRecord.Priority = int.Parse(root.Attribute("UserReactionsTypesRecord").Parent.Element("Priority").Value);
                recReaction.UserReactionsTypesRecord.CssName =root.Attribute("UserReactionsTypesRecord").Parent.Element("CssName").Value;
                recReaction.UserReactionsTypesRecord.Activating = Convert.ToBoolean(root.Attribute("UserReactionsTypesRecord").Parent.Element("Activating").Value);
            }
        }


        protected override void Exporting(UserReactionsPart part, ExportContentContext context) {

            var root = context.Element(part.PartDefinition.Name);

            if (part.Reactions.Count() > 0) {
                foreach (UserReactionsSummaryRecord receq in part.Reactions) 
                {
                    root.Element("Id").SetAttributeValue("Id", receq.Id);
                    root.Element("Quantity").SetAttributeValue("Quantity", receq.Id);

                    root.Element("UserReactionsTypesRecord").SetAttributeValue("UserReactionsTypesRecord", "UserReactionsTypesRecord");

                    var UserReactionsTypesRec = context.Element(part.PartDefinition.Name).Element("UserReactionsTypesRecord");
                    UserReactionsTypesRec.Element("TypeName").SetAttributeValue("TypeName", receq.UserReactionsTypesRecord.TypeName);
                    UserReactionsTypesRec.Element("TypeCssClass").SetAttributeValue("TypeCssClass", receq.UserReactionsTypesRecord.TypeCssClass);
                    UserReactionsTypesRec.Element("Priority").SetAttributeValue("Priority", receq.UserReactionsTypesRecord.Priority);
                    UserReactionsTypesRec.Element("CssName").SetAttributeValue("CssName", receq.UserReactionsTypesRecord.CssName);
                    UserReactionsTypesRec.Element("Activating").SetAttributeValue("Activating", receq.UserReactionsTypesRecord.Activating);
                }

            }

        }


        
      

    }
}
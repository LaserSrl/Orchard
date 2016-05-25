using Laser.Orchard.UserReactions.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Drivers {
    public class UserReactionsPartDriver : ContentPartDriver<UserReactionsPart>  {


        protected override DriverResult Display(UserReactionsPart part, string displayType, dynamic shapeHelper) {

            if (displayType != "Detail")
                return null;


            if (displayType == "SummaryAdmin")

                return ContentShape("Parts_UserReactions_SummaryAdmin", () => shapeHelper
                    .Parts_UserReactions_SummaryAdmin(UserReactions: part));
           

            return null;
        }


        /// <summary>
        /// GET Editor.
        /// </summary>
        //protected override DriverResult Editor(SeoPart part, dynamic shapeHelper) {

        //    return ContentShape("Parts_SEO_Edit",
        //                        () => shapeHelper.EditorTemplate(
        //                          TemplateName: "Parts/SEO",
        //                          Model: part,
        //                          Prefix: Prefix));
        //}


        ///// <summary>
        ///// POST Editor.
        ///// </summary>
        //protected override DriverResult Editor(SeoPart part, IUpdateModel updater, dynamic shapeHelper) {

        //    updater.TryUpdateModel(part, Prefix, null, null);
        //    return Editor(part, shapeHelper);
        //}
    }
}
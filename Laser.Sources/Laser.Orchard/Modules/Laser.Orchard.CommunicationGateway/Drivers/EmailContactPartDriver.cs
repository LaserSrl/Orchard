using AutoMapper;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class EmailContactPartDriver : ContentPartDriver<EmailContactPart> {
   
        protected override string Prefix {
            get { return "Laser.Mobile.EmailContact"; }
        }

        protected override DriverResult Editor(EmailContactPart part, dynamic shapeHelper) {
            Mapper.CreateMap<CommunicationEmailRecord, View_EmailVM_element>();
            View_EmailVM viewModel = new View_EmailVM();
            View_EmailVM_element vm = new View_EmailVM_element();
            // viewModel.Elenco.Add(vm);
             List<CommunicationEmailRecord> oldviewModel = part.EmailEntries.Value.ToList();
             foreach (CommunicationEmailRecord cm in oldviewModel) {
                 vm = new View_EmailVM_element();
                 Mapper.Map(cm, vm);
                 viewModel.Elenco.Add(vm);
             }
            return ContentShape("Parts_EmailContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/EmailContact_Edit", Model: viewModel, Prefix: Prefix));
        }



        protected override DriverResult Editor(EmailContactPart part, IUpdateModel updater, dynamic shapeHelper) {
         //   View_EmailVM oldviewModel = new View_EmailVM(); 
         //   updater.TryUpdateModel(oldviewModel, Prefix, null,null);
         ////   foreach(

         //   Mapper.CreateMap<View_EmailVM_element, CommunicationEmailRecord>();
           

          //  List<CommunicationEmailRecord> viewModel = part.EmailEntries.Value.ToList();
            // List<CommunicationEmailRecord> viewModel = part.EmailEntries.Value.ToList();
            //return ContentShape("Parts_EmailContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/EmailContact_Edit", Model: viewModel, Prefix: Prefix));
            //QueryFilterVM qfVM = new QueryFilterVM();
            //updater.TryUpdateModel(qfVM, Prefix, null, new string[] { "ElencoQuery" });
            //part.QueryId = Int32.Parse(qfVM.QueryId);
            //    updater.AddModelError("MapPartIsRequired", T("A point on the map is required."));
            return Editor(part, shapeHelper);
        }
    }

}
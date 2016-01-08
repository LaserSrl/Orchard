using AutoMapper;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class EmailContactPartDriver : ContentPartDriver<EmailContactPart> {
        public Localizer T { get; set; }
        protected override string Prefix {
            get { return "Laser.Mobile.EmailContact"; }
        }
        private readonly IRepository<CommunicationEmailRecord> _repoEmail;
        private readonly ITransactionManager _transaction;
        public EmailContactPartDriver(IRepository<CommunicationEmailRecord> repoEmail, ITransactionManager transaction) {
            _repoEmail = repoEmail;
            T = NullLocalizer.Instance;
            _transaction = transaction;
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
            View_EmailVM oldviewModel = new View_EmailVM();

            updater.TryUpdateModel(oldviewModel, Prefix, null, null);
            bool error = false;
            _transaction.Demand();
            foreach (View_EmailVM_element vmel in oldviewModel.Elenco) {
                if ((vmel.Delete || string.IsNullOrEmpty(vmel.Email)) && vmel.Id > 0) {
                    CommunicationEmailRecord cmr = _repoEmail.Fetch(x => x.Id == vmel.Id).FirstOrDefault();
                    _repoEmail.Delete(cmr);
                }
                else
                    if (!vmel.Delete) {
                        if (!string.IsNullOrEmpty(vmel.Email))
                            if (_repoEmail.Fetch(x => x.Email == vmel.Email && x.Id != vmel.Id).Count() > 0) {
                                error = true;
                                updater.AddModelError("Error", T("Email can't be assigned is linked to other contact"));
                            }
                        if (vmel.Id > 0) {
                            CommunicationEmailRecord cmr = _repoEmail.Fetch(x => x.Id == vmel.Id).FirstOrDefault();
                            if (cmr.Email != vmel.Email || cmr.Validated != vmel.Validated) {
                                cmr.Email = vmel.Email;
                                cmr.Validated = vmel.Validated;
                                cmr.DataModifica = DateTime.Now;
                                _repoEmail.Update(cmr);

                            }
                        }
                        else {
                            View_EmailVM_element vm = new View_EmailVM_element();
                            CommunicationEmailRecord cmr = new CommunicationEmailRecord();
                            Mapper.CreateMap<View_EmailVM_element, CommunicationEmailRecord>();
                            Mapper.Map(vm, cmr);
                            cmr.Email = vmel.Email;
                            cmr.Validated = vmel.Validated;
                            cmr.EmailContactPartRecord_Id = part.Id;
                            _repoEmail.Create(cmr);

                        }
                    }
            }
            if (error == true)
                _transaction.Cancel();
            else
                _repoEmail.Flush();
        //    _transaction.RequireNew();
            return Editor(part, shapeHelper);
        }
    }

}
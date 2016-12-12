using AutoMapper;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class EmailContactPartDriver : ContentPartDriver<EmailContactPart> {
        public Localizer T { get; set; }
        protected override string Prefix {
            get { return "Laser.Mobile.EmailContact"; }
        }
        private readonly IRepository<CommunicationEmailRecord> _repoEmail;
        private readonly ITransactionManager _transaction;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;

        public EmailContactPartDriver(IRepository<CommunicationEmailRecord> repoEmail, ITransactionManager transaction, IOrchardServices orchardServices
            , IContentManager contentManager) {
            _repoEmail = repoEmail;
            T = NullLocalizer.Instance;
            _transaction = transaction;
            _orchardServices = orchardServices;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(EmailContactPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Detail") {
                    Mapper.CreateMap<CommunicationEmailRecord, View_EmailVM_element>();
                    View_EmailVM viewModel = new View_EmailVM();
                    View_EmailVM_element vm = new View_EmailVM_element();
                    if (part.EmailEntries.Value != null) {
                        List<CommunicationEmailRecord> oldviewModel = part.EmailEntries.Value.ToList();
                        foreach (CommunicationEmailRecord cm in oldviewModel) {
                            vm = new View_EmailVM_element();
                            Mapper.Map(cm, vm);
                            viewModel.Elenco.Add(vm);
                        }
                    }
                    return ContentShape("Parts_EmailContact",
                        () => shapeHelper.Parts_EmailContact(Elenco: viewModel.Elenco));
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        }

        protected override DriverResult Editor(EmailContactPart part, dynamic shapeHelper) {
            Mapper.CreateMap<CommunicationEmailRecord, View_EmailVM_element>();
            View_EmailVM viewModel = new View_EmailVM();
            View_EmailVM_element vm = new View_EmailVM_element();
            // viewModel.Elenco.Add(vm);
            if (part.EmailEntries.Value != null) {
                List<CommunicationEmailRecord> oldviewModel = part.EmailEntries.Value.ToList();
                foreach (CommunicationEmailRecord cm in oldviewModel) {
                    vm = new View_EmailVM_element();
                    Mapper.Map(cm, vm);
                    viewModel.Elenco.Add(vm);
                }
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
                        if (!string.IsNullOrEmpty(vmel.Email)) {
                            if (_repoEmail.Fetch(x => x.Email == vmel.Email && x.Id != vmel.Id).Count() > 0) {
                                error = true;
                                updater.AddModelError("Error", T("Email can't be assigned is linked to other contact"));
                            }
                        }
                        if (vmel.Id > 0) {
                            CommunicationEmailRecord cmr = _repoEmail.Fetch(x => x.Id == vmel.Id).FirstOrDefault();
                            if (cmr.Email != vmel.Email || cmr.Validated != vmel.Validated ||
                                cmr.AccettatoUsoCommerciale != vmel.AccettatoUsoCommerciale ||
                                cmr.AutorizzatoTerzeParti != vmel.AutorizzatoTerzeParti) {
                                cmr.Email = vmel.Email;
                                cmr.Validated = vmel.Validated;
                                cmr.AccettatoUsoCommerciale = vmel.AccettatoUsoCommerciale;
                                cmr.AutorizzatoTerzeParti = vmel.AutorizzatoTerzeParti;
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
                            cmr.AccettatoUsoCommerciale = vmel.AccettatoUsoCommerciale;
                            cmr.AutorizzatoTerzeParti = vmel.AutorizzatoTerzeParti;
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="context"></param>
        protected override void Importing(EmailContactPart part, ImportContentContext context) {

            var root = context.Data.Element(part.PartDefinition.Name);
            var emailRecord = context.Data.Element(part.PartDefinition.Name).Elements("EmailRecord");

            if (emailRecord != null) {

            try {

                _transaction.Demand();

                 List<CommunicationEmailRecord> listcomemail = new List<CommunicationEmailRecord>();

                 foreach (var rec in emailRecord) {

                        CommunicationEmailRecord recMail = new CommunicationEmailRecord();

                        ////////////////////////////////////////////////////////////////
                        //mod 30-11-2016
                        var tempPartFromid = context.GetItemFromSession(rec.Attribute("EmailContactPartRecord_Id").Value);

                        if (tempPartFromid != null && tempPartFromid.Is<CommunicationContactPart>()) {
                            //associa id contact
                            recMail.EmailContactPartRecord_Id = tempPartFromid.As<CommunicationContactPart>().Id;
                        }
                        //////////////////////

                        var Validated = rec.Attribute("Validated").Value;
                        if (Validated != null)
                            recMail.Validated = Convert.ToBoolean(Validated);

                        var DataInserimento = rec.Attribute("DataInserimento").Value;
                        if (DataInserimento != null)
                            recMail.DataInserimento = Convert.ToDateTime(DataInserimento);

                        var DataModifica = rec.Attribute("DataModifica").Value;
                        if (DataModifica != null)
                            recMail.DataModifica = Convert.ToDateTime(DataModifica);

                        var Email = rec.Attribute("Email").Value;
                        if (Email != null)
                            recMail.Email = Email;

                        var Produzione = rec.Attribute("Produzione").Value;
                        if (Produzione != null)
                            recMail.Produzione = Convert.ToBoolean(Produzione);

                        var AccettatoUsoCommerciale = rec.Attribute("AccettatoUsoCommerciale").Value;
                        if (AccettatoUsoCommerciale != null)
                            recMail.AccettatoUsoCommerciale = Convert.ToBoolean(AccettatoUsoCommerciale);

                        var AutorizzatoTerzeParti = rec.Attribute("AutorizzatoTerzeParti").Value;
                        if (AutorizzatoTerzeParti != null)
                            recMail.AutorizzatoTerzeParti = Convert.ToBoolean(AutorizzatoTerzeParti);

                        _repoEmail.Create(recMail);

                        listcomemail.Add(recMail);
                        
                    }

                    part.EmailRecord = listcomemail;
                    _repoEmail.Flush();

                }
                catch (Exception ex) {
                    _transaction.Cancel();

                }

            }
        }




        protected override void Exporting(EmailContactPart part, ExportContentContext context) {

            // var root = context.Element(part.PartDefinition.Name);

            if (part.EmailRecord != null) {
                var root = context.Element(part.PartDefinition.Name);
                foreach (CommunicationEmailRecord rec in part.EmailRecord) {
                    XElement emailText = new XElement("EmailRecord");


                    emailText.SetAttributeValue("Language", rec.Language);

                    //mod 31-11-2016      
                    // da settare con identity
                    if (rec.EmailContactPartRecord_Id > 0) {
                        //cerco il corrispondente valore dell' identity dalla parts del contact e lo associo al campo EmailContactPartRecord_Id 
                        var contItemContact = _contentManager.Get(rec.EmailContactPartRecord_Id);
                        if (contItemContact != null) {
                            emailText.SetAttributeValue("EmailContactPartRecord_Id", _contentManager.GetItemMetadata(contItemContact).Identity.ToString());
                        }
                    }
                    //////

                    emailText.SetAttributeValue("Validated", rec.Validated);
                    emailText.SetAttributeValue("DataInserimento", rec.DataInserimento);
                    emailText.SetAttributeValue("DataModifica", rec.DataModifica);
                    emailText.SetAttributeValue("Email", rec.Email);
                    emailText.SetAttributeValue("Produzione", rec.Produzione);
                    emailText.SetAttributeValue("AccettatoUsoCommerciale", rec.AccettatoUsoCommerciale);
                    emailText.SetAttributeValue("AutorizzatoTerzeParti", rec.AutorizzatoTerzeParti);
                    emailText.SetAttributeValue("KeyUnsubscribe", rec.KeyUnsubscribe);
                    emailText.SetAttributeValue("DataUnsubscribe", rec.DataUnsubscribe);
                    root.Add(emailText);
                }
            }


        }






    }

}
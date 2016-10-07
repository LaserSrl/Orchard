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

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class EmailContactPartDriver : ContentPartDriver<EmailContactPart> {
        public Localizer T { get; set; }
        protected override string Prefix {
            get { return "Laser.Mobile.EmailContact"; }
        }
        private readonly IRepository<CommunicationEmailRecord> _repoEmail;
        private readonly ITransactionManager _transaction;
        private readonly IOrchardServices _orchardServices;

        public EmailContactPartDriver(IRepository<CommunicationEmailRecord> repoEmail, ITransactionManager transaction, IOrchardServices orchardServices) {
            _repoEmail = repoEmail;
            T = NullLocalizer.Instance;
            _transaction = transaction;
            _orchardServices = orchardServices;
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
                } else {
                    return null;
                }
            } else {
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



        protected override void Importing(EmailContactPart part, ImportContentContext context) {
            //throw new NotImplementedException();

            var root = context.Data.Element(part.PartDefinition.Name);
            var emailRecord = context.Attribute("EmailRecord", "EmailRecord");

            if (emailRecord != null) {
                foreach (CommunicationEmailRecord rec in part.EmailRecord) {
                    
                    rec.Id = int.Parse(root.Attribute("EmailRecord").Parent.Element("Id").Value);

                    //rec.Language = root.Attribute("EmailRecord").Parent.Element("Language").Value;
                    var importedLanguage = root.Attribute("EmailRecord").Parent.Element("Language").Value;
                    if (importedLanguage != null) {
                        rec.Language = importedLanguage;
                    }

                    //rec.EmailContactPartRecord_Id = int.Parse(root.Attribute("EmailRecord").Parent.Element("EmailContactPartRecord_Id").Value);
                    var importedEmailContactPartRecord_Id = int.Parse(root.Attribute("EmailRecord").Parent.Element("EmailContactPartRecord_Id").Value);
                    if (importedEmailContactPartRecord_Id != null) {
                        rec.EmailContactPartRecord_Id = importedEmailContactPartRecord_Id;
                    }

                    //rec.Validated = bool.Parse(root.Attribute("EmailRecord").Parent.Element("Validated").Value);
                    var importedValidated = bool.Parse(root.Attribute("EmailRecord").Parent.Element("Validated").Value);
                    if (importedValidated != null) {
                        rec.Validated = importedValidated;
                    }

                    //rec.DataInserimento = DateTime.Parse(root.Attribute("EmailRecord").Parent.Element("DataInserimento").Value);
                    var importedDataInserimento = DateTime.Parse(root.Attribute("EmailRecord").Parent.Element("DataInserimento").Value);
                    if (importedDataInserimento != null) {
                        rec.DataInserimento = importedDataInserimento;
                    }

                    //rec.DataModifica = DateTime.Parse(root.Attribute("EmailRecord").Parent.Element("DataModifica").Value);
                    var importedDataModifica = DateTime.Parse(root.Attribute("EmailRecord").Parent.Element("DataModifica").Value);
                    if (importedDataModifica != null) {
                        rec.DataModifica = importedDataModifica;
                    }
                    
                    //rec.Email = root.Attribute("EmailRecord").Parent.Element("Email").Value;
                    var importedEmail = root.Attribute("EmailRecord").Parent.Element("Email").Value;
                    if (importedEmail != null) {
                        rec.Email = importedEmail;
                    }

                    //rec.Produzione = bool.Parse(root.Attribute("EmailRecord").Parent.Element("Produzione").Value);
                    var importedProduzione = root.Attribute("EmailRecord").Parent.Element("Produzione").Value;
                    if (importedProduzione != null) {
                        rec.Produzione = bool.Parse(importedProduzione);
                    }

                    //rec.AccettatoUsoCommerciale = bool.Parse(root.Attribute("EmailRecord").Parent.Element("AccettatoUsoCommerciale").Value);
                    var importedAccettatoUsoCommerciale = root.Attribute("EmailRecord").Parent.Element("AccettatoUsoCommerciale").Value;
                    if (importedAccettatoUsoCommerciale != null) {
                        rec.AccettatoUsoCommerciale = bool.Parse(importedAccettatoUsoCommerciale);
                    }                    
                    
                   // rec.AutorizzatoTerzeParti = bool.Parse(root.Attribute("EmailRecord").Parent.Element("AutorizzatoTerzeParti").Value);
                    var importedAutorizzatoTerzeParti = root.Attribute("EmailRecord").Parent.Element("AutorizzatoTerzeParti").Value;
                    if (importedAutorizzatoTerzeParti != null) {
                        rec.AutorizzatoTerzeParti = bool.Parse(importedAutorizzatoTerzeParti);
                    }

                    //rec.KeyUnsubscribe = root.Attribute("EmailRecord").Parent.Element("KeyUnsubscribe").Value;
                    var importedKeyUnsubscribe = root.Attribute("EmailRecord").Parent.Element("KeyUnsubscribe").Value;
                    if (importedKeyUnsubscribe != null) {
                        rec.KeyUnsubscribe = importedKeyUnsubscribe;
                    }

                    //rec.DataUnsubscribe = DateTime.Parse(root.Attribute("EmailRecord").Parent.Element("DataUnsubscribe").Value);
                    var importedDataUnsubscribe = root.Attribute("EmailRecord").Parent.Element("DataUnsubscribe").Value;
                    if (importedDataUnsubscribe != null) {
                        rec.DataUnsubscribe =  DateTime.Parse(importedDataUnsubscribe);
                    }
                }

            }

        }

        protected override void Exporting(EmailContactPart part, ExportContentContext context) {
            //throw new NotImplementedException();
            var root = context.Element(part.PartDefinition.Name);

            if (part.EmailRecord != null) {

                context.Element(part.PartDefinition.Name).SetAttributeValue("EmailRecord", part.EmailRecord);
                var email = context.Element(part.PartDefinition.Name).Element("EmailRecord");

                foreach (CommunicationEmailRecord rec in part.EmailRecord) {

                    email.Element("Id").SetAttributeValue("Id", rec.Id);
                    email.Element("Language").SetAttributeValue("Language", rec.Language);
                    email.Element("EmailContactPartRecord_Id").SetAttributeValue("EmailContactPartRecord_Id", rec.EmailContactPartRecord_Id);
                    email.Element("Validated").SetAttributeValue("Validated", rec.Validated);
                    email.Element("DataInserimento").SetAttributeValue("DataInserimento", rec.DataInserimento);
                    email.Element("DataModifica").SetAttributeValue("DataModifica", rec.DataModifica);
                    email.Element("Email").SetAttributeValue("Email", rec.Email);
                    email.Element("Produzione").SetAttributeValue("Produzione", rec.Produzione);
                    email.Element("AccettatoUsoCommerciale").SetAttributeValue("AccettatoUsoCommerciale", rec.AccettatoUsoCommerciale);
                    email.Element("AutorizzatoTerzeParti").SetAttributeValue("AutorizzatoTerzeParti", rec.AutorizzatoTerzeParti);
                    email.Element("KeyUnsubscribe").SetAttributeValue("KeyUnsubscribe", rec.KeyUnsubscribe);
                    email.Element("DataUnsubscribe").SetAttributeValue("DataUnsubscribe", rec.DataUnsubscribe);
                }
            }
           

        }






    }

}
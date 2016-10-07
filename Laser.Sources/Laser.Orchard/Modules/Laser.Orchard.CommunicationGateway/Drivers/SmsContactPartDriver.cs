//using Laser.Orchard.CommunicationGateway.Models;
//using Orchard.ContentManagement.Drivers;
//using Orchard.Data;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace Laser.Orchard.CommunicationGateway.Drivers {
//    public class SmsContactPartDriver : ContentPartDriver<SmsContactPart> {

//        protected override string Prefix {
//            get { return "Laser.Mobile.SmsContact"; }
//        }

//        protected override DriverResult Editor(SmsContactPart part, dynamic shapeHelper) {
//            List<CommunicationSmsRecord> viewModel = part.SmsEntries.Value.ToList();
//            return ContentShape("Parts_SmsContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/SmsContact_Edit", Model: viewModel, Prefix: Prefix));
//        }
//    }

//}


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
    public class SmsContactPartDriver : ContentPartDriver<SmsContactPart> {
        public Localizer T { get; set; }
        protected override string Prefix {
            get { return "Laser.Mobile.SmsContact"; }
        }
        private readonly IRepository<CommunicationSmsRecord> _repoSms;
        private readonly ITransactionManager _transaction;
        private readonly IOrchardServices _orchardServices;

        public SmsContactPartDriver(IRepository<CommunicationSmsRecord> repoSms, ITransactionManager transaction, IOrchardServices orchardServices) {
            _repoSms = repoSms;
            T = NullLocalizer.Instance;
            _transaction = transaction;
            _orchardServices = orchardServices;
        }

        protected override DriverResult Display(SmsContactPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Detail") {
                    Mapper.CreateMap<CommunicationSmsRecord, View_SmsVM_element>();
                    View_SmsVM viewModel = new View_SmsVM();
                    View_SmsVM_element vm = new View_SmsVM_element();
                    if (part.SmsEntries.Value != null) {
                        List<CommunicationSmsRecord> oldviewModel = part.SmsEntries.Value.ToList();
                        foreach (CommunicationSmsRecord cm in oldviewModel) {
                            vm = new View_SmsVM_element();
                            Mapper.Map(cm, vm);
                            viewModel.Elenco.Add(vm);
                        }
                    }
                    return ContentShape("Parts_SmsContact",
                        () => shapeHelper.Parts_SmsContact(Elenco: viewModel.Elenco));
                } else {
                    return null;
                }
            } else {
                return null;
            }
        }

        protected override DriverResult Editor(SmsContactPart part, dynamic shapeHelper) {
            Mapper.CreateMap<CommunicationSmsRecord, View_SmsVM_element>();
            View_SmsVM viewModel = new View_SmsVM();
            View_SmsVM_element vm = new View_SmsVM_element();
            // viewModel.Elenco.Add(vm);
            if (part.SmsEntries.Value != null) {
                List<CommunicationSmsRecord> oldviewModel = part.SmsEntries.Value.ToList();
                foreach (CommunicationSmsRecord cm in oldviewModel) {
                    vm = new View_SmsVM_element();
                    Mapper.Map(cm, vm);
                    viewModel.Elenco.Add(vm);
                }
            }
            return ContentShape("Parts_SmsContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/SmsContact_Edit", Model: viewModel, Prefix: Prefix));
        }



        protected override DriverResult Editor(SmsContactPart part, IUpdateModel updater, dynamic shapeHelper) {
            View_SmsVM oldviewModel = new View_SmsVM();

            updater.TryUpdateModel(oldviewModel, Prefix, null, null);
            bool error = false;
            _transaction.Demand();
            foreach (View_SmsVM_element vmel in oldviewModel.Elenco) {
                if ((vmel.Delete || string.IsNullOrEmpty(vmel.Sms)) && vmel.Id > 0) {
                    CommunicationSmsRecord cmr = _repoSms.Fetch(x => x.Id == vmel.Id).FirstOrDefault();
                    _repoSms.Delete(cmr);
                }
                else
                    if (!vmel.Delete) {
                        if (!string.IsNullOrEmpty(vmel.Sms))
                            if (_repoSms.Fetch(x => x.Sms == vmel.Sms && x.Prefix == vmel.Prefix && x.Id != vmel.Id).Count() > 0) {
                                error = true;
                                updater.AddModelError("Error", T("Sms can't be assigned is linked to other contact"));
                            }
                        if (vmel.Id > 0) {
                            CommunicationSmsRecord cmr = _repoSms.Fetch(x => x.Id == vmel.Id).FirstOrDefault();
                            if (cmr.Sms != vmel.Sms || cmr.Prefix != vmel.Prefix || cmr.Validated != vmel.Validated|| 
                                cmr.AccettatoUsoCommerciale != vmel.AccettatoUsoCommerciale ||
                                cmr.AutorizzatoTerzeParti != vmel.AutorizzatoTerzeParti) {
                                cmr.Sms = vmel.Sms;
                                cmr.Prefix = vmel.Prefix;
                                cmr.Validated = vmel.Validated;
                                cmr.AccettatoUsoCommerciale = vmel.AccettatoUsoCommerciale;
                                cmr.AutorizzatoTerzeParti = vmel.AutorizzatoTerzeParti;
                                cmr.DataModifica = DateTime.Now;
                                _repoSms.Update(cmr);

                            }
                        }
                        else {
                            View_SmsVM_element vm = new View_SmsVM_element();
                            CommunicationSmsRecord cmr = new CommunicationSmsRecord();
                            Mapper.CreateMap<View_SmsVM_element, CommunicationSmsRecord>();
                            Mapper.Map(vm, cmr);
                            cmr.Sms = vmel.Sms;
                            cmr.Validated = vmel.Validated;
                            cmr.AccettatoUsoCommerciale = vmel.AccettatoUsoCommerciale;
                            cmr.AutorizzatoTerzeParti = vmel.AutorizzatoTerzeParti;
                            cmr.Prefix = vmel.Prefix;
                            cmr.SmsContactPartRecord_Id = part.Id;
                            _repoSms.Create(cmr);

                        }
                    }
            }
            if (error == true)
                _transaction.Cancel();
            else
                _repoSms.Flush();
            return Editor(part, shapeHelper);
        }


        protected override void Importing(SmsContactPart part, ImportContentContext context) {
            //throw new NotImplementedException();

            var root = context.Data.Element(part.PartDefinition.Name);

            var importedSmsRecord = context.Attribute("SmsRecord", "SmsRecord");

            if (importedSmsRecord != null) {

                foreach (CommunicationSmsRecord rec in part.SmsRecord) {
                    rec.Id = int.Parse(root.Attribute("SmsRecord").Parent.Element("Id").Value);

                    //rec.Language = root.Attribute("SmsRecord").Parent.Element("Language").Value;
                    //rec.Validated = bool.Parse(root.Attribute("SmsRecord").Parent.Element("Validated").Value);
                    //rec.DataInserimento = DateTime.Parse(root.Attribute("SmsRecord").Parent.Element("DataInserimento").Value);
                    //rec.DataModifica = DateTime.Parse(root.Attribute("SmsRecord").Parent.Element("DataModifica").Value);
                    //rec.Sms = root.Attribute("SmsRecord").Parent.Element("Sms").Value;
                    //rec.Prefix = root.Attribute("SmsRecord").Parent.Element("Prefix").Value;
                    //rec.Produzione = bool.Parse(root.Attribute("SmsRecord").Parent.Element("Produzione").Value);
                    //rec.AccettatoUsoCommerciale = bool.Parse(root.Attribute("SmsRecord").Parent.Element("AccettatoUsoCommerciale").Value);
                    //rec.AutorizzatoTerzeParti = bool.Parse(root.Attribute("SmsRecord").Parent.Element("AutorizzatoTerzeParti").Value);


                    var importedLanguage = root.Attribute("EmailRecord").Parent.Element("Language").Value;
                    if (importedLanguage != null) {
                        rec.Language = importedLanguage;
                    }

                   
                    var importedValidated = bool.Parse(root.Attribute("EmailRecord").Parent.Element("Validated").Value);
                    if (importedValidated != null) {
                        rec.Validated = importedValidated;
                    }

                    var importedDataInserimento = DateTime.Parse(root.Attribute("EmailRecord").Parent.Element("DataInserimento").Value);
                    if (importedDataInserimento != null) {
                        rec.DataInserimento = importedDataInserimento;
                    }

                    var importedDataModifica = DateTime.Parse(root.Attribute("EmailRecord").Parent.Element("DataModifica").Value);
                    if (importedDataModifica != null) {
                        rec.DataModifica = importedDataModifica;
                    }

                    var importedSms = root.Attribute("EmailRecord").Parent.Element("Sms").Value;
                    if (importedSms != null) {
                        rec.Sms = importedSms;
                    }

                    var importedPrefix = root.Attribute("EmailRecord").Parent.Element("Prefix").Value;
                    if (importedPrefix != null) {
                        rec.Prefix = importedPrefix;
                    }

                    var importedProduzione = root.Attribute("EmailRecord").Parent.Element("Produzione").Value;
                    if (importedProduzione != null) {
                        rec.Produzione = bool.Parse(importedProduzione);
                    }

                    var importedAccettatoUsoCommerciale = root.Attribute("EmailRecord").Parent.Element("AccettatoUsoCommerciale").Value;
                    if (importedAccettatoUsoCommerciale != null) {
                        rec.AccettatoUsoCommerciale = bool.Parse(importedAccettatoUsoCommerciale);
                    }
                    
                    var importedAutorizzatoTerzeParti = root.Attribute("EmailRecord").Parent.Element("AutorizzatoTerzeParti").Value;
                    if (importedAutorizzatoTerzeParti != null) {
                        rec.AutorizzatoTerzeParti = bool.Parse(importedAutorizzatoTerzeParti);
                    }


                }

            }

        }

        protected override void Exporting(SmsContactPart part, ExportContentContext context) {
            //throw new NotImplementedException();
            
            if (part.SmsRecord != null) {

                context.Element(part.PartDefinition.Name).SetAttributeValue("SmsRecord", part.SmsRecord);
                var smsRecord = context.Element(part.PartDefinition.Name).Element("SmsRecord");

                foreach (CommunicationSmsRecord rec in part.SmsRecord) {

                    smsRecord.Element("Id").SetAttributeValue("Id", rec.Id);
                    smsRecord.Element("Language").SetAttributeValue("Language", rec.Language);
                    smsRecord.Element("Validated").SetAttributeValue("Validated", rec.Validated);
                    smsRecord.Element("DataInserimento").SetAttributeValue("DataInserimento", rec.DataInserimento);
                    smsRecord.Element("DataModifica").SetAttributeValue("DataModifica", rec.DataModifica);
                    smsRecord.Element("Sms").SetAttributeValue("Sms", rec.Sms);
                    smsRecord.Element("Prefix").SetAttributeValue("Sms", rec.Prefix);
                    smsRecord.Element("Produzione").SetAttributeValue("Produzione", rec.Produzione);
                    smsRecord.Element("AccettatoUsoCommerciale").SetAttributeValue("AccettatoUsoCommerciale", rec.AccettatoUsoCommerciale);
                    smsRecord.Element("AutorizzatoTerzeParti").SetAttributeValue("AutorizzatoTerzeParti", rec.AutorizzatoTerzeParti);

                }
            }


        }





    }

}
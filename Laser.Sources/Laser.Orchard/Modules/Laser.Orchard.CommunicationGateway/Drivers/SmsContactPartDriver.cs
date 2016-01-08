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
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
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
        public SmsContactPartDriver(IRepository<CommunicationSmsRecord> repoSms, ITransactionManager transaction) {
            _repoSms = repoSms;
            T = NullLocalizer.Instance;
            _transaction = transaction;
        }

        protected override DriverResult Editor(SmsContactPart part, dynamic shapeHelper) {
            Mapper.CreateMap<CommunicationSmsRecord, View_SmsVM_element>();
            View_SmsVM viewModel = new View_SmsVM();
            View_SmsVM_element vm = new View_SmsVM_element();
            // viewModel.Elenco.Add(vm);
            List<CommunicationSmsRecord> oldviewModel = part.SmsEntries.Value.ToList();
            foreach (CommunicationSmsRecord cm in oldviewModel) {
                vm = new View_SmsVM_element();
                Mapper.Map(cm, vm);
                viewModel.Elenco.Add(vm);
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
                            if (cmr.Sms != vmel.Sms || cmr.Prefix != vmel.Prefix || cmr.Validated != vmel.Validated) {
                                cmr.Sms = vmel.Sms;
                                cmr.Prefix = vmel.Prefix;
                                cmr.Validated = vmel.Validated;
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
    }

}
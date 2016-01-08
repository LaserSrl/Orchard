using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Laser.Orchard.Mobile.SmsServiceReference;
using System.ServiceModel;
using Orchard.Logging;
using Orchard.Users.Models;
using Laser.Orchard.CommunicationGateway.Models;
using Orchard.Data;
namespace Laser.Orchard.Mobile.Services {
    public interface ISmsServices : IDependency {
        string SendSms(long[] TelDestArr, string TestoSMS);
        void Synchronize();
    }

    [OrchardFeature("Laser.Orchard.Sms")]
    public class SmsServices : ISmsServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<CommunicationSmsRecord> _repositoryCommunicationSmsRecord;
        public SmsServices(IOrchardServices orchardServices, IRepository<CommunicationSmsRecord> repositoryCommunicationSmsRecord) {
            _repositoryCommunicationSmsRecord = repositoryCommunicationSmsRecord;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        public void Synchronize() {
            #region lego tutti gli sms
            var alluser = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(x => x.RegistrationStatus == UserStatus.Approved);
            if (alluser.List().FirstOrDefault().As<UserPwdRecoveryPart>() != null) {
                var allusercol = alluser.List().Where(x => !string.IsNullOrEmpty(x.ContentItem.As<UserPwdRecoveryPart>().PhoneNumber)).ToList();
                foreach (IContent user in allusercol) {
                    string pref = user.ContentItem.As<UserPwdRecoveryPart>().InternationalPrefix;
                    string num = user.ContentItem.As<UserPwdRecoveryPart>().PhoneNumber;
                    CommunicationSmsRecord csr = _repositoryCommunicationSmsRecord.Fetch(x => x.Sms == num && x.Prefix == pref).FirstOrDefault();
                    CommunicationContactPart ciCommunication = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == user.Id).List().FirstOrDefault();
                    if (ciCommunication == null) {
                        // Una contact part dovrebbe esserci in quanto questo codice viene eseguito dopo la sincronizzazione utenti
                        // Se non vi è una contartpart deduco che il dato sia sporco (es: UUid di un utente che è stato cancellato quindi non sincronizzo il dato con contactpart, verrà legato come se fosse scollegato al contentitem che raggruppa tutti i scollegati)
                        //throw new Exception("Utente senza associazione alla profilazione");
                    }
                    else {
                        if (csr == null) {
                            CommunicationSmsRecord newsms = new CommunicationSmsRecord();
                            newsms.Prefix = pref;
                            newsms.Sms = num;
                            newsms.SmsContactPartRecord_Id = ciCommunication.ContentItem.Id;
                            newsms.Id = 0;
                            newsms.Validated = true;
                            newsms.DataInserimento = DateTime.Now;
                            newsms.DataModifica = DateTime.Now;
                            newsms.Produzione = true;                     
                            _repositoryCommunicationSmsRecord.Create(newsms);
                            _repositoryCommunicationSmsRecord.Flush();
                        }
                        else {
                            if (csr.SmsContactPartRecord_Id != ciCommunication.ContentItem.Id) {
                                csr.SmsContactPartRecord_Id = ciCommunication.ContentItem.Id;
                                csr.DataModifica = DateTime.Now;
                                _repositoryCommunicationSmsRecord.Update(csr);
                                _repositoryCommunicationSmsRecord.Flush();
                            }
                        }
                    }
                }
            }
            #endregion
        }



        public string SendSms(long[] telDestArr, string testoSMS) {
            var bRet = "FALSE";
            ArrayOfLong numbers = new ArrayOfLong();
            numbers.AddRange(telDestArr);
            try {
                var smsSettings = _orchardServices.WorkContext.CurrentSite.As<SmsSettingsPart>();
                Sms sms = new Sms {
                    DriverId = smsSettings.MamDriverIdentifier,
                    SmsFrom = smsSettings.SmsFrom,
                    MamHaveAlias = smsSettings.MamHaveAlias,
                    SmsPrority = smsSettings.SmsPrority ?? 0,
                    SmsValidityPeriod = smsSettings.SmsValidityPeriod ?? 3600,
                    ExternalId = new Guid().ToString(),
                    SmsBody = testoSMS,
                    SmsTipoCodifica = 0,
                    SmsNumber = numbers,
                };
                //Specify the binding to be used for the client.
                EndpointAddress address = new EndpointAddress(smsSettings.SmsServiceEndPoint);
                SmsServiceReference.SmsServiceSoapClient _service;
                if (smsSettings.SmsServiceEndPoint.ToLower().StartsWith("https://")) {
                    WSHttpBinding binding = new WSHttpBinding();
                    binding.Security.Mode = SecurityMode.Transport;
                    _service = new SmsServiceSoapClient(binding, address);
                } else {
                    BasicHttpBinding binding = new BasicHttpBinding();
                    _service = new SmsServiceSoapClient(binding, address);
                }
                
                var result = _service.SendSMS(sms);

                //Log.Info(Metodo + " Inviato SMS ID: " + idSmsComponent);
                bRet = result;

            } catch (Exception ex) {
                Logger.Error(ex, ex.Message + " :: " + ex.StackTrace);
            }
            return bRet;
        }

    }
}
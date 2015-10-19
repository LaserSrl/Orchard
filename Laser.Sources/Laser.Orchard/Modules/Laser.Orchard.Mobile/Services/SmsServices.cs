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
namespace Laser.Orchard.Mobile.Services {
    public interface ISmsServices : IDependency {
        string SendSms(long[] TelDestArr, string TestoSMS);
    }

    [OrchardFeature("Laser.Orchard.Sms")]
    public class SmsServices : ISmsServices {
        private readonly IOrchardServices _orchardServices;
        public SmsServices(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

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
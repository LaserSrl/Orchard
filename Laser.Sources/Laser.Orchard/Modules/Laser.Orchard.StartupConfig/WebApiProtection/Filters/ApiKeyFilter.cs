using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.OutputCache.Filters;
using Orchard.Security;
using Orchard.Utility.Extensions;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Filters {

    /// <summary>
    /// A fini di test è possibile passare la ApiKey in QueryString nel seguente formato: OZVV5TpP4U6wJthaCORZEQ,10/03/2016T10.00.00+2
    /// Se ApiKey viene passato in QueryString non viene applicata la logica di cifratura.
    /// Se ApiKey viene passato in QueryString insieme al parametro clear=false invece, viene applicata la logica di cifratura.
    /// </summary>
    [OrchardFeature("Laser.Orchard.StartupConfig.WebApiProtection")]
    public class ApiKeyFilter : ICachingEventHandler {
        private readonly ShellSettings _shellSettings;


        public ApiKeyFilter(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        public System.Text.StringBuilder InflatingCacheKey(System.Text.StringBuilder key) {
            var request = HttpContext.Current.Request;
            var area = request.RequestContext.RouteData.Values["area"];
            var controller = request.RequestContext.RouteData.Values["controller"];
            var action = request.RequestContext.RouteData.Values["action"];
            //TODO: Sostituire con stringhe prelevate dai settings; in fase di abilitazione moduli, queste stringhe devono essere precaricate nei settings.
            var protectedControllers = new string[] {
                "Laser.Orchard.WebServices.Json.GetByAlias",
                "Laser.Orchard.Mobile.Signal.Trigger",
                "Laser.Orchard.Braintree.Paypal.GetClientToken",
                "Laser.Orchard.Braintree.Paypal.Pay"
            };
            if (protectedControllers.Contains(String.Format("{0}.{1}.{2}", area, controller, action), StringComparer.InvariantCultureIgnoreCase)) {
                if (!TryValidateKey(request.QueryString["ApiKey"] ?? request.Headers["ApiKey"], (request.QueryString["ApiKey"] != null && request.QueryString["clear"] != "false"))) {
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.StatusCode = 401;
                    HttpContext.Current.Response.Write("Error");
                    HttpContext.Current.Response.End();
                    throw new UnauthorizedAccessException();
                    //key.Append("UnAuthorizedApi");
                    //_cacheManager.Get(key.ToString(), ctx => {
                    //    ctx.Monitor(_signals.When(key.ToString()));
                    //    //var _originalWriter = HttpContext.Current.Response.Output;
                    //    //HttpContext.Current.Response.Clear();
                    //    //_originalWriter.Write("Error");
                    //    return ("Error");
                    //});
                } else {
                    key.Append("AuthorizedApi");
                }
            }
            return key;
        }


        private bool TryValidateKey(string token, bool clearText) {
            try {
                string key = token;
                if (!clearText) {
                    key = DecryptStringFromBytes_Aes(Convert.FromBase64String(token));
                } else {
                    var encrypted = Convert.ToBase64String(EncryptStringToBytes_Aes(token));
                }

                //TODO: Sostituire con key prelelvate dai settings
                var allowedKeys = new string[] {
                    "OZVV5TpP4U6wJthaCORZEQ",
                    "DJSH348579DEJfjdklsFHK",
                    "dhj940FHJSfljs905890fk"
                };


                if (String.IsNullOrWhiteSpace(token) || !allowedKeys.Contains(key))
                    return false;

                DateTime.Today.ToUniversalTime();
                return true;
            } catch {
                return false;
            }
        }
        byte[] EncryptStringToBytes_Aes(string plainText) {

            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");

            byte[] data;
            byte[] encryptedData;

            using (var ms = new MemoryStream()) {
                using (var symmetricAlgorithm = CreateSymmetricAlgorithm()) {
                    data = Encoding.Unicode.GetBytes(plainText);
                    using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write)) {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    encryptedData = ms.ToArray();
                    return encryptedData;
                }
            }
        }

        string DecryptStringFromBytes_Aes(byte[] cipherText) {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");

            using (var symmetricAlgorithm = CreateSymmetricAlgorithm()) {
                using (var ms = new MemoryStream()) {
                    using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Write)) {
                        cs.Write(cipherText, 0, cipherText.Length);
                        cs.FlushFinalBlock();
                    }
                    return System.Text.Encoding.Unicode.GetString(ms.ToArray());
                }
            }

        }
        private SymmetricAlgorithm CreateSymmetricAlgorithm() {
            var algorithm = SymmetricAlgorithm.Create(_shellSettings.EncryptionAlgorithm);
            algorithm.Key = _shellSettings.EncryptionKey.ToByteArray();
            algorithm.IV = Encoding.Unicode.GetBytes(DateTime.UtcNow.ToString("ddMMyyyy").Substring(0, 8));
            return algorithm;
        }
    }
}



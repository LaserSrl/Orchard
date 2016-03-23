using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.OutputCache.Filters;
using Orchard.Security;
using Orchard.Utility.Extensions;
using Orchard.ContentManagement;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Laser.Orchard.StartupConfig.Services;
using Newtonsoft.Json;
using Laser.Orchard.StartupConfig.ViewModels;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Filters {

    /// <summary>
    /// A fini di test è possibile passare la ApiKey in QueryString nel seguente formato: OZVV5TpP4U6wJthaCORZEQ,10/03/2016T10.00.00+2
    /// Se ApiKey viene passato in QueryString non viene applicata la logica di cifratura.
    /// Se ApiKey viene passato in QueryString insieme al parametro clear=false invece, viene applicata la logica di cifratura.
    /// </summary>
    [OrchardFeature("Laser.Orchard.StartupConfig.WebApiProtection")]
    public class ApiKeyFilter : FilterProvider, IActionFilter, IResultFilter, ICachingEventHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly HttpRequest _request;
        private readonly IUtilsServices _utilsServices;
        private string _additionalCacheKey;

        public ApiKeyFilter(ShellSettings shellSettings, IOrchardServices orchardServices, IUtilsServices utilsServices) {
            _shellSettings = shellSettings;
            _request = HttpContext.Current.Request;
            Logger = NullLogger.Instance;
            _orchardServices = orchardServices;
            _utilsServices = utilsServices;
        }

        public ILogger Logger;

        private void ErrorResult(ActionExecutingContext filterContext, string errorData) {
            if (filterContext == null) return;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
            var response = _utilsServices.GetResponse(ViewModels.ResponseType.UnAuthorized);
            response.Data = errorData;
            filterContext.Result = new JsonResult {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            return;

        }

        private void ValidateRequestByApiKey(ActionExecutingContext filterContext) {
            if (_additionalCacheKey != null) {
                if (_additionalCacheKey == "UnauthorizedApi;") {
                    if (filterContext != null) {
                        ErrorResult(filterContext, String.Format("UnauthorizedApi: {0}", _request.QueryString["ApiKey"] ?? _request.Headers["ApiKey"]));
                        return;
                    }
                } else { return; }
            }



            var settings = _orchardServices.WorkContext.CurrentSite.As<ProtectionSettingsPart>();

            if (String.IsNullOrWhiteSpace(settings.ProtectedEntries))
                return; // non ci sono entries da proteggere
            var protectedControllers = settings.ProtectedEntries.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var area = _request.RequestContext.RouteData.Values["area"];
            var controller = _request.RequestContext.RouteData.Values["controller"];
            var action = _request.RequestContext.RouteData.Values["action"];

            if (protectedControllers.Contains(String.Format("{0}.{1}.{2}", area, controller, action), StringComparer.InvariantCultureIgnoreCase)) {
                if (!TryValidateKey(_request.QueryString["ApiKey"] ?? _request.Headers["ApiKey"], (_request.QueryString["ApiKey"] != null && _request.QueryString["clear"] != "false"))) {
                    Logger.Error(String.Format("UnauthorizedApi: {0}", _request.QueryString["ApiKey"] ?? _request.Headers["ApiKey"]));
                    _additionalCacheKey = "UnauthorizedApi;";
                    if (filterContext != null) {
                        ErrorResult(filterContext, String.Format("UnauthorizedApi: {0}", _request.QueryString["ApiKey"] ?? _request.Headers["ApiKey"]));
                        return;
                    }
                } else {
                    _additionalCacheKey = "AuthorizedApi;";
                }
            }

        }

        private bool TryValidateKey(string token, bool clearText) {
            byte[] mykey = _shellSettings.EncryptionKey.ToByteArray();
            byte[] myiv = Encoding.UTF8.GetBytes(string.Format("{0}{0}", DateTime.UtcNow.ToString("ddMMyyyy").Substring(0, 8)));
            try {
                if (String.IsNullOrWhiteSpace(token)) {
                    Logger.Error("Empty Token");
                    return false;
                }

                string key = token;
                if (!clearText) {
                    var encryptedAES = Convert.FromBase64String(token);
                    key = DecryptStringFromBytes_Aes(encryptedAES, mykey, myiv);
                    //key = aes.Decrypt(token, mykey, myiv);
                } else {
                    var encryptedAES = EncryptStringToBytes_Aes(token, mykey, myiv);
                    var base64EncryptedAES = Convert.ToBase64String(encryptedAES, Base64FormattingOptions.None);
                    //var encrypted = aes.Crypt(token, mykey, myiv);
                    if (_request.QueryString["unmask"] == "true") {
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.Write("Encoded: " + HttpContext.Current.Server.UrlEncode(base64EncryptedAES) + "<br/>");
                        HttpContext.Current.Response.Write("Clear: " + base64EncryptedAES);
                        HttpContext.Current.Response.End();

                    }
                }

                var settings = _orchardServices.WorkContext.CurrentSite.As<ProtectionSettingsPart>();
                if (!settings.ExternalApplicationList.ExternalApplications.Any(x => x.ApiKey.Equals(key))) {
                    Logger.Error("Decrypted key not found: key = " + key);
                    return false;
                }

                return true;
            } catch (Exception ex) {
                Logger.Error("Exception: " + ex.Message);
                return false;
            }
        }



        byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV) {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = CreateCryptoService(Key, IV)) {

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream()) {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV) {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = CreateCryptoService(Key, IV)) {
                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText)) {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        private AesCryptoServiceProvider CreateCryptoService(byte[] key, byte[] iv) {
            AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider();
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            return aesAlg;

        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            ValidateRequestByApiKey(filterContext);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
        }

        /// <summary>
        /// Called by OutpuCache after the default cache key has been defined
        /// </summary>
        /// <param name="key">default cache key such as defined in Orchard.OutpuCache</param>
        /// <returns>The new cache key</returns>
        public StringBuilder InflatingCacheKey(StringBuilder key) {
            ValidateRequestByApiKey(null);
            key.Append(_additionalCacheKey);
            return key;
        }
    }

}



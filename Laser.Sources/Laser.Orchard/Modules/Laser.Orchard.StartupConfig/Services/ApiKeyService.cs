using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Orchard.ContentManagement;
using Orchard.Utility.Extensions;
using System.Text;

namespace Laser.Orchard.StartupConfig.Services {
    public class ApiKeyService : IApiKeyService {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly HttpRequest _request;
        public ILogger Logger;

        public ApiKeyService(ShellSettings shellSettings, IOrchardServices orchardServices) {
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;
            _request = HttpContext.Current.Request;
            Logger = NullLogger.Instance;
        }
        public string ValidateRequestByApiKey(string additionalCacheKey, bool protectAlways = false) {
            if (additionalCacheKey != null) {
                return additionalCacheKey;
            }
            bool check = false;
            if (protectAlways == false) {
                var settings = _orchardServices.WorkContext.CurrentSite.As<ProtectionSettingsPart>();
                if (String.IsNullOrWhiteSpace(settings.ProtectedEntries)) {
                    return additionalCacheKey; // non ci sono entries da proteggere
                }
                var protectedControllers = settings.ProtectedEntries.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var area = _request.RequestContext.RouteData.Values["area"];
                var controller = _request.RequestContext.RouteData.Values["controller"];
                var action = _request.RequestContext.RouteData.Values["action"];
                var entryToVerify = "";
                if (action == null) {
                    // caso che si verifica con le web api (ApiController)
                    entryToVerify = String.Format("{0}.{1}", area, controller);
                }
                else {
                    // caso che si verifica con i normali Controller
                    entryToVerify = String.Format("{0}.{1}.{2}", area, controller, action);
                }
                if (protectedControllers.Contains(entryToVerify, StringComparer.InvariantCultureIgnoreCase)) {
                    check = true;
                }
            }
            else {
                check = true;
            }

            if (check == true) {
                if (!TryValidateKey(_request.QueryString["ApiKey"] ?? _request.Headers["ApiKey"], (_request.QueryString["ApiKey"] != null && _request.QueryString["clear"] != "false"))) {
                    additionalCacheKey = "UnauthorizedApi";
                    //HttpContext.Current.Response.Clear();
                    //HttpContext.Current.Response.StatusCode = 401;
                    //HttpContext.Current.Response.Write("Error");
                    //HttpContext.Current.Response.End();
                }
                else {
                    additionalCacheKey = "AuthorizedApi";
                }
            }
            return additionalCacheKey;
        }

        public string GetValidApiKey() {
            string key = "";
            byte[] mykey = _shellSettings.EncryptionKey.ToByteArray();
            byte[] myiv = Encoding.UTF8.GetBytes(string.Format("{0}{0}", DateTime.UtcNow.ToString("ddMMyyyy").Substring(0, 8)));
            try {
                var settings = _orchardServices.WorkContext.CurrentSite.As<ProtectionSettingsPart>();
                var defaulApp = settings.ExternalApplicationList.ExternalApplications.First();
                string aux = defaulApp.ApiKey;

                byte[] encryptedAES = EncryptStringToBytes_Aes(aux, mykey, myiv);
                key = Convert.ToBase64String(encryptedAES);
            }
            catch {
                // ignora volutamente qualsiasi errore e restituisce una stringa vuota
            }
            return key;
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
                }
                else {
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
            }
            catch (Exception ex) {
                Logger.Error("Exception: " + ex.Message);
                return false;
            }
        }

        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV) {
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

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV) {
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
    }
}
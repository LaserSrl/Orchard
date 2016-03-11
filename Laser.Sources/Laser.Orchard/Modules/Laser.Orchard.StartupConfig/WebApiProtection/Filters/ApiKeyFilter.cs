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
using Orchard.Logging;
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

        private readonly HttpRequest request;

        public ApiKeyFilter(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
            request = HttpContext.Current.Request;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger;

        public System.Text.StringBuilder InflatingCacheKey(System.Text.StringBuilder key) {
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
                } else {
                    key.Append("AuthorizedApi");
                }
            }
            return key;
        }


        private bool TryValidateKey(string token, bool clearText) {
            MyAES aes = new MyAES();
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
                    if (request.QueryString["unmask"] == "true") {
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.Write("Encoded: " + HttpContext.Current.Server.UrlEncode(base64EncryptedAES) + "<br/>");
                        HttpContext.Current.Response.Write("Clear: " + base64EncryptedAES);
                        HttpContext.Current.Response.End();

                    }
                }

                //TODO: Sostituire con key prelelvate dai settings
                var allowedKeys = new string[] {
                    "OZVV5TpP4U6wJthaCORZEQ",
                    "DJSH348579DEJfjdklsFHK",
                    "dhj940FHJSfljs905890fk"
                };


                if (!allowedKeys.Contains(key)) {
                    Logger.Error("Decrypted key not found: key = " + key);
                    return false;
                }

                return true;
            } catch (Exception ex){
                Logger.Error("Exception: "+ex.Message);
                return false;
            }
        }
        //byte[] EncryptStringToBytes_Aes(string plainText) {

        //    // Check arguments.
        //    if (plainText == null || plainText.Length <= 0)
        //        throw new ArgumentNullException("plainText");

        //    byte[] data;
        //    byte[] encryptedData;

        //    using (var ms = new MemoryStream()) {
        //        using (var symmetricAlgorithm = CreateSymmetricAlgorithm()) {

        //            // Converto il testo in byte unicode
        //            data = Encoding.Unicode.GetBytes(plainText);
        //            using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write)) {
        //                // cifro i byte unicode nel cryptostream
        //                cs.Write(data, 0, data.Length);
        //                cs.FlushFinalBlock();
        //            }

        //            // trasformo lo streaming in bytearray
        //            encryptedData = ms.ToArray();
        //            return encryptedData;
        //        }
        //    }
        //}

        //string DecryptStringFromBytes_Aes(byte[] cipherText) {
        //    // Check arguments.
        //    if (cipherText == null || cipherText.Length <= 0)
        //        throw new ArgumentNullException("cipherText");



        //    using (var symmetricAlgorithm = CreateSymmetricAlgorithm()) {
        //        var iv = new byte[symmetricAlgorithm.BlockSize / 8];
        //        var data = new byte[cipherText.Length - iv.Length];

        //        Array.Copy(cipherText, 0, iv, 0, iv.Length);
        //        Array.Copy(cipherText, iv.Length, data, 0, data.Length);

        //        using (var ms = new MemoryStream()) {
        //            using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Write)) {
        //                cs.Write(data, 0, data.Length);
        //                cs.FlushFinalBlock();
        //            }
        //            return System.Text.Encoding.Unicode.GetString(ms.ToArray());
        //        }
        //    }

        //}
        //private SymmetricAlgorithm CreateSymmetricAlgorithm() {
        //    var algorithm = SymmetricAlgorithm.Create(_shellSettings.EncryptionAlgorithm);
        //    algorithm.Key = _shellSettings.EncryptionKey.ToByteArray();
        //    algorithm.IV = Encoding.Unicode.GetBytes(DateTime.UtcNow.ToString("ddMMyyyy").Substring(0, 8));
        //    algorithm.Padding = PaddingMode.Zeros;
        //    return algorithm;
        //}


        /*aaaaaaaaaaaaaaaaaaa*/
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
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider()) {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.ANSIX923;

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
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider()) {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.ANSIX923;
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
    /*aaaaaaaaaaaaaaaaaaa*/
    
    }





//*************************************************************************
    class MyAES
    {
        public string Crypt(string text, byte[] key, byte[] iv)
        {
            //byte[] bKey = String2Bytes(key);
            //byte[] bIv = String2Bytes(iv);
            byte[] encrypted = EncryptStringToBytes(text, key, iv);
            return Bytes2String(encrypted);
        }
        public string Decrypt(string text, byte[] key, byte[] iv)
        {
            //byte[] bKey = String2Bytes(key);
            //byte[] bIv = String2Bytes(iv);
            byte[] crypted = String2Bytes(text);
            return DecryptStringFromBytes(crypted, key, iv);
        }

        private string Bytes2String(byte[] buffer)
        {
            ////return System.Text.ASCIIEncoding.UTF8.GetString(buffer);
            StringBuilder sb = new StringBuilder();

            foreach (var bb in buffer)
            {
                sb.AppendFormat("{0:x2}", bb);
            }
            return sb.ToString();

            //return System.Text.ASCIIEncoding.Unicode.GetString(buffer);
        }

        private byte[] String2Bytes(string hexString)
        {
            int lun = hexString.Length / 2;
            byte[] buffer = new byte[lun];
            //return System.Text.ASCIIEncoding.UTF8.GetBytes(text);
            for (int i = 0; i < lun; i++)
            {
                buffer[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return buffer;

            //return System.Text.ASCIIEncoding.Unicode.GetBytes(text);
        }

        public byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
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

        public string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
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

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}



using System;
using System.Collections.Generic;
using Laser.Orchard.FidelityGateway.Services;
using Laser.Orchard.FidelityGateway.Models;
using System.Net.Http;
using System.Net;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Laser.Orchard.FidelityLoyalzoo.Services
{
    public class LoyalzooSendService : ISendService
    {

        public APIResult<FidelityCustomer> SendCustomerRegistration(FidelitySettingsPart setPart, FidelityCustomer customer)
        {
            APIResult<FidelityCustomer> result = new APIResult<FidelityCustomer>();
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("username", customer.Username),
                    new KeyValuePair<string, string>("password",customer.Password),
                    new KeyValuePair<string, string>("email",customer.Email), //TODO vedeere se è oggnligatorio inserire il first-name
                };
                foreach (var otherData in customer.Data)
                {
                    kvpList.Add(new KeyValuePair<string, string>(otherData.Key, otherData.Value));
                }
                string responseString = SendRequest(setPart, APIType.customer, "create", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<bool>("success");
                    if (result.success)
                    {
                        customer.Data = this.DictionaryFromResponseToken(data.SelectToken("response"));
                        RemoveCustomerPropertyFromDataDictionary(customer);
                        result.data = customer;
                        result.message = "Loyalzoo registration success.";
                    }
                    else
                    {
                        result.data = null;
                        result.message = data.SelectToken("response").ToString();
                    }
                }
                else
                {
                    result.success = false;
                    result.data = null;
                    result.message = "no response from Loyalzoo server.";
                }
            }
            catch (Exception ex)
            {
                result.success = false;
                result.data = null;
                result.message = "exception: " + ex.Message + "; in method send service of LoyalzooFidelityModule.";
            }
            return result;
        }

        public string SendGetCustomerSessionId(FidelitySettingsPart setPart, FidelityCustomer customer)
        {
            //APIResult<FidelityCustomer> result = new APIResult<FidelityCustomer>();
            //try
            //{
            //    List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
            //    {
            //        new KeyValuePair<string, string>("username", customer.Username),
            //        new KeyValuePair<string, string>("password",customer.Password),
            //        new KeyValuePair<string, string>("email",customer.Email), //TODO vedeere se è oggnligatorio inserire il first-name
            //    };
            //    foreach (var otherData in customer.Data)
            //    {
            //        kvpList.Add(new KeyValuePair<string, string>(otherData.Key, otherData.Value));
            //    }
            //    string responseString = SendRequest(setPart, APIType.customer, "create", kvpList);
            //    if (!string.IsNullOrWhiteSpace(responseString))
            //    {
            //        JObject data = JObject.Parse(responseString);
            //        result.success = data.Value<bool>("success");
            //        if (result.success)
            //        {
            //            customer.Data = this.DictionaryFromResponseToken(data.SelectToken("response"));
            //            RemoveCustomerPropertyFromDataDictionary(customer);
            //            result.data = customer;
            //            result.message = "Loyalzoo registration success.";
            //        }
            //        else
            //        {
            //            result.data = null;
            //            result.message = data.SelectToken("response").ToString();
            //        }
            //    }
            //    else
            //    {
            //        result.success = false;
            //        result.data = null;
            //        result.message = "no response from Loyalzoo server.";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    result.success = false;
            //    result.data = null;
            //    result.message = "exception: " + ex.Message + "; in method send service of LoyalzooFidelityModule.";
            //}
            //return result;
            throw new NotImplementedException();
        }


        public APIResult<FidelityCustomer> SendCustomerDetails(FidelitySettingsPart setPart, FidelityCustomer customer)
        {
            throw new NotImplementedException();
        }

        public APIResult<FidelityCampaign> SendCampaignData(FidelitySettingsPart setPart, FidelityCampaign customer)
        {
            throw new NotImplementedException();
        }

        public APIResult<List<FidelityCampaign>> SendCampaignList(FidelitySettingsPart setPart)
        {
            throw new NotImplementedException();
        }

        public APIResult<FidelityCustomer> SendAddPoints(FidelitySettingsPart setPart, FidelityCustomer customer, FidelityCampaign campaign, double points)
        {
            throw new NotImplementedException();
        }

        public APIResult<FidelityReward> SendGiveReward(FidelitySettingsPart setPart, FidelityCustomer customer, FidelityReward reward, FidelityCampaign campaign)
        {
            throw new NotImplementedException();
        }

        public APIResult<FidelityCampaign> SendGetCampaignId(FidelitySettingsPart setPart, FidelityCampaign campaign)
        {
            throw new NotImplementedException();
        }

        public APIResult<string> SendGetMerchantId(FidelitySettingsPart setPart)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invia la richiesta all'URL API
        /// </summary>
        /// <param name="APIType">Tipologia di API da richiamare <see cref="APIType"/></param>
        /// <param name="APIMetodo">Metodo da richiamare</param>
        /// <param name="kvpList">Elenco dei parametri da passare all'API List&lt;KeyValuePair&lt;string, string&gt;&gt;</param>
        /// <returns>Restituisce una stringa in formato json</returns>
        private static string SendRequest(FidelitySettingsPart setPart, APIType APIType, string APIMetodo, List<KeyValuePair<string, string>> kvpList)
        {
            string responseString = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(setPart.ApiURL + APIType.ToString() + "/" + setPart.DeveloperKey + "/" + APIMetodo);

                    var content = new FormUrlEncodedContent(kvpList);

                    HttpResponseMessage result = client.PostAsync(GetAPIMetodo(APIMetodo), content).Result;
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        responseString = result.Content.ReadAsStringAsync().Result;
                        if (responseString.Contains("\"success\":false")) { throw new Exception(responseString); }
                    }
                    else if (result.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("Direttiva non valida");
                    }
                    else
                    {
                        throw new Exception(result.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return responseString;
        }

        /// <summary>
        /// GetAPIMetodo
        /// </summary>
        /// <param name="APIMetodo">Metodo</param>
        /// <returns>string</returns> TODOT ???????????????
        private static string GetAPIMetodo(string APIMetodo)
        {

            string sRetAPI = APIMetodo;

            if (APIMetodo.Contains("/"))
            {
                string[] arrTemp = APIMetodo.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                sRetAPI = arrTemp[arrTemp.Length - 1];
            }


            return sRetAPI;
        }

        private enum APIType
        {
            /// <summary>
            /// Merchant
            /// </summary>
            merchant,
            /// <summary>
            /// Customer
            /// </summary>
            customer,
            /// <summary>
            /// User
            /// </summary>
            user
        }

        private Dictionary<string, string> DictionaryFromResponseToken(JToken token)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            JObject response = JObject.Parse(token.ToString());
            foreach (KeyValuePair<string, JToken> entry in response)
            {
                data.Add(entry.Key, entry.Value.ToString());
            }
            return data;
        }

        private void RemoveCustomerPropertyFromDataDictionary(FidelityCustomer customer)
        {
            Dictionary<string, string> data = customer.Data;
            if (data.ContainsKey("email"))
            {
                data.Remove("email");
            }
            if (data.ContainsKey("id"))
            {
                data.Remove("id");
            }
            if (data.ContainsKey("username"))
            {
                data.Remove("username");
            }
            if (data.ContainsKey("password"))
            {
                data.Remove("password");
            }
            if (data.ContainsKey("rewards"))
            {
                data.Remove("password");
            }
        }
    }
}
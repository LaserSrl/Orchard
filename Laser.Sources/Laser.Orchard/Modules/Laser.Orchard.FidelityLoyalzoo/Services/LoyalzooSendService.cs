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
            result.data = null;
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
                        result.message = data.SelectToken("response").ToString();
                    }
                }
                else
                {
                    result.success = false;
                    result.message = "no response from Loyalzoo server.";
                }
            }
            catch (Exception ex)
            {
                result.success = false;
                result.message = "exception: " + ex.Message + "; in method send service of LoyalzooFidelityModule.";
            }
            return result;
        }

        public APIResult<FidelityCustomer> SendCustomerDetails(FidelitySettingsPart setPart, FidelityCustomer customer)
        {
            return loginCustomer(setPart, customer);
        }

        public APIResult<FidelityCampaign> SendCampaignData(FidelitySettingsPart setPart, FidelityCampaign campaign)
        {
            APIResult<FidelityCampaign> result = new APIResult<FidelityCampaign>();
            result.data = null;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("place_id", campaign.Id),
                    new KeyValuePair<string, string>("session_id",setPart.AccountID),
                };
                string responseString = SendRequest(setPart, APIType.merchant, "place", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<bool>("success");
                    if (result.success)
                    {
                        campaign.Data = this.DictionaryFromResponseToken(data.SelectToken("response"));
                        RemoveCampaignPropertyFromDataDictionary(campaign);
                        result.data = campaign;
                        result.message = "Loyalzoo place data request success.";
                    }
                    else
                    {
                        result.message = data.SelectToken("response").ToString();
                    }
                }
                else
                {
                    result.success = false;
                    result.message = "no response from Loyalzoo server.";
                }
            }
            catch (Exception ex)
            {
                result.success = false;
                result.message = "exception: " + ex.Message + "; in method send service of LoyalzooFidelityModule.";
            }
            return result;
        }

        public APIResult<List<string>> SendCampaignIdList(FidelitySettingsPart setPart)
        {
            APIResult<List<string>> result = new APIResult<List<string>>();
            List<string> listCamp = new List<string>();
            APIResult<string> loginResp = merchantLogin(setPart, true);
            result.success = loginResp.success;
            listCamp.Add(loginResp.data);
            result.data = listCamp;
            result.message = loginResp.message;
            return result;           
        }

        public APIResult<bool> SendAddPoints(FidelitySettingsPart setPart, FidelityCustomer customer, FidelityCampaign campaign, string points)
        {
            APIResult<bool> result = new APIResult<bool>();
            result.success = false;
            result.data = false;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("place_id", campaign.Id),
                    new KeyValuePair<string, string>("session_id",setPart.AccountID),
                    new KeyValuePair<string, string>("customer_id", customer.Id),
                    new KeyValuePair<string, string>("points", points),
                };
                string responseString = SendRequest(setPart, APIType.merchant, "givePoints", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<bool>("success");
                    if (result.success)
                    {
                        result.data = true;
                        result.message = "Loyalzoo points added whit success.";
                    }
                    else
                    {
                        result.message = data.SelectToken("response").ToString();
                    }
                }
                else
                {
                    result.message = "no response from Loyalzoo server.";
                }
            }
            catch (Exception ex)
            {
                result.message = "exception: " + ex.Message + "; in method send service of LoyalzooFidelityModule.";
            }
            return result;
        }

        public APIResult<bool> SendGiveReward(FidelitySettingsPart setPart, FidelityCustomer customer, FidelityReward reward, FidelityCampaign campaign)
        {
            APIResult<bool> result = new APIResult<bool>();
            result.data = false;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("place_id", campaign.Id),
                    new KeyValuePair<string, string>("session_id",setPart.AccountID),
                    new KeyValuePair<string, string>("customer_id", customer.Id),
                    new KeyValuePair<string, string>("reward_id", reward.Id),
                };
                string responseString = SendRequest(setPart, APIType.merchant, "giveReward", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<bool>("success");
                    if (result.success)
                    {       
                        result.data = true;
                        result.message = "Loyalzoo reward gived whit success.";
                    }
                    else
                    {
                        result.message = data.SelectToken("response").ToString();
                    }
                }
                else
                {
                    result.message = "no response from Loyalzoo server.";
                }
            }
            catch (Exception ex)
            {
                result.message = "exception: " + ex.Message + "; in method send service of LoyalzooFidelityModule.";
            }
            return result;
        }

        public APIResult<string> SendGetMerchantId(FidelitySettingsPart setPart)
        {
            return merchantLogin(setPart, false);
        }

        private APIResult<string> merchantLogin(FidelitySettingsPart setPart, bool isForPlace)
        {
            string responseString = string.Empty;
            APIResult<string> result = new APIResult<string>();
            result.success = false;
            result.data = null;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("username",setPart.UserID),
                    new KeyValuePair<string, string>("password",setPart.Password),
                };
                responseString = SendRequest(setPart, APIType.merchant, "login", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<bool>("success");
                    if (result.success)
                    {
                        JToken response = data.SelectToken("response");
                        if (!isForPlace)
                        {
                            result.data = response.Value<string>("session_id");
                        }
                        else
                        {
                            result.data = response.Value<string>("place_id");
                        }
                        
                        result.message = "Loyalzoo merchant login success.";
                    }
                    else
                    {
                        result.message = data.SelectToken("response").ToString();
                    }
                }
                else
                {
                    result.message = "no response from Loyalzoo server.";
                }
            }
            catch (Exception ex)
            {
                result.message = "Exception: " + ex.Message + " in Loyalzoo Login.";
            }
            return result;
        }

        private APIResult<FidelityCustomer> loginCustomer(FidelitySettingsPart setPart, FidelityCustomer customer)
        {
            string responseString = string.Empty;
            APIResult<FidelityCustomer> result = new APIResult<FidelityCustomer>();
            result.data = null;
            result.success = false;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("username",customer.Username),
                    new KeyValuePair<string, string>("password",customer.Password),
                };
                responseString = SendRequest(setPart, APIType.customer, "login", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<bool>("success");
                    if (result.success)
                    {
                        customer.Data = this.DictionaryFromResponseToken(data.SelectToken("response"));
                        RemoveCustomerPropertyFromDataDictionary(customer);
                        result.data = customer;
                        result.message = "Loyalzoo customer login success.";
                    }
                    else
                    {
                        result.message = data.SelectToken("response").ToString();
                    }
                }
                else
                {
                    result.message = "no response from Loyalzoo server.";
                }
            }
            catch (Exception ex)
            {
                result.message = "Exception: " + ex.Message + " in Loyalzoo Login.";
            }
            return result;
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
                        if (responseString.Contains("\"success\":false")) { throw new Exception(responseString); } //TODO vedere se ha senso mantenere il lancio dell'eccezione qui e tutti i controlli sui metodi dopo
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
                AddPointsInPlaceToCustomer(data["rewards"], customer);
                data.Remove("rewards");
            }
        }

        //agginge al dizionario pointsInCampaign del customer i valori presenti nel json,
        //il nome del parametro è infelice, ma cosi è impostato nella response di loyalzoo.
        //Inoltre non vengono gestite place che invece di restituire un numero restituiscono 
        //una lista di "non so che cosa" (vedi anche vecchio modulo) TODO 
        private void AddPointsInPlaceToCustomer(string jsonRewards, FidelityCustomer customer)
        {
            JObject rew = JObject.Parse(jsonRewards);
            foreach (KeyValuePair<string, JToken> entry in rew)
            {
                string tokenVal = entry.Value.ToString();
                double points;
                if (Double.TryParse(tokenVal, out points))
                {
                    customer.PointsInCampaign.Add(entry.Key, points);
                }              
            }
        }

        private void RemoveCampaignPropertyFromDataDictionary(FidelityCampaign campaign)
        {
            Dictionary<string, string> data = campaign.Data;
            if (data.ContainsKey("name"))
            {
                campaign.Name = data["name"];
                data.Remove("name");
            }
            if (data.ContainsKey("loyalty_scheme"))
            {
                string jsonList = data["loyalty_scheme"];
                JObject loyalty_scheme = JObject.Parse(data["loyalty_scheme"]);
                AddRewardsInCampaign(loyalty_scheme.SelectToken("rewards"), campaign);
               //TODO data.Remove("rewards");
            }
        }

        private void AddRewardsInCampaign(JToken tokenRewards, FidelityCampaign campaign)
        {
            foreach (JToken tokenRew in tokenRewards.Children())
            {
                FidelityReward reward = new FidelityReward();
                reward.Id = tokenRew.Value<string>("id");
                reward.Description = tokenRew.Value<string>("description");
                reward.Name = tokenRew.Value<string>("name");
                reward.Data.Add("target",tokenRew.Value<string>("target"));
                reward.Data.Add("icon", tokenRew.Value<string>("icon"));
            }
        }
    }
}
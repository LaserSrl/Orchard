using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Laser.Orchard.AppDirect
{
  public class OAuthBase
  {
    protected Random random = new Random();
    protected string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
    protected const string OAuthVersion = "1.0";
    protected const string OAuthParameterPrefix = "oauth_";
    protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
    protected const string OAuthCallbackKey = "oauth_callback";
    protected const string OAuthVersionKey = "oauth_version";
    protected const string OAuthSignatureMethodKey = "oauth_signature_method";
    protected const string OAuthSignatureKey = "oauth_signature";
    protected const string OAuthTimestampKey = "oauth_timestamp";
    protected const string OAuthNonceKey = "oauth_nonce";
    protected const string OAuthTokenKey = "oauth_token";
    protected const string OAuthTokenSecretKey = "oauth_token_secret";
    protected const string HMACSHA1SignatureType = "HMAC-SHA1";
    protected const string PlainTextSignatureType = "PLAINTEXT";
    protected const string RSASHA1SignatureType = "RSA-SHA1";

    private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
    {
      if (hashAlgorithm == null)
        throw new ArgumentNullException("hashAlgorithm");
      if (string.IsNullOrEmpty(data))
        throw new ArgumentNullException("data");
      byte[] bytes = Encoding.ASCII.GetBytes(data);
      return Convert.ToBase64String(hashAlgorithm.ComputeHash(bytes));
    }

    private List<OAuthBase.QueryParameter> GetQueryParameters(string parameters)
    {
      if (parameters.StartsWith("?"))
        parameters = parameters.Remove(0, 1);
      List<OAuthBase.QueryParameter> queryParameterList = new List<OAuthBase.QueryParameter>();
      if (!string.IsNullOrEmpty(parameters))
      {
        string str = parameters;
        char[] chArray = new char[1]{ '&' };
        foreach (string name in str.Split(chArray))
        {
          if (!string.IsNullOrEmpty(name) && !name.StartsWith("oauth_"))
          {
            if (name.IndexOf('=') > -1)
            {
              string[] strArray = name.Split('=');
              queryParameterList.Add(new OAuthBase.QueryParameter(strArray[0], strArray[1]));
            }
            else
              queryParameterList.Add(new OAuthBase.QueryParameter(name, string.Empty));
          }
        }
      }
      return queryParameterList;
    }

    protected string UrlEncode(string value)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in value)
      {
        if (this.unreservedChars.IndexOf(ch) != -1)
          stringBuilder.Append(ch);
        else
          stringBuilder.Append("%" + string.Format("{0:X2}", (object) (int) ch));
      }
      return stringBuilder.ToString();
    }

    protected string NormalizeRequestParameters(IList<OAuthBase.QueryParameter> parameters)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < parameters.Count; ++index)
      {
        OAuthBase.QueryParameter parameter = parameters[index];
        stringBuilder.AppendFormat("{0}={1}", (object) parameter.Name, (object) parameter.Value);
        if (index < parameters.Count - 1)
          stringBuilder.Append("&");
      }
      return stringBuilder.ToString();
    }

    public string GenerateSignatureBase(Uri url, string consumerKey, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters)
    {
      if (token == null)
        token = string.Empty;
      if (tokenSecret == null)
        tokenSecret = string.Empty;
      if (string.IsNullOrEmpty(consumerKey))
        throw new ArgumentNullException("consumerKey");
      if (string.IsNullOrEmpty(httpMethod))
        throw new ArgumentNullException("httpMethod");
      if (string.IsNullOrEmpty(signatureType))
        throw new ArgumentNullException("signatureType");
      normalizedUrl = (string) null;
      normalizedRequestParameters = (string) null;
      List<OAuthBase.QueryParameter> queryParameters = this.GetQueryParameters(url.Query);
      queryParameters.Add(new OAuthBase.QueryParameter("oauth_version", "1.0"));
      queryParameters.Add(new OAuthBase.QueryParameter("oauth_nonce", nonce));
      queryParameters.Add(new OAuthBase.QueryParameter("oauth_timestamp", timeStamp));
      queryParameters.Add(new OAuthBase.QueryParameter("oauth_signature_method", signatureType));
      queryParameters.Add(new OAuthBase.QueryParameter("oauth_consumer_key", consumerKey));
      if (!string.IsNullOrEmpty(token))
        queryParameters.Add(new OAuthBase.QueryParameter("oauth_token", token));
      queryParameters.Sort((IComparer<OAuthBase.QueryParameter>) new OAuthBase.QueryParameterComparer());
      normalizedUrl = string.Format("{0}://{1}", (object) url.Scheme, (object) url.Host);
      if ((!(url.Scheme == "http") || url.Port != 80) && (!(url.Scheme == "https") || url.Port != 443))
        normalizedUrl = normalizedUrl + ":" + (object) url.Port;
      normalizedUrl = normalizedUrl + url.AbsolutePath;
      normalizedRequestParameters = this.NormalizeRequestParameters((IList<OAuthBase.QueryParameter>) queryParameters);
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat("{0}&", (object) httpMethod.ToUpper());
      stringBuilder.AppendFormat("{0}&", (object) this.UrlEncode(normalizedUrl));
      stringBuilder.AppendFormat("{0}", (object) this.UrlEncode(normalizedRequestParameters));
      return stringBuilder.ToString();
    }

    public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
    {
      return this.ComputeHash(hash, signatureBase);
    }

    public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out string normalizedRequestParameters)
    {
      return this.GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, httpMethod, timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters);
    }

    public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, OAuthBase.SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters)
    {
      normalizedUrl = (string) null;
      normalizedRequestParameters = (string) null;
      switch (signatureType)
      {
        case OAuthBase.SignatureTypes.HMACSHA1:
          string signatureBase = this.GenerateSignatureBase(url, consumerKey, token, tokenSecret, httpMethod, timeStamp, nonce, "HMAC-SHA1", out normalizedUrl, out normalizedRequestParameters);
          HMACSHA1 hmacshA1 = new HMACSHA1();
          hmacshA1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", (object) this.UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? (object) "" : (object) this.UrlEncode(tokenSecret)));
          return this.GenerateSignatureUsingHash(signatureBase, (HashAlgorithm) hmacshA1);
        case OAuthBase.SignatureTypes.PLAINTEXT:
          return HttpUtility.UrlEncode(string.Format("{0}&{1}", (object) consumerSecret, (object) tokenSecret));
        case OAuthBase.SignatureTypes.RSASHA1:
          throw new NotImplementedException();
        default:
          throw new ArgumentException("Unknown signature type", "signatureType");
      }
    }

    public virtual string GenerateTimeStamp()
    {
      return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();
    }

    public virtual string GenerateNonce()
    {
      return this.random.Next(123400, 9999999).ToString();
    }

    public enum SignatureTypes
    {
      HMACSHA1,
      PLAINTEXT,
      RSASHA1,
    }

    protected class QueryParameter
    {
      private string name = (string) null;
      private string value = (string) null;

      public string Name
      {
        get
        {
          return this.name;
        }
      }

      public string Value
      {
        get
        {
          return this.value;
        }
      }

      public QueryParameter(string name, string value)
      {
        this.name = name;
        this.value = value;
      }
    }

    protected class QueryParameterComparer : IComparer<OAuthBase.QueryParameter>
    {
      public int Compare(OAuthBase.QueryParameter x, OAuthBase.QueryParameter y)
      {
        if (x.Name == y.Name)
          return string.Compare(x.Value, y.Value);
        return string.Compare(x.Name, y.Name);
      }
    }
  }
}

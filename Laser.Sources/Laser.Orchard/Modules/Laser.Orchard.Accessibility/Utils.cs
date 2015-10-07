using System.Web;

namespace Laser.Orchard.Accessibility
{
    public class Utils
    {
        public const string AccessibilityCookieName = "Accessibility";
        public const string AccessibilityTextOnly = "textonly";
        public const string AccessibilityNormal = "";
        public const string AccessibilityHighContrast = "highcontrast";

        /// <summary>
        /// Get the value of the cookie with the specified name.
        /// If there is more than one cookie with that name, the first one is returned.
        /// Usually, this is the more specialized cookie in terms of domain and path.
        /// This is useful if you want a tenant-specific cookie.
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public string getTenantCookieValue(string cookieName, HttpRequestBase request)
        {
            string result = "";
            HttpCookie cookie = null;

            // cicla perché potrebbero esserci più cookie con lo stesso nome e path differente
            for (int i = 0; i < request.Cookies.Count; i++)
            {
                cookie = request.Cookies[i];
                if (cookie.Name == cookieName)
                {
                    result = cookie.Value;
                    // esce dopo aver trovato il primo cookie col nome cercato perché dovrebbe
                    // essere quello più specifico come dominio e path, quindi specifico del tenant
                    break;
                }
            }

            return result;
        }
    }
}
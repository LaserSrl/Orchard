using Laser.Orchard.SEO.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.SEO.Services {
    public interface IRedirectService : IDependency {
        /// <summary>
        /// Gets the RedirectRule objects based on the pagination
        /// </summary>
        /// <param name="startIndex">Start index for pagination</param>
        /// <param name="pageSize">Page size (maximum number of objects)</param>
        /// <returns>An IEnumerable of RedirectRule objects</returns>
        IEnumerable<RedirectRule> GetRedirects(int startIndex = 0, int pageSize = 0);

        IEnumerable<RedirectRule> GetRedirects(int[] itemIds);

        /// <summary>
        /// Get the total number of RedirectRule objects.
        /// </summary>
        /// <returns>The total number of RedirectRule objects.</returns>
        int GetRedirectsTotalCount();

        RedirectRule GetRedirect(int id);
        RedirectRule GetRedirect(string path);
        RedirectRule Update(RedirectRule redirectRule);
        RedirectRule Add(RedirectRule redirectRule);
        void Delete(int id);
        void Delete(RedirectRule redirectRule);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.SEO.Models;
using Orchard.Data;

namespace Laser.Orchard.SEO.Services {
    public class RedirectService : IRedirectService {
        private readonly IRepository<RedirectRule> _repository;
        public RedirectService(
            IRepository<RedirectRule> repository) {

            _repository = repository;
        }
        public IEnumerable<RedirectRule> GetRedirects(int startIndex = 0, int pageSize = 0) {
            var result = _repository.Table.Skip(startIndex >= 0 ? startIndex : 0);

            if (pageSize > 0) {
                return result.Take(pageSize);
            }
            return result;
        }

        public int GetRedirectsTotalCount() {
            return _repository.Table.Count();
        }
    }
}
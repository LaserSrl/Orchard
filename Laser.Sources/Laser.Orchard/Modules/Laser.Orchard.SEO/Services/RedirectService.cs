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
        public IEnumerable<RedirectRule> GetRedirects(int[] itemIds) {
            return _repository.Fetch(x => itemIds.Contains(x.Id));
        }

        public int GetRedirectsTotalCount() {
            return _repository.Table.Count();
        }

        public RedirectRule Update(RedirectRule redirectRule) {
            FixRedirect(redirectRule);
            _repository.Update(redirectRule);
            return redirectRule;
        }

        public RedirectRule Add(RedirectRule redirectRule) {
            FixRedirect(redirectRule);
            _repository.Create(redirectRule);
            return redirectRule;
        }

        public void Delete(RedirectRule redirectRule) {
            _repository.Delete(redirectRule);
        }

        public void Delete(int id) {
            var redirect = GetRedirect(id);

            _repository.Delete(redirect);
        }

        public RedirectRule GetRedirect(string path) {
            path = path.TrimStart('/');
            return _repository.Get(x => x.SourceUrl == path);
        }

        public RedirectRule GetRedirect(int id) {
            return _repository.Get(id);
        }

        private static void FixRedirect(RedirectRule redirectRule) {
            redirectRule.SourceUrl = redirectRule.SourceUrl.TrimStart('/');
            redirectRule.DestinationUrl = redirectRule.DestinationUrl.TrimStart('/');
        }

    }
}
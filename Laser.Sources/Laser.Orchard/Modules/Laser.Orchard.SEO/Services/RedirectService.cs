using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.SEO.Models;
using Orchard.Data;
using Laser.Orchard.SEO.Exceptions;
using Orchard.Localization;

namespace Laser.Orchard.SEO.Services {
    public class RedirectService : IRedirectService {
        private readonly IRepository<RedirectRule> _repository;
        public RedirectService(
            IRepository<RedirectRule> repository) {

            _repository = repository;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<RedirectRule> GetRedirects(int startIndex = 0, int pageSize = 0) {
            var result = _repository.Table.Skip(startIndex >= 0 ? startIndex : 0);

            if (pageSize > 0) {
                return RedirectRule.Copy(result.Take(pageSize));
            }
            return RedirectRule.Copy(result.ToList());
        }

        public IEnumerable<RedirectRule> GetRedirects(int[] itemIds) {
            return RedirectRule.Copy(_repository.Fetch(x => itemIds.Contains(x.Id)));
        }

        public int GetRedirectsTotalCount() {
            return _repository.Table.Count();
        }

        public RedirectRule Update(RedirectRule redirectRule) {
            FixRedirect(redirectRule);
            if (_repository.Table.Any(rr => rr.Id != redirectRule.Id && rr.SourceUrl == redirectRule.SourceUrl)) {
                throw new RedirectRuleDuplicateException(T("Rules with same SourceURL are not valid."));
            }
            _repository.Update(redirectRule);
            return redirectRule;
        }

        public RedirectRule Add(RedirectRule redirectRule) {
            FixRedirect(redirectRule);
            if (_repository.Table.Any(rr => rr.SourceUrl == redirectRule.SourceUrl)) {
                throw new RedirectRuleDuplicateException(T("Rules with same SourceURL are not valid."));
            }
            _repository.Create(redirectRule);
            return redirectRule;
        }

        public void Delete(RedirectRule redirectRule) {
            Delete(redirectRule.Id);
        }

        public void Delete(int id) {
            var redirect = _repository.Get(id);

            _repository.Delete(redirect);
        }

        public RedirectRule GetRedirect(string path) {
            path = path.TrimStart('/');
            var rule = _repository.Get(x => x.SourceUrl == path);
            return rule == null ? null :
                RedirectRule.Copy(rule);
        }

        public RedirectRule GetRedirect(int id) {
            var rule = _repository.Get(id);
            return rule == null ? null :
                RedirectRule.Copy(rule);
        }
        
        private static void FixRedirect(RedirectRule redirectRule) {
            redirectRule.SourceUrl = redirectRule.SourceUrl.TrimStart('/');
            redirectRule.DestinationUrl = redirectRule.DestinationUrl.TrimStart('/');
        }

    }
}
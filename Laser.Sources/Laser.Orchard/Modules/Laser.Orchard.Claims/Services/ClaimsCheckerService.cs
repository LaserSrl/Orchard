using Laser.Orchard.Claims.Models;
using NHibernate;
using NHibernate.Criterion;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.UI.Admin;
using Orchard.Users.Models;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System;

namespace Laser.Orchard.Claims.Services {
    public interface IClaimsCheckerService : IDependency {
        ContentItem CheckClaims(ContentItem contentItem);
        void CheckClaims(ICriteria criteria);
    }
    public class ClaimsCheckerService : IClaimsCheckerService {
        private readonly IRepository<UserPartRecord> _repoUsers;
        private readonly IRepository<ContentItemRecord> _repoSite;
        private readonly IRepository<IdentityClaimsRecord> _repoClaims;
        private bool _isBackEnd;
        private List<List<string>> _userClaims;
        private bool _isSuperUser;
        private bool _applyToFrontEnd;
        // Warning: se si aggiungono parametri al costruttore prestare attenzione a non creare dipendenze circolari
        // perché questo costruttore viene richiamato PRIMA di avere a disposizione il content manager.
        public ClaimsCheckerService(
            IRepository<UserPartRecord> repoUsers, 
            IRepository<ContentItemRecord> repoSite,
            IRepository<IdentityClaimsRecord> repoClaims) {
            _repoUsers = repoUsers;
            _repoSite = repoSite;
            _repoClaims = repoClaims;
            _userClaims = new List<List<string>>();
            var context = HttpContext.Current;
            string userName = "";
            _applyToFrontEnd = true;
            try {
                _isBackEnd = AdminFilter.IsApplied(context.Request.RequestContext);
                if (_isBackEnd == false) {
                    _isBackEnd = context.Request.RawUrl.IndexOf("/Admin/", StringComparison.InvariantCultureIgnoreCase) >= 0;
                }
            } 
            catch {
                _isBackEnd = false;
            }
            // recupera il site per capire se l'utente è un amministratore e leggere i settings
            var aux = _repoSite.Get(1).Infoset;
            try {
                userName = context.User.Identity.Name;
                var superuser = aux.Element.Element("SiteSettingsPart").Attribute("SuperUser").Value;
                if (userName == superuser) {
                    _isSuperUser = true;
                }
            } catch {
                _isSuperUser = true;
            }
            try { 
                var applyToFrontEnd = aux.Element.Element("ClaimsSiteSettings").Attribute("ApplyToFrontEnd").Value;
                if(applyToFrontEnd.Equals("true", StringComparison.InvariantCultureIgnoreCase) == false) {
                    _applyToFrontEnd = false;
                } 
            } catch {
                _applyToFrontEnd = true;
            }
            // recupera le claims dell'utente
            try {
                if(string.IsNullOrWhiteSpace(userName) == false) {
                    // recupera l'id dell'utente
                    var userRecord = _repoUsers.Fetch(x => x.UserName == userName).FirstOrDefault();
                    if (userRecord != null) {
                        var userId = userRecord.Id;
                        // recupera le claims dell'utente tramite un repository
                        var claimsList = _repoClaims.Fetch(x => x.IdentityClaimsPartRecord_id == userId);
                        foreach(var set in claimsList) {
                            var claims = new List<string>();
                            foreach(var val in set.IdentityClaims.Split(',')) {
                                claims.Add(val);
                            }
                            _userClaims.Add(claims);
                        }
                    }
                }
            }
            catch {
                // non aggiunge claims all'utente
            }
        }
        public ContentItem CheckClaims(ContentItem contentItem) {
            if (_isSuperUser || (_isBackEnd == false && _applyToFrontEnd == false)) {
                return contentItem;
            }
            var claimsPart = contentItem.As<ItemClaimsPart>();
            if(claimsPart != null) {
                var itemClaims = new List<string>();
                if(string.IsNullOrWhiteSpace(claimsPart.Claims) == false) {
                    foreach (var row in claimsPart.Claims.Split(',')) {
                        itemClaims.Add(row);
                    }
                }
                // se l'item non ha nessuna claim è visibile a tutti
                if (itemClaims.Count == 0) {
                    return contentItem;
                }
                // almeno un set di claims dell'utente deve essere presente nell'item
                var granted = false;
                foreach (var set in _userClaims) {
                    // check sul singolo set di claims
                    var setGranted = true;
                    foreach (var row in set) {
                        if (itemClaims.Contains(row) == false) {
                            setGranted = false;
                            break;
                        }
                    }
                    if (setGranted) {
                        granted = true;
                        break;
                    }
                }
                if (granted == false) {
                    return null;
                }
            }
            return contentItem;
        }
        public void CheckClaims(ICriteria criteria) {
            if (_isSuperUser || (_isBackEnd == false && _applyToFrontEnd == false)) {
                return;
            }
            var newCriteria = criteria.CreateCriteria("ItemClaimsPartRecord", "laserClaims", NHibernate.SqlCommand.JoinType.LeftOuterJoin);
            AbstractCriterion crit = null;

            // almeno un set di claims dell'utente deve essere presente nell'item (or di and)
            foreach(var set in _userClaims) {
                AbstractCriterion innerCrit = null;
                foreach (var row in set) {
                    var kv = "%," + row + ",%";
                    if (innerCrit == null) {
                        innerCrit = Restrictions.Like("laserClaims.Claims", kv);
                    } else {
                        innerCrit = Restrictions.And(innerCrit, Restrictions.Like("laserClaims.Claims", kv));
                    }
                }
                if(crit == null) {
                    crit = innerCrit;
                } else {
                    crit = Restrictions.Or(crit, innerCrit);
                }
            }
            if(crit != null) {
                newCriteria = newCriteria.Add(Restrictions.Or(Restrictions.Or(Restrictions.IsNull("laserClaims.Claims"), Restrictions.Eq("laserClaims.Claims", "")), crit));
            }
        }
    }
}
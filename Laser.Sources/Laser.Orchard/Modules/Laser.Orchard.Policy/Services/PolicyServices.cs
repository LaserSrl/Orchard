using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.ViewModels;
using OrchardNS = Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard;
using Orchard.Localization.Services;
using Orchard.Localization.Models;
using Orchard.Data;
using System.Web.Script.Serialization;
using Laser.Orchard.StartupConfig.Services;
using System.Text;
using Orchard.Security;

namespace Laser.Orchard.Policy.Services {
    public interface IPolicyServices : IDependency {
        PoliciesForUserViewModel GetPoliciesForUserOrSession(bool writeMode, string language = null);
        void PolicyForUserUpdate(PolicyForUserViewModel viewModel, IUser user = null);
        void PolicyForUserMassiveUpdate(IList<PolicyForUserViewModel> viewModelCollection, IUser user = null);
        IList<PolicyForUserViewModel> GetCookieOrVolatileAnswers();
        void CreateAndAttachPolicyCookie(IList<PolicyForUserViewModel> viewModelCollection, bool writeMode);
        string[] GetPoliciesForContent(PolicyPart part);
        IEnumerable<PolicyTextInfoPart> GetPolicies(string culture = null, int[] ids = null);
    }

    public class PolicyServices : IPolicyServices {
        private readonly IContentManager _contentManager;
        private readonly OrchardNS.IWorkContextAccessor _workContext;
        private readonly ICultureManager _cultureManager;
        private readonly IRepository<UserPolicyAnswersRecord> _userPolicyAnswersRepository;
        private readonly IControllerContextAccessor _controllerContextAccessor;


        public PolicyServices(IContentManager contentManager, OrchardNS.IWorkContextAccessor workContext, ICultureManager cultureManager, IRepository<UserPolicyAnswersRecord> userPolicyAnswersRepository, IControllerContextAccessor controllerContextAccessor) {
            _contentManager = contentManager;
            _workContext = workContext;
            _cultureManager = cultureManager;
            _userPolicyAnswersRepository = userPolicyAnswersRepository;
            _controllerContextAccessor = controllerContextAccessor;
        }

        public PoliciesForUserViewModel GetPoliciesForUserOrSession(bool writeMode, string language = null) {
            var loggedUser = _workContext.GetContext().CurrentUser;
            var siteLanguage = _workContext.GetContext().CurrentSite.SiteCulture;

            int currentLanguageId;
            IList<PolicyForUserViewModel> model = new List<PolicyForUserViewModel>();
            IContentQuery<PolicyTextInfoPart> query;

            if (!String.IsNullOrWhiteSpace(language)) {
                currentLanguageId = _cultureManager.GetCultureByName(language).Id;
            } else {
                //Nel caso di contenuto senza Localizationpart prendo la CurrentCulture
                currentLanguageId = _cultureManager.GetCultureByName(_workContext.GetContext().CurrentCulture).Id;
            }

            query = _contentManager.Query<PolicyTextInfoPart, PolicyTextInfoPartRecord>()
                                   .OrderByDescending(o => o.Priority)
                                   .Join<LocalizationPartRecord>()
                                   .Where(w => w.CultureId == currentLanguageId || (w.CultureId == 0 && (siteLanguage.Equals(language) || language == null)))
                                   .ForVersion(VersionOptions.Published);

            if (loggedUser != null) { // loggato
                model = query.List().Select(s => {
                    var answer = loggedUser.As<UserPolicyPart>().UserPolicyAnswers.Where(w => w.PolicyTextInfoPartRecord.Id.Equals(s.Id)).SingleOrDefault();
                    return new PolicyForUserViewModel {
                        PolicyText = s,
                        PolicyTextId = s.Id,
                        AnswerId = answer != null ? answer.Id : 0,
                        AnswerDate = answer != null ? answer.AnswerDate : DateTime.MinValue,
                        OldAccepted = answer != null ? answer.Accepted : false,
                        Accepted = answer != null ? answer.Accepted : false,
                    };
                }).ToList();
            } else { // non loggato
                IList<PolicyForUserViewModel> answers = GetCookieOrVolatileAnswers();
                model = query.List().Select(s => {
                    var answer = answers.Where(w => w.PolicyTextId.Equals(s.Id)).SingleOrDefault();
                    return new PolicyForUserViewModel {
                        PolicyText = s,
                        PolicyTextId = s.Id,
                        AnswerId = answer != null ? answer.AnswerId : 0,
                        AnswerDate = answer != null ? answer.AnswerDate : DateTime.MinValue,
                        OldAccepted = answer != null ? answer.Accepted : false,
                        Accepted = answer != null ? answer.Accepted : false,
                    };
                }).ToList();
            }

            CreateAndAttachPolicyCookie(model.ToList(), writeMode);

            return new PoliciesForUserViewModel { Policies = model };
        }


        public void PolicyForUserUpdate(PolicyForUserViewModel viewModel, IUser user = null) {
            if ((viewModel.OldAccepted != viewModel.Accepted || (!viewModel.Accepted && viewModel.AnswerDate == DateTime.MinValue))) {
                var policyText = _contentManager.Get<PolicyTextInfoPart>(viewModel.PolicyTextId).Record;
                if ((policyText.UserHaveToAccept && viewModel.Accepted) || !policyText.UserHaveToAccept) {
                    var loggedUser = user ?? _workContext.GetContext().CurrentUser;
                    var shouldCreateRecord = false;
                    if (loggedUser != null) {
                        UserPolicyAnswersRecord record;
                        if (viewModel.AnswerId <= 0) {
                            // Anche se non ho un Id di risposta, provo a recuperare la risposta in base all'utente e all'Id del testo privacy
                            record = _userPolicyAnswersRepository.Table.Where(w => w.PolicyTextInfoPartRecord.Id == viewModel.PolicyTextId && w.UserPolicyPartRecord.Id == loggedUser.Id).SingleOrDefault();
                            if (record == null) {
                                record = new UserPolicyAnswersRecord();
                                shouldCreateRecord = true;
                            }
                        } else {
                            record = _userPolicyAnswersRepository.Get(viewModel.AnswerId);
                        }
                        record.AnswerDate = DateTime.UtcNow;
                        record.Accepted = viewModel.Accepted;
                        record.UserPolicyPartRecord = loggedUser.As<UserPolicyPart>().Record;
                        record.PolicyTextInfoPartRecord = policyText;
                        if (shouldCreateRecord) {
                            _userPolicyAnswersRepository.Create(record);
                        } else {
                            _userPolicyAnswersRepository.Update(record);
                        }
                        //_userPolicyAnswersRepository.Flush();
                    }
                }
            }
        }


        public void PolicyForUserMassiveUpdate(IList<PolicyForUserViewModel> viewModelCollection, IUser user = null) {
            var loggedUser = user ?? _workContext.GetContext().CurrentUser;
            if (loggedUser != null) {
                foreach (var item in viewModelCollection) {
                    PolicyForUserUpdate(item, loggedUser);
                }
            }
            //Dopo aver salvatao gli eventuali record, aggiorno anche il campo AnswerDate per il cookie. Devo farlo assolutamente dopo il salvataggio in quanto è l'unico modo per stabilire se si tratta di prima risposta o meno.
            CreateAndAttachPolicyCookie(viewModelCollection, true);
        }


        public IList<PolicyForUserViewModel> GetCookieOrVolatileAnswers() {
            var viewModelCollection = _controllerContextAccessor.Context != null ? _controllerContextAccessor.Context.Controller.ViewBag.PoliciesAnswers : null;
            IList<PolicyForUserViewModel> answers;
            try {
                if (viewModelCollection == null) {
                    answers = GetCookieAnswers();
                } else {
                    answers = (IList<PolicyForUserViewModel>)viewModelCollection;
                }
            } catch {
                answers = new List<PolicyForUserViewModel>();
            }
            return answers;
        }

        public void CreateAndAttachPolicyCookie(IList<PolicyForUserViewModel> viewModelCollection, bool writeMode) {

            var newCollection = new List<PolicyForUserViewModel>();

            //Controllo se esistono già delle policy answers nel cookie
            IList<PolicyForUserViewModel> previousPolicyAnswers = GetCookieAnswers();
            if (previousPolicyAnswers.Count > 0) {
                foreach (PolicyForUserViewModel policyAnswer in previousPolicyAnswers) {
                    var upToDateAnswer = viewModelCollection.Where(x => x.PolicyTextId == policyAnswer.PolicyTextId).SingleOrDefault();
                    if (upToDateAnswer == null) {
                        newCollection.Add(policyAnswer); // Se la risposta nel cookie non ha un corrispettivo nel json la aggiungo sempre al nuovo cookie
                    } else if (upToDateAnswer.Accepted == policyAnswer.Accepted) {
                        newCollection.Add(policyAnswer); // Se si ripete con lo stesso esito riporto quella vecchia in modo da mantenere la data di accettazione
                        viewModelCollection.Remove(upToDateAnswer);
                    }
                }
            }

            if (writeMode) {
                newCollection.AddRange(viewModelCollection.Select(x => {
                    x.AnswerDate = DateTime.UtcNow;
                    //x.PolicyText = null; // annullo la parte per evitare circolarità nella serializzazione
                    return x;
                }));
            }

            string myObjectJson = new JavaScriptSerializer().Serialize(newCollection.Where(w => {
                var policyText = _contentManager.Get<PolicyTextInfoPart>(w.PolicyTextId);
                if (policyText == null) return false;
                else {
                    var policyTextRecord = policyText.Record;
                    return (policyTextRecord.UserHaveToAccept && w.Accepted) || !policyTextRecord.UserHaveToAccept;
                }
            }));

            var cookie = new HttpCookie("PoliciesAnswers", Convert.ToBase64String(Encoding.UTF8.GetBytes(myObjectJson))) { // cookie salvato in base64
                Expires = DateTime.Now.AddMonths(6)
            };
            if (_controllerContextAccessor.Context != null)
                _controllerContextAccessor.Context.Controller.ViewBag.PoliciesAnswers = viewModelCollection;
            if (_workContext.GetContext().HttpContext.Response.Cookies["PoliciesAnswers"] != null) {
                _workContext.GetContext().HttpContext.Response.Cookies.Set(cookie);
            } else {
                _workContext.GetContext().HttpContext.Response.Cookies.Add(cookie);
            }

        }

        public string[] GetPoliciesForContent(PolicyPart part) {
            var settings = part.Settings.GetModel<PolicyPartSettings>();

            if (!settings.PolicyTextReferences.Contains("{DependsOnContent}"))
                return settings.PolicyTextReferences;
            else if (!part.PolicyTextReferences.Contains("{All}"))
                return part.PolicyTextReferences;
            else
                return null;
        }
        public IEnumerable<PolicyTextInfoPart> GetPolicies(string culture = null, int[] ids = null) {
            var siteLanguage = _workContext.GetContext().CurrentSite.SiteCulture;

            int currentLanguageId;
            IList<PolicyForUserViewModel> model = new List<PolicyForUserViewModel>();
            IContentQuery<PolicyTextInfoPart> query;

            if (!String.IsNullOrWhiteSpace(culture)) {
                currentLanguageId = _cultureManager.GetCultureByName(culture).Id;
            } else {
                //Nel caso di contenuto senza Localizationpart prendo la CurrentCulture
                currentLanguageId = _cultureManager.GetCultureByName(_workContext.GetContext().CurrentCulture).Id;
            }

            if (ids != null) {
                query = _contentManager.Query<PolicyTextInfoPart, PolicyTextInfoPartRecord>()
                           .Where(x => ids.Contains(x.Id))
                           .OrderByDescending(o => o.Priority)
                           .Join<LocalizationPartRecord>()
                           .Where(w => w.CultureId == currentLanguageId || (w.CultureId == 0 && (siteLanguage.Equals(culture) || culture == null)))
                           .ForVersion(VersionOptions.Published);
            } else {
                query = _contentManager.Query<PolicyTextInfoPart, PolicyTextInfoPartRecord>()
                           .OrderByDescending(o => o.Priority)
                           .Join<LocalizationPartRecord>()
                           .Where(w => w.CultureId == currentLanguageId || (w.CultureId == 0 && (siteLanguage.Equals(culture) || culture == null)))
                           .ForVersion(VersionOptions.Published);
            }

            return query.List<PolicyTextInfoPart>();
        }

        private IList<PolicyForUserViewModel> GetCookieAnswers() {
            HttpCookie cookie = _workContext.GetContext().HttpContext.Request.Cookies["PoliciesAnswers"];
            if (cookie != null && cookie.Value != null)
                return new JavaScriptSerializer().Deserialize<List<PolicyForUserViewModel>>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie.Value)));
            else
                return new List<PolicyForUserViewModel>();
        }
    }
}
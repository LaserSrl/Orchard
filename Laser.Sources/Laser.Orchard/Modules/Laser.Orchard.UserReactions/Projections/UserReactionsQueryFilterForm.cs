using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.UserReactions.Projections {
    public class UserReactionsQueryFilterForm : IFormProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly IUserReactionsService _reactionsService;
        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }

        public UserReactionsQueryFilterForm(IShapeFactory shapeFactory, IOrchardServices orchardServices, IUserReactionsService reactionsService) {
            _shapeFactory = shapeFactory;
            _orchardServices = orchardServices;
            _reactionsService = reactionsService;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            context.Form("ReactionsFilterForm", shape => {
                var f = _shapeFactory.Form(
                    Id: "ReactionsFilterForm",
                    _Reaction: _shapeFactory.SelectList(
                                Id: "reaction", Name: "Reaction",
                                Title: T("Reaction"),
                                Size: 1,
                                Multiple: false
                                ),
                    _Operator: _shapeFactory.SelectList(
                        Id: "operator", Name: "Operator",
                        Title: T("Operator"),
                        Size: 1,
                        Multiple: false
                        ),
                    _FieldSetSingle: _shapeFactory.FieldSet(
                        Id: "fieldset-single",
                        _Value: _shapeFactory.TextBox(
                            Id: "value", Name: "Value",
                            Title: T("Value"),
                            Classes: new[] { "tokenized" }
                            )
                        ),
                    _FieldSetMin: _shapeFactory.FieldSet(
                        Id: "fieldset-min",
                        _Min: _shapeFactory.TextBox(
                            Id: "min", Name: "Min",
                            Title: T("Min"),
                            Classes: new[] { "tokenized" }
                            )
                        ),

                    _FieldSetMax: _shapeFactory.FieldSet(
                        Id: "fieldset-max",
                        _Max: _shapeFactory.TextBox(
                            Id: "max", Name: "Max",
                            Title: T("Max"),
                            Classes: new[] { "tokenized" }
                            )
                        )
                );
                Dictionary<string, string> reactionType = GetTypesTable();

                foreach (var item in reactionType) {
                    f._Reaction.Add(new SelectListItem { Value = item.Key.ToString(), Text = item.Value.ToString() });
                }

                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.Equals), Text = T("Is equal to").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.NotEquals), Text = T("Is not equal to").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.Between), Text = T("Is between to").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.GreaterThan), Text = T("Is greater than").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.GreaterThanEquals), Text = T("Is greater than or equal").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.LessThan), Text = T("Is less than").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.LessThanEquals), Text = T("Is less than or equal").Text });
                f._Operator.Add(new SelectListItem { Value = Convert.ToString(UserReactionsFieldOperator.NotBetween), Text = T("Not between").Text });
                return f;
            });
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="formState"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Action<IHqlExpressionFactory> GetFilterPredicate(dynamic formState, string property) {

            var op = (UserReactionsFieldOperator)Enum.Parse(typeof(UserReactionsFieldOperator), Convert.ToString(formState.Operator));

            decimal min, max;

            if (op == UserReactionsFieldOperator.Between || op == UserReactionsFieldOperator.NotBetween) {
                min = Decimal.Parse(Convert.ToString(formState.Min), CultureInfo.InvariantCulture);
                max = Decimal.Parse(Convert.ToString(formState.Max), CultureInfo.InvariantCulture);
            }
            else {
                min = max = Decimal.Parse(Convert.ToString(formState.Value), CultureInfo.InvariantCulture);
            }

            switch (op) {
                case UserReactionsFieldOperator.LessThan:
                    return x => x.Lt(property, max);
                case UserReactionsFieldOperator.LessThanEquals:
                    return x => x.Le(property, max);
                case UserReactionsFieldOperator.Equals:
                    if (min == max) {
                        return x => x.Eq(property, min);
                    }
                    return y => y.And(x => x.Ge(property, min), x => x.Le(property, max));
                case UserReactionsFieldOperator.NotEquals:
                    return min == max ? (Action<IHqlExpressionFactory>)(x => x.Not(y => y.Eq(property, min))) : (y => y.Or(x => x.Lt(property, min), x => x.Gt(property, max)));
                case UserReactionsFieldOperator.GreaterThan:
                    return x => x.Gt(property, min);
                case UserReactionsFieldOperator.GreaterThanEquals:
                    return x => x.Ge(property, min);
                case UserReactionsFieldOperator.Between:
                    return y => y.And(x => x.Ge(property, min), x => x.Le(property, max));
                case UserReactionsFieldOperator.NotBetween:
                    return y => y.Or(x => x.Lt(property, min), x => x.Gt(property, max));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="formState"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static LocalizedString DisplayFilter(string fieldName, dynamic formState, Localizer T) {
            var op = (UserReactionsFieldOperator)Enum.Parse(typeof(UserReactionsFieldOperator), Convert.ToString(formState.Operator));
            string value = Convert.ToString(formState.Value);
            string min = Convert.ToString(formState.Min);
            string max = Convert.ToString(formState.Max);

            switch (op) {
                case UserReactionsFieldOperator.LessThan:
                    return T("{0} is less than {1}", fieldName, value);
                case UserReactionsFieldOperator.LessThanEquals:
                    return T("{0} is less or equal than {1}", fieldName, value);
                case UserReactionsFieldOperator.Equals:
                    return T("{0} equals {1}", fieldName, value);
                case UserReactionsFieldOperator.NotEquals:
                    return T("{0} is not equal to {1}", fieldName, value);
                case UserReactionsFieldOperator.GreaterThan:
                    return T("{0} is greater than {1}", fieldName, value);
                case UserReactionsFieldOperator.GreaterThanEquals:
                    return T("{0} is greater or equal than {1}", fieldName, value);
                case UserReactionsFieldOperator.Between:
                    return T("{0} is between {1} and {2}", fieldName, min, max);
                case UserReactionsFieldOperator.NotBetween:
                    return T("{0} is not between {1} and {2}", fieldName, min, max);
            }

            // should never be hit, but fail safe
            return new LocalizedString(fieldName);
        }


        //Loading UserReactions
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetTypesTable() {

            Dictionary<string, string> retVal = new Dictionary<string, string>();
            var reactionSettings = _orchardServices.WorkContext.CurrentSite.As<UserReactionsSettingsPart>();
            var userRT = new UserReactionsTypes();

            userRT.UserReactionsType = _reactionsService.GetTypesTable().Select(r => new UserReactionsTypeVM {
                Id = r.Id,
                TypeName = r.TypeName,

            }).ToList();

            foreach (UserReactionsTypeVM retval in userRT.UserReactionsType) {
                retVal.Add(retval.Id.ToString(), retval.TypeName);
            }

            return retVal;
        }
    }
}
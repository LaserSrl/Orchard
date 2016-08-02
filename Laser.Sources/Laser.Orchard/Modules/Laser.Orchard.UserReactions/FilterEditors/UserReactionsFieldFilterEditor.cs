using Orchard.ContentManagement;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Projections.FieldTypeEditors;
using Orchard.Projections.FilterEditors;
using Orchard.Projections.Models;



namespace Laser.Orchard.UserReactions.FilterEditors {
    public class UserReactionsFieldFilterEditor : IFilterEditor {

        public Localizer T { get; set; }

        public UserReactionsFieldFilterEditor() {
            T = NullLocalizer.Instance;
        }

        public bool CanHandle(Type type) {
            return new[] {
                typeof(char),
                typeof(string),
            }.Contains(type);
        }

        public string FormName {
            get { return UserReactionsFieldFilterForm.FormName; }
        }

        public Action<IHqlExpressionFactory> Filter(string property, dynamic formState) {
            return UserReactionsFieldFilterForm.GetFilterPredicate(formState, property);
        }

        public LocalizedString Display(string property, dynamic formState) {
            return UserReactionsFieldFilterForm.DisplayFilter(property, formState, T);
        }
    }

    
}
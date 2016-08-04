using Laser.Orchard.UserReactions.Drivers;
using Laser.Orchard.UserReactions.FilterEditors;
using Laser.Orchard.UserReactions.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.FieldTypeEditors;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace Laser.Orchard.UserReactions.Projection {
    public class UserReactionsFieldFilter : IFilterProvider {

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly UserReactionsFieldDriver _driver;
        private readonly UserReactionsFieldFilterEditor _fieldTypeEditor;
        public Localizer T { get; set; }

        public UserReactionsFieldFilter(IContentDefinitionManager contentDefinitionManager,
                                        IEnumerable<IFieldTypeEditor> fieldTypeEditors) {

            _contentDefinitionManager = contentDefinitionManager;
            _fieldTypeEditor = (UserReactionsFieldFilterEditor)fieldTypeEditors.Where(x => x.FormName == UserReactionsFieldFilterForm.FormName).SingleOrDefault();
            T = NullLocalizer.Instance;
        }



        public void Describe(DescribeFilterContext describe) {
            //-------------------------------------------------------------------------------
            //Enabling this method causes the fact thatHiddneStringfields show up twicein the list of available filter,
            //because also the default Orchard.Projection methods process them. This has been implemented anyway to be
            //able to add further predicates to the HiddenStringField. Specifically, we implemented IsEmpty and IsNotEmpty
            //predicates. This may be enabled by commeting this out in case we want to add further filters that cannot be
            //replicated by combinations of the standard ones.
            //-------------------------------------------------------------------------------

            //describe.For(
            //    "Content",          // The category of this filter
            //    T("Content"),       // The name of the filter (not used in 1.4)
            //    T("Content"))       // The description of the filter (not used in 1.4)

            //    // Defines the actual filter (we could define multiple filters using the fluent syntax)
            //    .Element(
            //        "UserReactionsParts",     // Type of the element
            //        T("UserReactions Parts"), // Name of the element
            //        T("UserReactions Parts"), // Description of the element
            //        ApplyFilter,        // Delegate to a method that performs the actual filtering for this element
            //        DisplayFilter,       // Delegate to a method that returns a descriptive string for this element
            //        form: "UserReactionsFieldFilter"
            //    );


            foreach (var part in _contentDefinitionManager.ListPartDefinitions()) {

                var descriptor = describe
                    .For(part.Name + "ContentFields",
                        T("{0} Content Fields", part.Name),
                        T("Content Fields for {0}", part.Name));

               // foreach (var partName in part.Where(fi => fi. == "UserReactionsField")) {
                    //var localField = field;
                    var localPart = part;

                    var membersContext = new DescribeMembersContext(
                        (storageName, storageType, displayName, description) => {
                           // IFieldTypeEditor ftEditor = (IFieldTypeEditor)_fieldTypeEditor;

                            descriptor.Element(
                                type: "UserReactionsParts",
                                name: T("UserReactions Parts"),
                                description: T("UserReactions Parts"),
                                filter: context => ApplyFilter(context),
                                display: DisplayFilter,
                                form: "UserReactionsFieldFilter");
                        });
                    ((IContentFieldDriver)_driver).Describe(membersContext);
                //}
            }

        }

        
        public void ApplyFilter
            (FilterContext context) {

            //var propertyName = String.Join(".", part.Name, field.Name, storageName ?? "");

            //// use an alias with the join so that two filters on the same Field Type wont collide
            //var relationship = _fieldTypeEditor.GetFilterRelationship(ToSafeName(propertyName));

            //// generate the predicate based on the editor which has been used
            //Action<IHqlExpressionFactory> predicate = _fieldTypeEditor.GetFilterPredicate(context.State);

            //// combines the predicate with a filter on the specific property name of the storage, as implemented in FieldIndexService
            //Action<IHqlExpressionFactory> andPredicate = x => x.And(y => y.Eq("PropertyName", propertyName), predicate);

            //// apply where clause
            //context.Query = context.Query.Where(relationship, andPredicate);
            context.Query = context.Query.Join(x => x.ContentPartRecord(typeof(UserReactionsPartRecord)));

        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Content with UserReactionsPart");

        }

        ///String methods taken from orchard
        ///
        /// <summary>
        /// Generates a valid technical name.
        /// </summary>
        /// <remarks>
        /// Uses a white list set of chars.
        /// </remarks>
        //private static string ToSafeName(string name) {
        //    if (String.IsNullOrWhiteSpace(name))
        //        return String.Empty;

        //    name = RemoveDiacritics(name);
        //    name = Strip(name, c =>
        //        !IsLetter(c)
        //        && !Char.IsDigit(c)
        //        );

        //    name = name.Trim();

        //    // don't allow non A-Z chars as first letter, as they are not allowed in prefixes
        //    while (name.Length > 0 && !IsLetter(name[0])) {
        //        name = name.Substring(1);
        //    }

        //    if (name.Length > 128)
        //        name = name.Substring(0, 128);

        //    return name;
        //}

        //private static string RemoveDiacritics(string name) {
        //    string stFormD = name.Normalize(NormalizationForm.FormD);
        //    var sb = new StringBuilder();

        //    foreach (char t in stFormD) {
        //        UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(t);
        //        if (uc != UnicodeCategory.NonSpacingMark) {
        //            sb.Append(t);
        //        }
        //    }

        //    return (sb.ToString().Normalize(NormalizationForm.FormC));
        //}

        //private static string Strip(string subject, Func<char, bool> predicate) {

        //    var result = new char[subject.Length];

        //    var cursor = 0;
        //    for (var i = 0; i < subject.Length; i++) {
        //        char current = subject[i];
        //        if (!predicate(current)) {
        //            result[cursor++] = current;
        //        }
        //    }

        //    return new string(result, 0, cursor);
        //}

        ///// <summary>
        ///// Whether the char is a letter between A and Z or not
        ///// </summary>
        //private static bool IsLetter(char c) {
        //    return ('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z');
        //}

        //private static bool IsSpace(char c) {
        //    return (c == '\r' || c == '\n' || c == '\t' || c == '\f' || c == ' ');
        //}
    }
}
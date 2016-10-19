using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Projections {
    public class ReactionClickedFilterForm : IFormProvider {
        private readonly IUserReactionsService _reactionsService;
        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }
        public const string FormName = "ReactionClickedFilterForm";

        public ReactionClickedFilterForm(IUserReactionsService reactionsService, IShapeFactory shapeFactory) {
            _reactionsService = reactionsService;
            _shapeFactory = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            context.Form("ReactionClickedFilterForm", shape => {
                var f = _shapeFactory.Form(
                    Id: "ReactionClickedFilterForm",

                     _Reaction: _shapeFactory.FieldSet(
                            Id: "reaction",
                            _Reaction: _shapeFactory.TextBox(
                            Name: "Reaction",
                            Title: T("Reaction type"),
                            Classes: new[] { "tokenized" }
                            )
                        ),

                    _ReactionTitle: _shapeFactory.Markup(
                        Value: "<fieldset><legend>Reactions list available:</legend>"),

                    _ReactionsList: _shapeFactory.List(
                        Id: "reactionslist"
                    ),

                     _ReactionPanel: _shapeFactory.Markup(
                        Value: " </fieldset>"),

                    _FieldSetSingle: _shapeFactory.FieldSet(
                        Id: "fieldset-content-item",
                        _Value: _shapeFactory.TextBox(
                            Id: "contentId", Name: "ContentId",
                            Title: T("Content ID"),
                            Classes: new[] { "tokenized" }
                            )
                        )
                );

                var reactionTypes = _reactionsService.GetTypesTableFiltered();
                foreach (var item in reactionTypes) {
                    f._ReactionsList.Add(item.TypeName); //, item.Id.ToString());
                }
                return f;
            });
        }
    }
}
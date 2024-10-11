using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using Orchard.Localization;
using System;
using System.Collections.Generic;

namespace Orchard.Fields.Drivers {
    public class LinkFieldDriver : ContentFieldDriver<LinkField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Link.Edit";

        public LinkFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(ContentField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, LinkField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_Link", GetDifferentiator(field, part), () => {
                var settings = field.PartFieldDefinition.Settings.GetModel<LinkFieldSettings>();
                return shapeHelper.Fields_Link().Settings(settings);
            });
        }

        protected override DriverResult Editor(ContentPart part, LinkField field, dynamic shapeHelper) {
            return ContentShape("Fields_Link_Edit", GetDifferentiator(field, part),
                () => {
                    if (part.IsNew()) {
                        var settings = field.PartFieldDefinition.Settings.GetModel<LinkFieldSettings>();
                        if (String.IsNullOrEmpty(field.Value)) {
                            field.Value = settings.DefaultValue;
                        }
                        if (String.IsNullOrEmpty(field.Text)) {
                            field.Text = settings.TextDefaultValue;
                        }
                    }
                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: field, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, LinkField field, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(field, GetPrefix(field, part), null, null)) {
                var settings = field.PartFieldDefinition.Settings.GetModel<LinkFieldSettings>();

                if (settings.Required && String.IsNullOrWhiteSpace(field.Value)) {
                    updater.AddModelError(GetPrefix(field, part), T("Url is required for {0}", T(field.DisplayName)));
                }
                else if (settings.LinkTextMode == LinkTextMode.Required && String.IsNullOrWhiteSpace(field.Text)) {
                    updater.AddModelError(GetPrefix(field, part), T("Text is required for {0}.", T(field.DisplayName)));
                } else if (!String.IsNullOrWhiteSpace(field.Value)) {
                    // Check if it's a valid uri, considering that there may be the link to an anchor only
                    // e.g.: field.Value = "#divId"
                    // Take everything before the first "#" character and check if it's a valid uri.
                    // If there is no character before the first "#", consider the value as a valid one (because it is a reference to a div inside the same page)
                    var index = field.Value.IndexOf('#');
                    if (index >= 0) {
                        var url = field.Value.Substring(0, index);
                        if (!String.IsNullOrWhiteSpace(url) && !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) {
                            updater.AddModelError(GetPrefix(field, part), T("{0} is an invalid url.", field.Value));
                        } else {
                            // The first part of the value is valid (or empty)
                            // In the same way, check that the second part of the value (after the "#" character) is valid
                            // For html 5, a tag id is valid as long as it doesn't contain white spaces
                            var anchor = field.Value.Substring(index + 1);
                            if (anchor.IndexOf(' ') >= 0) {
                                updater.AddModelError(GetPrefix(field, part), T("{0} is an invalid url.", field.Value));
                            }
                        }
                    } else if (!Uri.IsWellFormedUriString(field.Value, UriKind.RelativeOrAbsolute)) {
                        updater.AddModelError(GetPrefix(field, part), T("{0} is an invalid url.", field.Value));
                    }
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, LinkField field, ImportContentContext context) {
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Text", v => field.Text = v);
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Url", v => field.Value = v);
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Target", v => field.Target = v);
        }

        protected override void Exporting(ContentPart part, LinkField field, ExportContentContext context) {
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Text", field.Text);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Url", field.Value);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Target", field.Target);
        }


        protected override void Describe(DescribeMembersContext context) {
            context
                .Member("Text", typeof(string), T("Text"), T("The text of the link."))
                .Member(null, typeof(string), T("Url"), T("The url of the link."))
                .Enumerate<LinkField>(() => field => new[] { field.Value });
        }
    }
}

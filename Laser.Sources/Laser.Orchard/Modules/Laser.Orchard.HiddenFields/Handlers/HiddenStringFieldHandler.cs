using Orchard.ContentManagement;
using Orchard;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Settings;
using Orchard.Tokens;

namespace Laser.Orchard.HiddenFields.Handlers {
    public class HiddenStringFieldHandler : ContentHandler {

        private readonly ITokenizer _tokenizer;

        public HiddenStringFieldHandler(ITokenizer tokenizer) {

            _tokenizer = tokenizer;
            

            OnUpdated<ContentPart>((context, part) => {
                List<HiddenStringField> fields = new List<HiddenStringField>();
                foreach (var pa in context.ContentItem.Parts) {
                    fields.AddRange(pa.Fields.Where(f => f.FieldDefinition.Name == "HiddenStringField").Select(f => (HiddenStringField)f));
                }
                if (fields.Count > 0) {
                    var tokens = new Dictionary<string, object> { { "Content", context.ContentItem } };
                    foreach (var fi in fields) {
                        if (string.IsNullOrEmpty(((HiddenStringField)fi).Value)) {
                            var settings = fi.PartFieldDefinition.Settings.GetModel<HiddenStringFieldSettings>();
                            if (settings.Tokenized) {
                                fi.Value = _tokenizer.Replace(
                                    settings.TemplateString,
                                    tokens);
                            } else {
                                fi.Value = settings.TemplateString;
                            }
                            
                            //((HiddenStringField)fi).Value = ((((HiddenStringField)fi).PartFieldDefinition.Settings).GetModel<HiddenStringFieldSettings>()).TemplateString;
                            //TODO replace tokens
                        }
                    }
                }
            });

            OnCreated<ContentPart>((context, part) => {
                List<HiddenStringField> fields = new List<HiddenStringField>();
                foreach (var pa in context.ContentItem.Parts) {
                    fields.AddRange(pa.Fields.Where(f => f.FieldDefinition.Name == "HiddenStringField").Select(f => (HiddenStringField)f));
                }
                if (fields.Count > 0) {
                    var tokens = new Dictionary<string, object> { { "Content", context.ContentItem } };
                    foreach (var fi in fields) {
                        if (string.IsNullOrEmpty(((HiddenStringField)fi).Value)) {
                            var settings = fi.PartFieldDefinition.Settings.GetModel<HiddenStringFieldSettings>();
                            if (settings.Tokenized) {
                                fi.Value = _tokenizer.Replace(
                                    settings.TemplateString,
                                    tokens);
                            } else {
                                fi.Value = settings.TemplateString;
                            }
                            //TODO replace tokens

                        }
                    }
                }
               
            });
        }
    }
}
using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Fields.Fields {
    public class EnumerationField : ContentField {
        private const char Separator = ';';

        public string Value {
            get => Storage.Get<string>()?.Trim(Separator) ?? "";
            set => Storage.Set(string.IsNullOrWhiteSpace(value)
                ? string.Empty
                // It is now the responsibility of this field to (re-)add the separators.
                : Separator + value.Trim(Separator) + Separator);
        }

        public string[] SelectedValues {
            get => Value?.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            set => Value = value?.Length > 0 ? string.Join(Separator.ToString(), value) : "";
        }
    }
}

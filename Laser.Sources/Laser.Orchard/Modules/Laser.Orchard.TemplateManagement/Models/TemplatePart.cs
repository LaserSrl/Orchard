using Laser.Orchard.TemplateManagement.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using Orchard.Data.Conventions;

namespace Laser.Orchard.TemplateManagement.Models {
    public class TemplatePart : ContentPart<TemplatePartRecord>, ITitleAspect {
        internal LazyField<TemplatePart> LayoutField = new LazyField<TemplatePart>();

        public string Title {
            get { return Record.Title; }
            set { Record.Title = value; }
        }

        public string Subject {
            get { return Record.Subject; }
            set { Record.Subject = value; }
        }

        public string Text {
            get { return Record.Text; }
            set { Record.Text = value; }
        }

        public bool IsLayout {
            get { return Record.IsLayout; }
            set { Record.IsLayout = value; }
        }

        public TemplatePart Layout {
            get { return LayoutField.Value; }
            set { LayoutField.Value = value; }
        }

        public string DefaultParserIdSelected {
            get { return Settings.GetModel<TemplatePartSettings>().DefaultParserIdSelected; }
        }
    }

    public class TemplatePartRecord : ContentPartRecord {
        public virtual string Title { get; set; }
        public virtual string Subject { get; set; }

        [StringLengthMax]
        public virtual string Text { get; set; }
        public virtual int? LayoutIdSelected { get; set; }
        public virtual bool IsLayout { get; set; }
    }
}
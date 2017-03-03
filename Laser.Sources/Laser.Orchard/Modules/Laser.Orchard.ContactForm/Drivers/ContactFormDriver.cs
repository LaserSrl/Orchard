using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Laser.Orchard.ContactForm.Models;
using Laser.Orchard.ContactForm.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;

namespace Laser.Orchard.ContactForm.Drivers {
    public class ContactFormDriver : ContentPartCloningDriver<ContactFormPart> {

        private readonly IUtilsServices _utilsServices;
        private readonly IStorageProvider _storageProvider;
        private readonly INotifier _notifier;

        public Localizer T { get; set; }

        public ContactFormDriver(IUtilsServices utilsServices, INotifier notifier, IStorageProvider storageProvider) {
            _storageProvider = storageProvider;
            _utilsServices = utilsServices;
            _notifier = notifier;
        }

        /// <summary>
        /// Defines the shapes required for the part's main view.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="displayType">The display type.</param>
        /// <param name="shapeHelper">The shape helper.</param>
        protected override DriverResult Display(ContactFormPart part, string displayType, dynamic shapeHelper) {

            var viewModel = new ContactFormViewModel();
            if (part != null && displayType.Contains("Detail")) {
                viewModel.ContentRecordId = part.Record.Id;
                viewModel.ShowSubjectField = !part.UseStaticSubject;
                viewModel.ShowNameField = part.DisplayNameField;
                viewModel.RequireNameField = part.RequireNameField;
                viewModel.EnableFileUpload = part.EnableUpload;
            }
            return ContentShape("Parts_ContactForm",
                () => shapeHelper.Parts_ContactForm(
                    ContactForm: viewModel
                    ));
        }

        /// <summary>
        /// Defines the shapes required for the editor view.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="shapeHelper">The shape helper.</param>
        protected override DriverResult Editor(ContactFormPart part, dynamic shapeHelper) {
            if (part == null)
                part = new ContactFormPart();

            var editModel = new ContactFormEditModel
            {
                BasePath = _utilsServices.VirtualMediaPath,
                ContactForm = part
            };

            return ContentShape("Parts_ContactForm_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContactForm", Model: editModel, Prefix: Prefix));
        }

        /// <summary>
        /// Runs upon the POST of the editor view.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="updater">The updater.</param>
        /// <param name="shapeHelper">The shape helper.</param>
        protected override DriverResult Editor(ContactFormPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (part == null || updater == null)
                return Editor(null, shapeHelper);

            var editModel = new ContactFormEditModel {
                BasePath = _utilsServices.VirtualMediaPath,
                ContactForm = part
            };

            if (updater.TryUpdateModel(editModel, Prefix, null, null)) {
                if (!editModel.ContactForm.DisplayNameField) {
                    editModel.ContactForm.RequireNameField = false;
                }
                if (!string.IsNullOrWhiteSpace(editModel.ContactForm.PathUpload))
                {
                    if (!_storageProvider.FolderExists(editModel.ContactForm.PathUpload))
                    {
                        if (_storageProvider.TryCreateFolder(editModel.ContactForm.PathUpload))
                            _notifier.Information(T("The destination folder for the uploaded files has been succesfully created!"));
                        else
                            _notifier.Error(T("The destination folder for the uploaded files has not been created!"));
                    }
                }
            }

            return Editor(editModel.ContactForm, shapeHelper);
        }

        #region [ Import/Export ]
        protected override void Exporting(ContactFormPart part, ExportContentContext context) {

            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("AttachFiles", part.AttachFiles);
            root.SetAttributeValue("DisplayNameField", part.DisplayNameField);
            root.SetAttributeValue("EnableUpload", part.EnableUpload);
            root.SetAttributeValue("PathUpload", part.PathUpload);
            root.SetAttributeValue("RecipientEmailAddress", part.RecipientEmailAddress);
            root.SetAttributeValue("RequireNameField", part.RequireNameField);
            root.SetAttributeValue("StaticSubjectMessage", part.StaticSubjectMessage);
            root.SetAttributeValue("TemplateRecord_Id", -1); //Ovvero nessun template
            root.SetAttributeValue("UseStaticSubject", part.UseStaticSubject);
        }

        protected override void Importing(ContactFormPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            part.AttachFiles = bool.Parse(root.Attribute("AttachFiles").Value);
            part.DisplayNameField = bool.Parse(root.Attribute("DisplayNameField").Value);
            part.EnableUpload = bool.Parse(root.Attribute("EnableUpload").Value);
            part.PathUpload = root.Attribute("PathUpload").Value;
            part.RecipientEmailAddress = root.Attribute("RecipientEmailAddress").Value;
            part.RequireNameField = bool.Parse(root.Attribute("RequireNameField").Value);
            part.StaticSubjectMessage = root.Attribute("StaticSubjectMessage").Value;
            part.TemplateRecord_Id = int.Parse(root.Attribute("TemplateRecord_Id").Value);
            part.UseStaticSubject = bool.Parse(root.Attribute("UseStaticSubject").Value);
        }
        #endregion

        protected override void Cloning(ContactFormPart originalPart, ContactFormPart clonePart, CloneContentContext context) {
            clonePart.RecipientEmailAddress = originalPart.RecipientEmailAddress;
            clonePart.StaticSubjectMessage = originalPart.StaticSubjectMessage;
            clonePart.UseStaticSubject = originalPart.UseStaticSubject;
            clonePart.DisplayNameField = originalPart.DisplayNameField;
            clonePart.RequireNameField = originalPart.RequireNameField;
            clonePart.TemplateRecord_Id = originalPart.TemplateRecord_Id;
            clonePart.EnableUpload = originalPart.EnableUpload;
            clonePart.AttachFiles = originalPart.AttachFiles;
            clonePart.PathUpload = originalPart.PathUpload;
            clonePart.RequireAttachment = originalPart.RequireAttachment;
        }
    }
}
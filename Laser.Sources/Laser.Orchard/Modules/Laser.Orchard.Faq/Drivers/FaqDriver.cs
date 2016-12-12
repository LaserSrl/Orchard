using System.Linq;
using Laser.Orchard.Faq.Models;
using Laser.Orchard.Faq.Services;
using Laser.Orchard.Faq.ViewModels;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Localization;

namespace Laser.Orchard.Faq.Drivers
{
    public class FaqDriver : ContentPartDriver<FaqPart>
    {
        private readonly IFaqTypeService _faqTypeService;
        private readonly IContentManager _contentManager;
        private readonly IFaqService _faqService;

        public Localizer T;

        public FaqDriver(IContentManager contentManager, IFaqService faqService, IFaqTypeService faqTypeService)
        {
            _contentManager = contentManager;
            _faqTypeService = faqTypeService;
            _faqService = faqService;
        }

        protected override string Prefix
        {
            get { return "Faq"; }
        }

        protected override DriverResult Display(FaqPart part, string displayType, dynamic shapeHelper)
        {
            var faqType =
                _contentManager.Query<FaqTypePart>(VersionOptions.Published, "FaqType")
                               .Where<FaqTypePartRecord>(t => t.Id == part.FaqTypeId)
                               .List()
                               .FirstOrDefault();

            return ContentShape("Parts_Faq",
                                () => shapeHelper.Parts_Faq(
                                    Question: part.Question,
                                    FaqType: part.FaqTypeId != 0 ? _faqTypeService.GetFaqType(part.FaqTypeId).Title : string.Empty,
                                    Answer: part.ContentItem.As<BodyPart>().Text));
        }


        protected override DriverResult Editor(FaqPart part, dynamic shapeHelper)
        {
            var temp = ContentShape("Parts_Faq_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/Faq",
                                    Model: BuildEditorViewModel(part),
                                    Prefix: Prefix));
            return temp;
        }

        //POST
        protected override DriverResult Editor(FaqPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new EditFaqViewModel();
            if (updater.TryUpdateModel(model, Prefix, null, null))
            {
                if (string.IsNullOrWhiteSpace(model.Question))
                {
                    updater.AddModelError(Prefix, T("Error"));
                }
            }
            if (part.ContentItem.Id != 0)
            {
                _faqService.UpdateFaqForContentItem(part.ContentItem, model);
            }
            
            return Editor(part, shapeHelper);
        }


        protected override void Exporting(FaqPart part, ExportContentContext context)
        {
           
            //12-12-2016
            var root = context.Element(part.PartDefinition.Name);
           
            if (part.FaqTypeId > 0) {
                
                ////cerco il corrispondente valore dell' identity dalla parts del template e lo associo al campo 
                var contItemTempl = _contentManager.Get(part.FaqTypeId);
                if (contItemTempl != null) {
                    root.SetAttributeValue("FaqTypeId", _contentManager.GetItemMetadata(contItemTempl).Identity.ToString());
                }
             
            }
            ////////////////////////////////////////////////////////   
            
            context.Element(part.PartDefinition.Name).SetAttributeValue("Question", part.Question);
        }



        protected override void Importing(FaqPart part, ImportContentContext context)
        {
            part.Question = context.Attribute(part.PartDefinition.Name, "Question") ?? string.Empty;

            //////12-12-2016
            context.ImportAttribute(part.PartDefinition.Name, "FaqTypeId", x => {
                var tempPartFromid = context.GetItemFromSession(x);

                if (tempPartFromid != null && tempPartFromid.Is<FaqTypePart>()) {
                    //associa id faq
                    part.FaqTypeId = tempPartFromid.As<FaqTypePart>().Id;
                }
            });

           
        }


        private EditFaqViewModel BuildEditorViewModel(FaqPart part)
        {
            var avm = new EditFaqViewModel
            {
                Question = part.Question,
                FaqTypes = _faqTypeService.GetFaqTypes(false)
            };

            if (part.FaqTypeId > 0)
            {
                avm.FaqType = part.FaqTypeId;
            }

            return avm;
        }
    }
}

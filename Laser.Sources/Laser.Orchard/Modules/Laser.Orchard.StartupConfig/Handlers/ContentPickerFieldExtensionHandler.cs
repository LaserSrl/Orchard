using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.Settings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Handlers
{
    public class ContentPickerFieldExtensionHandler : ContentHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        public ContentPickerFieldExtensionHandler(IContentManager contentManager, IControllerContextAccessor controllerContextAccessor,
                                                  ILocalizationService localizationService, IOrchardServices orchardServices, INotifier notifier)
        {
            _contentManager = contentManager;
            _controllerContextAccessor = controllerContextAccessor;
            _localizationService = localizationService;
            _notifier = notifier;
            _orchardServices = orchardServices;

            OnPublishing<CommonPart>((context, part) => CascadePublish(context.ContentItem));
            OnUpdated<CommonPart>((context, part) => TranslatePickedContents(context.ContentItem));

            T = NullLocalizer.Instance;
        }

        private void TranslatePickedContents(ContentItem contentItem)
        {
            if (contentItem.Has<LocalizationPart>())
            {
                string language = null;
                bool translatingTerm = _controllerContextAccessor.Context.RouteData.Values["action"].ToString() == "Translate" && _controllerContextAccessor.Context.Controller.GetType().FullName == "Orchard.Localization.Controllers.AdminController";

                if (translatingTerm)
                    language = System.Web.HttpContext.Current.Request.Form["SelectedCulture"];
                else
                    language = contentItem.As<LocalizationPart>().Culture == null ? null : contentItem.As<LocalizationPart>().Culture.Culture;

                if (language != null)
                {
                    var fields = contentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(ContentPickerField).Name))
                                                  .Cast<ContentPickerField>();
                    foreach (ContentPickerField field in fields)
                    {
                        var settings = field.PartFieldDefinition.Settings.GetModel<ContentPickerFieldExtensionSettings>();
                        if (settings.TranslateContents)
                        {
                            bool itemsTranslated = false;
                            List<int> translatedContents = new List<int>();

                            var pickedContents = field.Ids.ToList();
                            foreach (int pickedContentId in pickedContents)
                            {
                                ContentItem content = _contentManager.Get(pickedContentId);
                                if (content != null) {
                                    if (content.Has<LocalizationPart>()) {
                                        var masterContent = content.As<LocalizationPart>().MasterContentItem == null ? content : content.As<LocalizationPart>().MasterContentItem;

                                        var localizedContent = _localizationService.GetLocalizedContentItem(masterContent, language);
                                        if (localizedContent == null
                                            && content.As<LocalizationPart>().Culture != contentItem.As<LocalizationPart>().Culture
                                            && content.As<LocalizationPart>().MasterContentItem != null) {
                                            if (masterContent.As<LocalizationPart>().Culture == contentItem.As<LocalizationPart>().Culture)
                                                localizedContent = masterContent.As<LocalizationPart>();
                                        }

                                        if (localizedContent != null) {
                                            if (!translatedContents.Contains(localizedContent.Id)) {
                                                itemsTranslated = true;
                                                translatedContents.Add(localizedContent.Id);
                                            }
                                        }
                                        else if (!translatedContents.Contains(content.Id))
                                            translatedContents.Add(content.Id);
                                    }
                                    else
                                        translatedContents.Add(content.Id);
                                }
                            }

                            field.Ids = translatedContents.ToArray();

                            if (itemsTranslated)
                                _notifier.Add(NotifyType.Information, T("The items in the Content Picker Field have been changed to their localized version for the language {0}", language));
                        }
                    }
                }
            }
        }

        private void CascadePublish(ContentItem contentItem)
        {
            var fields = contentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(ContentPickerField).Name)).Cast<ContentPickerField>();
            foreach (ContentPickerField field in fields)
            {
                var settings = field.PartFieldDefinition.Settings.GetModel<ContentPickerFieldExtensionSettings>();
                if (settings.CascadePublish)
                {
                    foreach (Int32 id in field.Ids)
                    {
                        ContentItem contentlinked = _orchardServices.ContentManager.Get(id, VersionOptions.Published);
                        if (contentlinked == null)
                        {
                            contentlinked = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
                            _orchardServices.ContentManager.Publish(contentlinked);
                            _notifier.Add(NotifyType.Information, (T("Linked {0} has been published", contentlinked.ContentType)));
                        }
                    }
                }
            }
        }
    }
}
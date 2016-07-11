using Laser.Orchard.Mobile.Services;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Localization.Services;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.WorkFlows {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushEditForm : IFormProvider {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ICultureManager _cultureManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public PushEditForm(IShapeFactory shapeFactory, IPushNotificationService pushNotificationService, ICultureManager cultureManager) {
            Shape = shapeFactory;
            _pushNotificationService = pushNotificationService;
            T = NullLocalizer.Instance;
            _cultureManager = cultureManager;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "ActionPush",
                        _Type: Shape.FieldSet(
                            Title: T("Send Push"),
                        //_Recipient0: Shape.Radio(
                        //    Id: "dispositivo-all",
                        //    Name: "Device",
                        //    Value: "All",
                        //    Title: T("All"),
                        //    Description: T("All Devices")
                        //),
                        //_Recipient1: Shape.Radio(
                        //    Id: "dispositivo-apple",
                        //    Name: "Device",
                        //    Value: "Apple",
                        //    Title: T("Apple"),
                        //    Description: T("Only Apple Devices")
                        //),
                        //_Recipient2: Shape.Radio(
                        //    Id: "dispositivo-android",
                        //    Name: "Device",
                        //    Value: "Android",
                        //    Title: T("Android"),
                        //    Description: T("Only Android Devices")
                        //),
                        //_Recipient3: Shape.Radio(
                        //    Id: "dispositivo-windowsmobile",
                        //    Name: "Device",
                        //    Value: "WindowsMobile",
                        //    Title: T("Windows Mobile"),
                        //      Description: T("Only Windows Mobile Devices")
                        //),
                        // _Recipient4: Shape.Radio(
                        //    Id: "dispositivo-ContentUser",
                        //    Name: "Device",
                        //    Value: "ContentUser",
                        //    Title: T("Content Owner"),
                        //    Description: T("Devices of the ContentItem's owner")
                        //),
                  _ddlDevice: Shape.SelectList(
                              Id: "allDevice",
                              Name: "allDevice",
                              Title: T("Device"),
                        //     Description: T("select Device"),
                              Size: 7,
                              Multiple: false
                              ),

                             _ddlLanguage: Shape.SelectList(
                              Id: "allLanguage",
                              Name: "allLanguage",
                              Title: T("Language"),
                        // Description: T("select Language"),
                              Size: 4,
                              Multiple: false
                              ),

                                       _RecipientProd: Shape.Radio(
                                Id: "Produzione",
                                Name: "Produzione",
                                Value: "Produzione",
                                Title: T("Produzione")
                        //     Description: T("Produzione")
                            ),
                               _RecipientProd2: Shape.Radio(
                                Id: "Produzione",
                                Name: "Produzione",
                                Value: "Sviluppo",
                                Title: T("Sviluppo")
                        //      Description: T("Sviluppo")
                            ),
                                                            _Recipientidrelated: Shape.Checkbox(
                                Id: "idRelated",
                                Name: "idRelated",
                                Value: "idRelated",
                                Title: T("Link the content as Related Content")
                        //     Description: T("Produzione")
                            ),
                    _Recipient5: Shape.Textbox(
                                Id: "PushMessage",
                                Name: "PushMessage",
                                Title: T("Push Message"),
                                Description: T("Push Message Tokenized."),
                                Classes: new[] { "large", "text", "tokenized" }
                            )


                        )
                          );
                    //               IEnumerable<UserPart> users = _contentManager
                    //.Query<UserPart, UserPartRecord>()
                    //   .Where(x => x.UserName != null)
                    //   .List();

                    f._Type._ddlDevice.Add(new SelectListItem { Value = "All", Text = "All Devices" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "Apple", Text = "All Apple's device" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "Android", Text = "All Android's device" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "WindowsMobile", Text = "All WindowsMobile's device" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "ContentOwner", Text = "Content's Owner" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "ContentCreator", Text = "Content's Creator" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "ContentLastModifier", Text = "Content's LastModifier" });
                    

                    f._Type._ddlLanguage.Add(new SelectListItem { Value = "All", Text = "All Languages" });
                    foreach (string up in _cultureManager.ListCultures()) {
                        f._Type._ddlLanguage.Add(new SelectListItem { Value = up.ToString(), Text = up.ToString() });
                        //            f._ddlOwner.Add(new SelectListItem { Value = userProfiles[i].FirstName, Text = userProfiles[i].LastName });
                    }


                    //  var aaa=  _culturePickerSettingsService.CultureList;


                    //,
                    //_Parts: Shape.SelectList(
                    //    Id: "email-template", Name: "EmailTemplate",
                    //    Title: T("Template"),
                    //    Description: T("A template to format your email message."),
                    //        Size: 1,
                    //        Multiple: false
                    //        )


                    //           var allTemplates = _templateServices.GetTemplates().Where(w => !w.IsLayout);

                    //foreach (var template in allTemplates) {
                    //    f._Parts.Add(new SelectListItem { Value = template.Id.ToString(), Text = template.Title });
                    //}
                    //return f;
                    return f;
                };


            context.Form("ActivityMobileForm", form);
        }
    }

    //public class MailFormsValidator : IFormEventHandler {
    //    public Localizer T { get; set; }

    //    public void Building(BuildingContext context) {
    //    }

    //    public void Built(BuildingContext context) {
    //    }

    //    public void Validating(ValidatingContext context) {
    //        if (context.FormName != "ActivityMobileForm") return;

    //        var recipientFormValue = context.ValueProvider.GetValue("Recipient");
    //        var recipient = recipientFormValue != null ? recipientFormValue.AttemptedValue : String.Empty;

    //        if (recipient == String.Empty) {
    //            context.ModelState.AddModelError("Recipient", T("You must select at least one recipient").Text);
    //        }

    //        if (context.ValueProvider.GetValue("RecipientOther").AttemptedValue == String.Empty && recipient == "other") {
    //            context.ModelState.AddModelError("RecipientOther", T("You must provide an e-mail address").Text);
    //        }
    //    }

    //    public void Validated(ValidatingContext context) {
    //    }
    //}

}
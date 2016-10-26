using Laser.Orchard.ContentExtension.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using Orchard.Localization.Models;
using Orchard.Logging;
using Orchard.MediaLibrary.Fields;
using Orchard.Roles.Services;
using Orchard.Security.Permissions;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Laser.Orchard.ContentExtension.Services {

    public enum Methods { Get, Post, Delete, Publish };

    public interface IContentExtensionService : IDependency {

        Response StoreInspectExpando(ExpandoObject theExpando, ContentItem TheContentItem);

        void StoreInspectExpandoFields(List<ContentPart> listpart, string key, object value, ContentItem theContentItem);

        bool FileAllowed(string filename);

        /// <summary>
        ///
        /// </summary>
        /// <param name="ContentType"></param>
        /// <param name="method">Get,Post,Delete,Publish</param>
        /// <param name="mycontent">Default null, da valorizzare nel caso in cui voglio testare own permission</param>
        /// <returns></returns>
        bool HasPermission(string ContentType, Methods method, IContent mycontent = null);
    }

    public class ContentExtensionService : IContentExtensionService {
        private string[] allowedFiles = new string[] { "jpg", "png", "gif", "doc", "docx", "xls", "xlsx", "pdf", "mov", "mp4", "mpg", "mpeg", "avi", "3gp", "mp4v", "m4v", "m4a", "aac", "jpeg", "bmp", "wmv", "wav", "mp3" };
        private string[] ProtectedPart = new string[] { "commonpart", "autoroutepart", "userrolespart" };
        private readonly IContentManager _contentManager;

        //  private readonly IRepository<StringFieldIndexRecord> _stringFieldIndexRepository;
        //    private readonly IRepository<FieldIndexPartRecord> _fieldIndexRepository;
        private readonly ITaxonomyService _taxonomyService;

        private readonly IUtilsServices _utilsServices;
        private readonly IContentTypePermissionSettingsService _contentTypePermissionSettingsService;
        public ILogger Log { get; set; }
        private readonly IOrchardServices _orchardServices;
        private readonly IRoleService _roleService;

        public ContentExtensionService(IContentManager contentManager,
            ITaxonomyService taxonomyService,
            IUtilsServices utilsServices,
             IContentTypePermissionSettingsService contentTypePermissionSettingsService,
             IOrchardServices orchardServices,
            IRoleService roleService) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
            Log = NullLogger.Instance;
            _utilsServices = utilsServices;
            _contentTypePermissionSettingsService = contentTypePermissionSettingsService;
            _orchardServices = orchardServices;
            _roleService = roleService;
        }

        #region [Content Permission]

        //private static bool HasOwnership(IUser user, IContent content) {
        //    if (user == null || content == null)
        //        return false;

        //    if (HasOwnershipOnContainer(user, content)) {
        //        return true;
        //    }

        //    var common = content.As<ICommonPart>();
        //    if (common == null || common.Owner == null)
        //        return false;

        //    return user.Id == common.Owner.Id;
        //}
        //private static bool HasOwnershipOnContainer(IUser user, IContent content) {
        //    if (user == null || content == null)
        //        return false;

        //    var common = content.As<ICommonPart>();
        //    if (common == null || common.Container == null)
        //        return false;

        //    common = common.Container.As<ICommonPart>();
        //    if (common == null || common.Container == null)
        //        return false;

        //    return user.Id == common.Owner.Id;
        //}

        private Permission GetPermissionByName(string permission) {
            if (!string.IsNullOrEmpty(permission)) {
                var listpermissions = _roleService.GetInstalledPermissions().Values;
                foreach (IEnumerable<Permission> sad in listpermissions) {
                    foreach (Permission perm in sad) {
                        if (perm.Name == permission) {
                            return perm;
                        }
                    }
                }
            }
            return null;
        }

        private bool TestPermission(string permission, IContent mycontent = null) {
            bool testpermission = false;
            if (!string.IsNullOrEmpty(permission)) {
                Permission Permissiontotest = GetPermissionByName(permission);
                if (Permissiontotest != null) {
                    if (mycontent != null)
                        testpermission = _orchardServices.Authorizer.Authorize(Permissiontotest, mycontent);
                    else
                        testpermission = _orchardServices.Authorizer.Authorize(Permissiontotest);
                    //if (testpermission && (permission.Contains("Own") && permission != "SiteOwner")) {
                    //    testpermission = HasOwnership(_orchardServices.WorkContext.CurrentUser, mycontent);
                    //}
                }
            }
            return testpermission;
        }

        public bool HasPermission(string ContentType, Methods method, IContent mycontent = null) {
            bool haspermission = false;
            List<ContentTypePermissionRecord> settings = _contentTypePermissionSettingsService.ReadSettings().ListContPermission.Where(x => x.ContentType == ContentType).ToList();
            if (settings != null && settings.Count > 0) {
                // test if exist one record in permission setting that enable user

                foreach (ContentTypePermissionRecord ctpr in settings) {
                    switch (method) {
                        case Methods.Get:
                            if (TestPermission(ctpr.GetPermission, mycontent))
                                return true;
                            break;

                        case Methods.Post:
                            if (TestPermission(ctpr.PostPermission, mycontent))
                                return true;
                            break;

                        case Methods.Publish:
                            if (TestPermission(ctpr.PublishPermission, mycontent))
                                return true;
                            break;

                        case Methods.Delete:
                            if (TestPermission(ctpr.DeletePermission, mycontent))
                                return true;
                            break;
                    }
                }
            } else {
                // test generic permission for contenttype

                switch (method) {
                    case Methods.Get:
                        return TestPermission("ViewContent", mycontent);
                        break;

                    case Methods.Post:
                        return TestPermission("EditContent", mycontent);
                        break;

                    case Methods.Publish:
                        return TestPermission("PublishContent", mycontent);
                        break;

                    case Methods.Delete:
                        return TestPermission("DeleteContent", mycontent);
                        break;
                }
            }

            return haspermission;
        }

        #endregion [Content Permission]

        public void StoreInspectExpandoFields(List<ContentPart> listpart, string key, object value, ContentItem theContentItem) {
            var fields = listpart.SelectMany(x => x.Fields.Where(f => f.Name == key));
            if (fields != null) {
                var fieldObj = fields.FirstOrDefault();
                if (fieldObj != null) {
                    // provo a registrare il dato in uno dei fields
                    // non posso fare questo
                    //      fieldObj.GetType().GetProperty("Value").SetValue(fieldObj, value, null);
                    // perchè non regge il tipo nullabile
                    string tipofield = fieldObj.GetType().Name;
                    if (tipofield == typeof(EnumerationField).Name) {
                        RegistraValoreEnumerator(fieldObj, "SelectedValues", value);
                    } else
                        if (tipofield == typeof(DateTimeField).Name) {
                            RegistraValore(fieldObj, "DateTime", value);
                        } else {
                            if (tipofield == typeof(ContentPickerField).Name || tipofield == typeof(MediaLibraryPickerField).Name) {
                                RegistraValoreContentPickerField(fieldObj, "Ids", value);
                            } else {
                                if (tipofield == typeof(TaxonomyField).Name) {
                                    var taxobase = _taxonomyService.GetTaxonomyByName(fieldObj.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"]);

                                    List<TaxoVM> second = ConvertToVM((List<dynamic>)value);

                                    List<Int32> ElencoCategorie = second.Select(x => x.Id).ToList();
                                    List<TermPart> ListTermPartToAdd = new List<TermPart>();
                                    if (_taxonomyService.GetTerm(ElencoCategorie.FirstOrDefault()) == null && ElencoCategorie.Count > 0)
                                        throw new Exception("Field " + key + " Taxonomy term with id=" + ElencoCategorie[0].ToString() + " not exist");
                                    else {
                                        // Se l'elenco delle categorie è nullo salta questa parte e aggiorna
                                        if (_taxonomyService.GetTerm(ElencoCategorie.FirstOrDefault()) != null) {
                                            var taxo_sended_user = _taxonomyService.GetTaxonomy(_taxonomyService.GetTerm(ElencoCategorie.FirstOrDefault()).TaxonomyId);

                                            foreach (Int32 idtermine in ElencoCategorie) {
                                                TermPart termine_selezionato = taxo_sended_user.Terms.Where(x => x.Id == idtermine).FirstOrDefault();

                                                #region [ Tassonomia in Lingua ]

                                                if (theContentItem.As<LocalizationPart>() == null || theContentItem.ContentType == "User") { // se il contenuto non ha localization oppure è user salvo il mastercontent del termine
                                                    Int32 idmaster = 0;
                                                    if (termine_selezionato.ContentItem.As<LocalizationPart>() == null) {
                                                        idmaster = termine_selezionato.ContentItem.Id;
                                                    } else if (termine_selezionato.ContentItem.As<LocalizationPart>().MasterContentItem == null)
                                                        idmaster = termine_selezionato.ContentItem.As<LocalizationPart>().Id;
                                                    else
                                                        idmaster = termine_selezionato.ContentItem.As<LocalizationPart>().MasterContentItem.Id;
                                                    TermPart toAdd = taxobase.Terms.Where(x => x.Id == idmaster).FirstOrDefault();
                                                    if (toAdd == null)
                                                        toAdd = taxobase.Terms.Where(x => x.ContentItem.As<LocalizationPart>().MasterContentItem.Id == idmaster).FirstOrDefault();
                                                    ListTermPartToAdd.Add(toAdd);
                                                } else { // se il contenuto ha localization e non è user salvo il termine come mi viene passato
                                                    // TODO: testare pertinenza della lingua Contenuto in italianao=>termine in italiano
                                                    TermPart toAdd = termine_selezionato;
                                                    ListTermPartToAdd.Add(toAdd);
                                                }

                                                #endregion [ Tassonomia in Lingua ]
                                            }
                                        }
                                    }
                                    _taxonomyService.UpdateTerms(theContentItem, ListTermPartToAdd, fieldObj.Name);
                                } else {
                                    RegistraValore(fieldObj, "Value", value);
                                }
                            }
                        }
                } else {
                    //provo a registrare il dato nella proprieta dello current user
                    //RegistraValore(currentUser, key, value);
                    if (!(key.IndexOf(".") > 0) && key.ToLower() != "contenttype" && key.ToLower() != "id" && key.ToLower() != "language") {
                        throw new Exception("Field " + key + " not in contentitem");
                    }
                    //   if (TheContentItem.ContentType == "User")
                    //
                }
            } else {
                //provo a registrare il dato nella proprieta dello current user
                // RegistraValore(currentUser, key, value);
                // currentUser.GetType().GetProperty(key).SetValue(currentUser, value, null);
                if (!(key.IndexOf(".") > 0) && key.ToLower() != "contenttype" && key.ToLower() != "id" && key.ToLower() != "language") {
                    throw new Exception("Field " + key + " not in contentitem");
                }
                //    if (TheContentItem.ContentType == "User")
                //       throw new Exception("Field " + key + " not in user contentitem");
            }
        }

        public Response StoreInspectExpando(ExpandoObject theExpando, ContentItem theContentItem) {
            try {
                foreach (var kvp in theExpando) {
                    string key = kvp.Key.ToString();
                    string valueType = kvp.Value.GetType().Name;
                    object value = kvp.Value;
                    if (kvp.Value is ExpandoObject) {
                        StoreInspectExpando(kvp.Value as ExpandoObject, theContentItem);
                    }

                    // provo a registrare nel profile part altrimenti provo a registrare nel IUser
                    //currentUser.GetType().GetProperty(key).SetValue(currentUser,value, null);

                    StoreInspectExpandoFields(theContentItem.Parts.ToList(), key, value, theContentItem);
                }
            } catch (Exception ex) {
                Log.Error("ContentExtension -> ContentExtensionService -> StoreInspectExpando : " + ex.Message + " <Stack> " + ex.StackTrace);
                return (_utilsServices.GetResponse(ResponseType.None, "Error:" + ex.Message));
            }
            return StoreInspectExpandoPart(theExpando, theContentItem);
        }

        private Response StoreInspectExpandoPart(ExpandoObject theExpando, ContentItem TheContentItem) {
            try {
                foreach (var kvp in theExpando) {
                    string key = kvp.Key.ToString();
                    if (key.IndexOf(".") > 0) {
                        string valueType = kvp.Value.GetType().Name;
                        object value = kvp.Value;
                        StoreLikeDynamic(key, value, TheContentItem);
                    }
                }
            } catch (Exception ex) {
                Log.Error("ContentExtension -> ContentExtensionService -> StoreInspectExpandoPart : " + ex.Message + " <Stack> " + ex.StackTrace);
                return _utilsServices.GetResponse(ResponseType.None, ex.Message);
            }
            return (_utilsServices.GetResponse(ResponseType.Success));
        }

        private void StoreLikeDynamic(string key, object value, ContentItem TheContentItem) {
            string[] ListProperty = key.Split('.');
            if (!ProtectedPart.Contains(ListProperty[0].ToLower())) {
                dynamic subobject = TheContentItem.Parts.Where(x => x.PartDefinition.Name == ListProperty[0]).FirstOrDefault();
                if (subobject == null)
                    throw new Exception("Part " + ListProperty[0] + " not exist");
                Int32 numparole = ListProperty.Count();
                for (int i = 1; i < numparole; i++) {
                    string property = ListProperty[i];
                    if (i != numparole - 1)
                        subobject = subobject.GetType().GetProperty(property);
                    else {
                        try { // é una proprietà
                            subobject.GetType().GetProperty(property).SetValue(subobject, Convert.ChangeType(value, subobject.GetType().GetProperty(property).PropertyType), null);
                            // potrei ancora tentare di scrivere direttamente con
                            // subobject.GetType().GetProperty(property).SetValue(subobject, value, null);
                        } catch { // é un field della parte
                            List<ContentPart> lcp = new List<ContentPart>();
                            lcp.Add((ContentPart)subobject);
                            StoreInspectExpandoFields(lcp, property, value, TheContentItem);
                        }
                    }
                }
            }
        }

        //private void RegistraValoreArray(object obj, string key, object[] value) {
        //    var property = obj.GetType().GetProperty(key);
        //    if (property != null) {
        //        Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        //        for (Int32 i = 0; i <= value.Length; i++) {
        //            object safeValue = (value[i] == null) ? null : Convert.ChangeType(value[i], t);
        //            property.SetValue(obj, safeValue, null);
        //        }
        //    }
        //}

        private void RegistraValoreEnumerator(object obj, string key, object value) {
            ListMode listmode = ((dynamic)obj).PartFieldDefinition.Settings.GetModel<EnumerationFieldSettings>().ListMode;
            if (listmode != ListMode.Listbox && listmode != ListMode.Checkbox) {
                key = "Value";
            }
            var property = obj.GetType().GetProperty(key);
            if (property != null) {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                //   ListMode listmode = ((dynamic)obj).PartFieldDefinition.Settings.GetModel<EnumerationFieldSettings>().ListMode;
                if (listmode != ListMode.Listbox && listmode != ListMode.Checkbox) {
                    try {
                        if (((dynamic)value).Count == 1) {
                            value = ((dynamic)value)[0];
                        }
                    } catch { }
                    // string myvalue = ((List<object>)value).Select(x => x.ToString()).FirstOrDefault();
                    //  object safeValue = (value == null) ? null : Convert.ChangeType(myvalue, t);
                    //  property.SetValue(obj, myvalue, null);
                    RegistraValore(obj, key, value);
                } else {
                    if (t.Name == "String[]") { // caso di enumerationfield con multivalue
                        object safeValue = (value == null) ? null : ((List<object>)value).Select(x => x.ToString()).ToArray();
                        property.SetValue(obj, safeValue, null);
                    }
                }
            }
        }

        private void RegistraValore(object obj, string key, object value) {
            // non posso fare questo
            //      fieldObj.GetType().GetProperty("Value").SetValue(fieldObj, value, null);
            // perchè non regge il tipo nullabile
            var property = obj.GetType().GetProperty(key);
            if (property != null) {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object safeValue = (value == null) ? null : Convert.ChangeType(value, t);
                property.SetValue(obj, safeValue, null);
            }
        }

        private void RegistraValoreContentPickerField(object obj, string key, object value) {
            // Convert ha varie implementazioni e non sa castare il tipo correttamente
            var property = obj.GetType().GetProperty(key);
            if (property != null) {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object safeValue = (value == null) ? null : Convert.ChangeType(((List<object>)value).Select(x => Convert.ToInt32(x)).ToArray(), t);
                property.SetValue(obj, safeValue, null);
            }
        }

        //private TaxoVM FindTaxoVM(List<TaxoVM> elements, Int32 idToFind) {
        //    if (elements != null) {
        //        foreach (TaxoVM myterm in elements) {
        //            if (myterm.Id == idToFind)
        //                return myterm;
        //            else
        //                return FindTaxoVM(myterm.child, idToFind);
        //        }
        //        return null;
        //    }
        //    else
        //        return null;
        //}
        private List<TaxoVM> ConvertToVM(List<dynamic> obj) {
            List<TaxoVM> listatvm = new List<TaxoVM>();
            foreach (dynamic el in obj) {
                TaxoVM newel = new TaxoVM();
                newel.Id = Convert.ToInt32(el);
                listatvm.Add(newel);
            }
            return listatvm;
        }

        public bool FileAllowed(string filename) {
            if (filename != null && filename.IndexOf('.') > 0)
                return allowedFiles.Contains(filename.ToLower().Split('.').Last());
            else
                return false;
        }
    }

    public class TaxoVM {

        public TaxoVM() {
            testo = "";
            valore = "";
            child = new List<TaxoVM>();
        }

        public Int32 Id { get; set; }
        public string testo { get; set; }
        public string valore { get; set; }
        public List<TaxoVM> child { get; set; }
    }
}
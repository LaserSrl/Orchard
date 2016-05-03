using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Fields.Fields;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IContentSerializationServices : IDependency {
        JProperty SerializeContentItem(ContentItem item, int actualLevel);
    }

    public class ContentSerializationServices : IContentSerializationServices {

        private readonly ITaxonomyService _taxonomyService;

        private readonly string[] _skipAlwaysProperties;
        private readonly string[] _skipFieldProperties;
        private readonly string[] _skipFieldTypes;
        private readonly string[] _skipPartNames;
        private readonly string[] _skipPartProperties;
        private readonly string[] _skipPartTypes;

        private readonly Type[] _basicTypes;

        private int _maxLevel = 10;

        private List<string> processedItems;

        public ContentSerializationServices(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;

            _skipAlwaysProperties = new string[]{ "ContentItemRecord","ContentItemVersionRecord" };
            _skipFieldProperties = new string[]{ "Storage", "Name", "DisplayName", "Setting" };
            _skipFieldTypes = new string[]{ "FieldDefinition","PartFieldDefinition" };
            _skipPartNames = new string[]{ "InfosetPart","FieldIndexPart","IdentityPart","UserPart","UserRolesPart", "AdminMenuPart", "MenuPart" };
            _skipPartProperties = new string[] { };
            _skipPartTypes = new string[]{ "ContentItem","Zones","TypeDefinition","TypePartDefinition","PartDefinition", "Settings", "Fields", "Record" };

            _basicTypes = new Type[] {
                typeof(string),
                typeof(decimal),
                typeof(float),
                typeof(int),
                typeof(bool),
                typeof(DateTime),
                typeof(Enum)
            };

            processedItems = new List<string>();
        }

        public JProperty SerializeContentItem(ContentItem item, int actualLevel) {
            if ((actualLevel + 1) > _maxLevel) {
                return new JProperty("ContentItem", "...");
            }
            JProperty jsonItem;
            var jsonProps = new JObject(
                new JProperty("Id", item.Id),
                new JProperty("Version", item.Version));

            var partsObject = new JObject();
            var parts = item.Parts
                .Where(cp => !cp.PartDefinition.Name.Contains("`") && !_skipPartNames.Contains(cp.PartDefinition.Name)
                );
            foreach (var part in parts) {
                jsonProps.Add(SerializePart(part, actualLevel + 1));
            }

            jsonItem = new JProperty(item.ContentType,
                jsonProps
                );

            return jsonItem;
        }

        private JProperty SerializePart(ContentPart part, int actualLevel) {
            // ciclo sulle properties delle parti
            var properties = part.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop =>
                !_skipPartTypes.Contains(prop.Name) //skip 
                );
            var partObject = new JObject();
            foreach (var property in properties) {
                try {
                    if (!_skipPartProperties.Contains(property.Name)) {
                        object val = property.GetValue(part, BindingFlags.GetProperty, null, null, null);
                        if (val != null) {
                            PopulateJObject(ref partObject, property, val, _skipPartProperties, actualLevel);
                        }
                    }
                }
                catch { }
            }

            //// now add the fields to the json object....
            foreach (var contentField in part.Fields) {
                var fieldObject = SerializeField(contentField, actualLevel);
                partObject.Add(fieldObject);
            }

            try {
                if (part.GetType() == typeof(ContentPart) && !part.PartDefinition.Name.EndsWith("Part")) {
                    return new JProperty(part.PartDefinition.Name + "DPart", partObject);
                }
                else {
                    return new JProperty(part.PartDefinition.Name, partObject);
                }
            }
            catch {
                return new JProperty(Guid.NewGuid().ToString(), partObject);
            }
        }

        private JProperty SerializeField(ContentField field, int actualLevel) {
            var fieldObject = new JObject();
            if (field.FieldDefinition.Name == "EnumerationField") {
                var enumField = (EnumerationField)field;
                string[] selected = enumField.SelectedValues;
                string[] options = enumField.PartFieldDefinition.Settings["EnumerationFieldSettings.Options"].Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                fieldObject.Add("Options", JToken.FromObject(options));
                fieldObject.Add("SelectedValues", JToken.FromObject(selected));
            }
            else if (field.FieldDefinition.Name == "TaxonomyField") {
                var taxoField = (TaxonomyField)field;
                fieldObject.Add("Terms", JToken.FromObject(taxoField.Terms.Select(x => x.Id).ToList()));
                var taxo = taxoField.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"];
                var taxoPart = _taxonomyService.GetTaxonomyByName(taxo);
                JArray arr = new JArray();
                fieldObject.Add("Taxonomy", arr);
                foreach (var term in taxoPart.Terms) {
                    CustomTermPart customTermPart = new CustomTermPart {
                        Id = term.Id,
                        Name = term.Name,
                        Path = term.Path,
                        Selectable = term.Selectable,
                        Slug = term.Slug
                    };
                    JToken jObj = JToken.FromObject(customTermPart);
                    arr.Add(jObj);
                    var contentPartList = term.ContentItem.Parts.Where(x => (x.GetType().Name == "ContentPart") && (x.PartDefinition.Name == term.ContentItem.TypeDefinition.Name)); // part aggiunta da Orchard per contenere i fields diretti
                    foreach (var contentPart in contentPartList) {
                        foreach (var innerField in contentPart.Fields) {
                            jObj.Last.AddAfterSelf(SerializeField(innerField, actualLevel));
                        }
                    }
                }
            }
            else {
                var properties = field.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop =>
                    !_skipFieldTypes.Contains(prop.Name) //skip 
                    );

                foreach (var property in properties) {
                    try {
                        if (!_skipFieldProperties.Contains(property.Name)) {
                            object val = property.GetValue(field, BindingFlags.GetProperty, null, null, null);
                            if (val != null) {
                                PopulateJObject(ref fieldObject, property, val, _skipFieldProperties, actualLevel);
                            }
                        }
                    }
                    catch {

                    }
                }
            }

            return new JProperty(field.Name, fieldObject);
        }

        private JProperty SerializeObject(object item, int actualLevel, string[] skipProperties = null) {
            if ((actualLevel + 1) > _maxLevel) {
                return new JProperty(item.GetType().Name, "...");
            }
            try {
                if (((dynamic)item).Id != null) {
                    if (processedItems.Contains(String.Format("{0}({1})", item.GetType().Name, ((dynamic)item).Id)))
                        return null;
                }
            }
            catch {
            }
            skipProperties = skipProperties ?? new string[0];
            try {
                if (item is ContentPart) {
                    return SerializePart((ContentPart)item, actualLevel);
                }
                else if (item is ContentField) {
                    return SerializeField((ContentField)item, actualLevel);
                }
                else if (item is ContentItem) {
                    return SerializeContentItem((ContentItem)item, actualLevel + 1);
                }
                else if (typeof(IEnumerable).IsInstanceOfType(item)) { // Lista o array
                    JArray array = new JArray();
                    foreach (var itemArray in (item as IEnumerable)) {
                        if (IsBasicType(itemArray.GetType())) {
                            var valItem = itemArray;
                            FormatValue(ref valItem);
                            array.Add(valItem);
                        }
                        else {
                            array.Add(new JObject(SerializeObject(itemArray, actualLevel + 1, skipProperties)));
                        }
                    }
                    PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    return new JProperty(item.GetType().Name, array);
                }
                else if (item.GetType().IsClass) {
                    var members = item.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public).Cast<MemberInfo>()
                    .Union(item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    .Where(m => !skipProperties.Contains(m.Name) && !_skipAlwaysProperties.Contains(m.Name))
                    ;
                    List<JProperty> properties = new List<JProperty>();
                    foreach (var member in members) {
                        var propertyInfo = item.GetType().GetProperty(member.Name);
                        object val = item.GetType().GetProperty(member.Name).GetValue(item);
                        if (IsBasicType(propertyInfo.PropertyType)) {
                            var memberVal = val;
                            FormatValue(ref memberVal);
                            properties.Add(new JProperty(member.Name, memberVal));
                        }
                        else if (typeof(IEnumerable).IsInstanceOfType(val)) {
                            JArray arr = new JArray();
                            properties.Add(new JProperty(member.Name, arr));
                            foreach (var element in (val as IEnumerable)) {
                                if (IsBasicType(element.GetType())) {
                                    var valItem = element;
                                    FormatValue(ref valItem);
                                    arr.Add(valItem);
                                }
                                else {
                                    var aux = SerializeObject(element, actualLevel + 1, skipProperties);
                                    arr.Add(new JObject(aux));
                                }
                            }
                        }
                        else {
                            properties.Add(SerializeObject(propertyInfo.GetValue(item), actualLevel + 1, skipProperties));
                        }
                    }
                    PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    return new JProperty(item.GetType().Name, new JObject(properties));

                    //JObject propertiesObject;
                    //var serializer = JsonSerializerInstance();
                    //propertiesObject = JObject.FromObject(item, serializer);
                    //foreach (var skip in skipProperties) {
                    //    propertiesObject.Remove(skip);
                    //}
                    //PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    //return new JProperty(item.GetType().Name, propertiesObject);
                }
                else {
                    PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    return new JProperty(item.GetType().Name, item);
                }
            }
            catch (Exception ex) {
                return new JProperty(item.GetType().Name, ex.Message);
            }

        }

        private void PopulateJObject(ref JObject jObject, PropertyInfo property, object val, string[] skipProperties, int actualLevel) {

            JObject propertiesObject;
            var serializer = JsonSerializerInstance();
            if (val is Array || val.GetType().IsGenericType) {
                JArray array = new JArray();
                foreach (var itemArray in (IEnumerable)val) {

                    if (!IsBasicType(itemArray.GetType())) {
                        array.Add(new JObject(SerializeObject(itemArray, actualLevel, skipProperties)));
                    }
                    else {
                        var valItem = itemArray;
                        FormatValue(ref valItem);
                        array.Add(valItem);
                    }
                }
                jObject.Add(new JProperty(property.Name, array));

            }
            else {
                // jObject.Add(SerializeObject(val, skipProperties));
            }
            if (!IsBasicType(val.GetType())) {
                try {
                    propertiesObject = JObject.FromObject(val, serializer);
                    foreach (var skip in skipProperties) {
                        propertiesObject.Remove(skip);
                    }
                    jObject.Add(property.Name, propertiesObject);
                }
                catch {
                    jObject.Add(new JProperty(property.Name, val.GetType().FullName));
                }
            }
            else {
                FormatValue(ref val);
                jObject.Add(new JProperty(property.Name, val));
            }
        }

        private bool IsBasicType(Type type) {
            return _basicTypes.Contains(type) || type.IsEnum;
        }

        private void FormatValue(ref object val) {
            if (val != null && val.GetType().IsEnum) {
                val = val.ToString();
            }
        }

        private void PopulateProcessedItems(string key, dynamic id) {
            if (id != null)
                processedItems.Add(String.Format("{0}({1})", key, id.ToString()));
        }

        private JsonSerializer JsonSerializerInstance() {
            return new JsonSerializer {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatString = "#MM-dd-yyyy hh.mm.ss#",
            };
        }
    }

    class CustomTermPart {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Slug { get; set; }
        public bool Selectable { get; set; }
    }
}
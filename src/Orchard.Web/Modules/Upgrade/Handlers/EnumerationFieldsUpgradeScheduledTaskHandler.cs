using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Fields.Fields;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;

namespace Upgrade.Handlers {
    public class EnumerationFieldsUpgradeScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IContentHandler> _contentHandlers;

        public static string TaskType = "Upgrade.EnumerationFields.Task";

        public ILogger Logger { get; set; }

        public EnumerationFieldsUpgradeScheduledTaskHandler(IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IEnumerable<IContentHandler> contentHandlers) {

            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _contentHandlers = contentHandlers;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType.Equals(TaskType, StringComparison.OrdinalIgnoreCase)) {
                // Content types
                var contentTypes = _contentDefinitionManager.ListTypeDefinitions()
                    // Where a part
                    .Where(ct => ct.Parts
                        // Contains a Enumeration Field
                        .Any(pd => pd.PartDefinition.Fields
                            .Any(pfd => pfd.FieldDefinition.Name.Equals("EnumerationField", StringComparison.OrdinalIgnoreCase))))
                    .Select(ct => ct.Name)
                    .ToArray();

                var query = _contentManager.Query()
                    .ForType(contentTypes);

                var sliceSize = 20;
                var i = 0;
                var contentItems = query.Slice(i * sliceSize, sliceSize);

                while (contentItems.Any()) {
                    foreach (var ci in contentItems) {
                        UpdateItem(ci);
                    }
                    i++;
                    contentItems = query.Slice(i * sliceSize, sliceSize);
                }
            }
        }

        private ContentItem UpdateItem(ContentItem ci) {
            var updateContext = new UpdateContentContext(ci);
            _contentHandlers.Invoke(handler => handler.Updating(updateContext), Logger);

            // Force the update for each EnumerationField.
            var enumFields = ci.Parts
                .SelectMany(pa => pa.Fields)
                .Where(fi => fi is EnumerationField)
                .ToList();

            foreach(EnumerationField field in enumFields) {
                var v = field.Value;
                field.Value = v;
            }
            
            _contentHandlers.Invoke(handler => handler.Updated(updateContext), Logger);

            if (ci.IsPublished()) {
                ci.VersionRecord.Published = false;
                _contentManager.Publish(ci);
            }
            return ci;
        }
    }
}
using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Pdf {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("PrintButtonPart", p => p
                .Attachable()
            );
            return 1;
        }
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("PrintButtonPart", p => p
                .Attachable()
                .WithDescription("Adds a Print Button which opens a new window.")
            );
            return 2;
        }
    }
}
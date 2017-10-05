using System;
using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.UsersExtensions {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("User", content => content
                .WithPart("UserRegistrationPolicyPart"));

            return 1;
        }

        /// <summary>
        /// This migration added when we implemented the front end settings for display/
        /// edit controlled by ProfilePart, that need things you want to show on front end to 
        /// be in the actual definitions of ContentTypes.
        /// </summary>
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("FavoriteCulturePart", builder => builder
                .Attachable(false));
            ContentDefinitionManager.AlterTypeDefinition("User", content => content
                .WithPart("FavoriteCulturePart"));

            return 2;
        }
    }
}

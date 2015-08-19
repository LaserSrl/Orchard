using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Laser.Orchard.Questionnaires {
    public class Permissions : IPermissionProvider {
        public static readonly Permission SubmitQuestionnaire = new Permission { Description = "Submit questionnaire", Name = "SubmitQuestionnaire" };
        public static readonly Permission AccessStatistics = new Permission { Description = "Access questionnaire statistics", Name = "AccessStatistics" };


        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                SubmitQuestionnaire,
                AccessStatistics,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {SubmitQuestionnaire}
                },
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {AccessStatistics}
                },

            };
        }
    }
}




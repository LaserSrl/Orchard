using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions.Models;
using Orchard.ContentManagement;
using Laser.Orchard.Reporting.Models;
using Orchard.Localization;
using Orchard.Core.Title.Models;

namespace Laser.Orchard.Reporting.Security {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ShowDataReports = new Permission { Description = "Show Data Reports on back-end menu", Name = "ShowDataReports" };
        private readonly IContentManager _contentManager;
        public Localizer T;
        public Feature Feature { get; set; }
        public Permissions(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ShowDataReports}
                },
                new PermissionStereotype {
                    Name = "Editor",
                },
                new PermissionStereotype {
                    Name = "Moderator",
                  },
                new PermissionStereotype {
                    Name = "Author",
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            var result = new List<Permission>();
            var reportPermissions = new List<Permission>();
            result.Add(ShowDataReports);
            foreach(var report in GetReports()) {
                var title = (report.ContentItem.Has<TitlePart>() ? report.ContentItem.As<TitlePart>().Title : T("[No Title]").ToString());
                reportPermissions.Add(new Permission {
                    Name = string.Format("ShowDataReport{0}", report.Id),
                    Description = string.Format("Show Data Report {0}", title)
                });
            }
            ShowDataReports.ImpliedBy = reportPermissions;
            result.AddRange(reportPermissions);
            return result;
        }
        private IEnumerable<DataReportViewerPart> GetReports() {
            return _contentManager.Query<DataReportViewerPart>().List();
        }
    }
}
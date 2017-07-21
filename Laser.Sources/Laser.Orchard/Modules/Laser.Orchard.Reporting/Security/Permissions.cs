using Orchard.Security.Permissions;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.ContentManagement;
using Laser.Orchard.Reporting.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;

namespace Laser.Orchard.Reporting.Security {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ShowDataReports = new Permission { Description = "Show Data Reports on back-end menu", Name = "ShowDataReports" };
        public static readonly Permission ShowDataDashboard = new Permission { Description = "Show Dashboards on back-end menu", Name = "ShowDataDashboard" };
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        public Localizer T;
        public Feature Feature { get; set; }
        public Permissions(IContentManager contentManager, ITransactionManager transactionManager) {
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            T = NullLocalizer.Instance;
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ShowDataReports, ShowDataDashboard }
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
            var reportPermissions = GetReportPermissions();
            result.Add(ShowDataReports);
            result.AddRange(reportPermissions.Values);
            var dashboardPermissions = GetDashboardPermissions();
            ShowDataDashboard.ImpliedBy = dashboardPermissions.Values;
            result.Add(ShowDataDashboard);
            result.AddRange(dashboardPermissions.Values);
            return result;
        }
        public Dictionary<int, Permission> GetReportPermissions() {
            Dictionary<int, Permission> result = new Dictionary<int, Permission>();
            // utilizza una query hql per migliorare le performance
            var hql = @"select ci.Id as id, title.Title as tot
                from Orchard.ContentManagement.Records.ContentItemRecord as ci
                join ci.Versions as civ with civ.Published=1
                join ci.DataReportViewerPartRecord as vw
                left join civ.TitlePartRecord as title";
            var reportList = _transactionManager.GetSession().CreateQuery(hql).List();
            foreach(object[] report in reportList) {
                var title = (report[1] != null ? report[1] : T("[No Title]").ToString());
                result.Add((int)(report[0]), new Permission {
                    Name = string.Format("ShowDataReport{0}", report[0]),
                    Description = string.Format("Show Data Report {0}", title)
                });
            }

            // codice equivalente ma molto più lento
            //var reportList = _contentManager.Query<DataReportViewerPart>().List();
            //foreach (var report in reportList) {
            //    var title = (report.ContentItem.Has<TitlePart>() ? report.ContentItem.As<TitlePart>().Title : T("[No Title]").ToString());
            //    result.Add(report.Id, new Permission {
            //        Name = string.Format("ShowDataReport{0}", report.Id),
            //        Description = string.Format("Show Data Report {0}", title)
            //    });
            //}
            return result;
        }
        public Dictionary<int, Permission> GetDashboardPermissions() {
            Dictionary<int, Permission> result = new Dictionary<int, Permission>();
            var dashboardList = _contentManager.Query("DataReportDashboard").List();
            foreach (var dashboard in dashboardList) {
                var title = (dashboard.Has<TitlePart>() ? dashboard.As<TitlePart>().Title : T("[No Title]").ToString());
                result.Add(dashboard.Id, new Permission {
                    Name = string.Format("ShowDashboard{0}", dashboard.Id),
                    Description = string.Format("Show Dashboard {0}", title)
                });
            }
            return result;
        }
    }
}
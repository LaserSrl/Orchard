using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using Laser.Orchard.GoogleAnalytics.Models;

namespace Laser.Orchard.GoogleAnalytics.Services {
	[OrchardFeature("Laser.Orchard.GoogleAnalytics")]
	public class MissingSettingsBanner : INotificationProvider {
		private readonly IOrchardServices _orchardServices;

		public MissingSettingsBanner(IOrchardServices orchardServices) {
			_orchardServices = orchardServices;
			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		public IEnumerable<NotifyEntry> GetNotifications() {
			var googleAnalyticsSettings = _orchardServices.WorkContext.CurrentSite.As<GoogleAnalyticsSettingsPart>();
			if (googleAnalyticsSettings == null || string.IsNullOrWhiteSpace(googleAnalyticsSettings.GoogleAnalyticsKey)) {
				yield return new NotifyEntry { Message = T("The Google Analytics settings need to be configured."), Type = NotifyType.Warning };
			}
		}
	}
}
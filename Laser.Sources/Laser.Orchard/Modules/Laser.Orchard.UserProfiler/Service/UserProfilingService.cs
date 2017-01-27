using Laser.Orchard.UserProfiler.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security;
using Orchard.Users.Models;
using Laser.Orchard.UserProfiler.ViewModels;

namespace Laser.Orchard.UserProfiler.Service {
    public class UserProfilingService : IUserProfilingService {
        private readonly IRepository<UserProfilingSummaryRecord> _userProfilingSummaryRecord;
        private readonly IContentManager _contentManager;
        public UserProfilingService(
            IRepository<UserProfilingSummaryRecord> userProfilingSummaryRecord,
            IContentManager contentManager) {
            _userProfilingSummaryRecord = userProfilingSummaryRecord;
            _contentManager = contentManager;
        }


        public Dictionary<string, int> UpdateProfile(int UserId, UpdateVM update) {
            var dicSUM=new Dictionary<string, int>();
            foreach(var el in update.Profile) {
               var dicout= UpdateProfile(UserId, el.Text, el.Type, el.Count);
               dicSUM.Union(dicout).ToDictionary(k => k.Key, v => v.Value);
            }
            return dicSUM;
        }

        public Dictionary<string, int> UpdateProfile(int UserId, string text, TextSourceTypeOptions sourceType, int count) {
            var item = _userProfilingSummaryRecord.Fetch(x => x.UserProfilingPartRecord.Id.Equals(UserId) && x.Text.Equals(text) && x.SourceType==sourceType).FirstOrDefault();
            if (item == null) {
                var userProfilingPartRecord = ((dynamic)_contentManager.Get(UserId)).UserProfilingPart.Record;
                item = new UserProfilingSummaryRecord() {
                    SourceType = sourceType,
                    Text = text,
                    Count=count,
                    UserProfilingPartRecord = userProfilingPartRecord
                };
                _userProfilingSummaryRecord.Create(item);
            }
            else {
                item.Count += count;
                _userProfilingSummaryRecord.Update(item);
            }
            var data = new Dictionary<string, int>();
            data.Add(text,  item.Count);
            return data;
        }

    }

}
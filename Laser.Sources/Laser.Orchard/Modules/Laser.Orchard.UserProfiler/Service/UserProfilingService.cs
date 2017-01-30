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


        public Dictionary<string, int> UpdateProfile(int UserId, List<ProfileVM> update) {
            var dicSUM = new Dictionary<string, int>();
            foreach (var el in update) {
                TextSourceTypeOptions sourcetype = (TextSourceTypeOptions)Enum.Parse(typeof(TextSourceTypeOptions), el.Type);
                var dicout = UpdateProfile(UserId, el.Text, sourcetype, el.Count);
                if (dicSUM.ContainsKey(dicout.Keys.First()))
                    dicSUM[dicout.Keys.First()] =  dicout[dicout.Keys.First()];
                else
                    dicSUM.Add(dicout.Keys.First(), dicout[dicout.Keys.First()]);
            }
            return dicSUM;
        }

        public Dictionary<string, int> UpdateProfile(int UserId, int id) {
            return UpdateProfile(UserId, id.ToString(), TextSourceTypeOptions.ContentItem, 1);
            //var item = _userProfilingSummaryRecord.Fetch(x => x.UserProfilingPartRecord.Id.Equals(UserId) && x.SourceType == TextSourceTypeOptions.ContentItem && x.Text.Equals(id.ToString())).FirstOrDefault();
            //if (item == null) {
            //    var userProfilingPartRecord = ((dynamic)_contentManager.Get(UserId)).UserProfilingPart.Record;
            //    item = new UserProfilingSummaryRecord() {
            //        SourceType = TextSourceTypeOptions.ContentItem,
            //        Text = id.ToString(),
            //        Count = 1,
            //        UserProfilingPartRecord = userProfilingPartRecord
            //    };
            //    _userProfilingSummaryRecord.Create(item);
            //}
            //else {
            //    item.Count += 1;
            //    _userProfilingSummaryRecord.Update(item);
            //}
            //var data = new Dictionary<string, int>();
            //data.Add(text, item.Count);
            //return data;
        }


        public Dictionary<string, int> UpdateProfile(int UserId, string text, TextSourceTypeOptions sourceType, int count) {
            var item = _userProfilingSummaryRecord.Fetch(x => x.UserProfilingPartRecord.Id.Equals(UserId) && x.Text.Equals(text) && x.SourceType == sourceType).FirstOrDefault();
            if (item == null) {
                var userProfilingPartRecord = ((dynamic)_contentManager.Get(UserId)).UserProfilingPart.Record;
                item = new UserProfilingSummaryRecord() {
                    SourceType = sourceType,
                    Text = text,
                    Count = count,
                    UserProfilingPartRecord = userProfilingPartRecord
                };
                _userProfilingSummaryRecord.Create(item);
            }
            else {
                item.Count += count;
                _userProfilingSummaryRecord.Update(item);
            }
            var data = new Dictionary<string, int>();
            data.Add(text, item.Count);
            return data;
        }

    }

}
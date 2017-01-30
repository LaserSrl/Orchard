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
using Orchard.Tags.Models;
using Laser.Orchard.StartupConfig.ViewModels;

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


        public Dictionary<string, int> UpdateUserProfile(int UserId, List<ProfileVM> update) {
            var dicSUM = new Dictionary<string, int>();
            foreach (var el in update) {
                TextSourceTypeOptions sourcetype = (TextSourceTypeOptions)Enum.Parse(typeof(TextSourceTypeOptions), el.Type);
                var dicout = UpdateUserProfile(UserId, el.Text, sourcetype, el.Count);
                if (dicSUM.ContainsKey(dicout.Keys.First()))
                    dicSUM[dicout.Keys.First()] = dicout[dicout.Keys.First()];
                else
                    dicSUM.Add(dicout.Keys.First(), dicout[dicout.Keys.First()]);
            }
            return dicSUM;
        }


        private Dictionary<string, int> ProfileTagPart(int UserId, TagsPart tagPart) {
            var dicSUM = new Dictionary<string, int>();
            if (tagPart != null) {
                var listTags = tagPart.CurrentTags;
                    foreach (var tag in listTags) {
                        var dicout = UpdateUserProfile(UserId, tag, TextSourceTypeOptions.Tag, 1);
                        if (dicSUM.ContainsKey(tag))
                            dicSUM[tag] = dicout[tag];
                        else
                            dicSUM.Add(tag, dicout[tag]);
                    }
                }
            return dicSUM;
        }

        public Dictionary<string, int> UpdateUserProfile(int UserId, int id) {
             var dicSUM =new Dictionary<string, int>();
             if (id > 0) {
                 var content = _contentManager.Get(id);
                 if (content != null && content.As<TrackingPart>() != null) {
                     dicSUM = ProfileTagPart(UserId, content.As<TagsPart>());
                     var responseDictionary = UpdateUserProfile(UserId, id.ToString(), TextSourceTypeOptions.ContentItem, 1);
                     dicSUM.Add(responseDictionary.Keys.FirstOrDefault(), responseDictionary[responseDictionary.Keys.FirstOrDefault()]);
                 }
             } return dicSUM;
        }


        public Dictionary<string, int> UpdateUserProfile(int UserId, string text, TextSourceTypeOptions sourceType, int count) {
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
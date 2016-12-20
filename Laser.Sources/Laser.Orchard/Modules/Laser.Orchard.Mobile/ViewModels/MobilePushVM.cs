using Laser.Orchard.Mobile.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.ViewModels {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class MobilePushVM {
        public MobilePushVM() {
            TitlePush = "";
            TextPush = "";
            ToPush = false;
            TestPush = false;
            DevicePush = "All";
            PushSent = false;
            TargetDeviceNumber = 0;
            PushSentNumber = 0;
            PushAdvertising = true;
            PushTestNumber = 0;
        }
        public string TitlePush { get; set; }
        public string TextPush { get; set; }
        public bool ToPush { get; set; }
        public bool TestPush { get; set; }
        public string DevicePush { get; set; }
        public string RecipientList { get; set; }


        // proprietà aggiuntive
        public bool PushSent { get; set; }
        public int TargetDeviceNumber { get; set; }
        public int PushSentNumber { get; set; }

        public SelectList ListOfDevice {
            get {
                SelectList enumToList = new SelectList(Enum.GetValues(typeof(TipoDispositivo)).Cast<TipoDispositivo>().Select(v => new SelectListItem {
                    Text = v.ToString(),
                    Value = v.ToString()
                }).ToList(), "Value", "Text");
                List<SelectListItem> _list = enumToList.ToList();
                _list.Insert(0, new SelectListItem() { Value = "All", Text = "All" });
                return new SelectList((IEnumerable<SelectListItem>)_list, "Value", "Text");
            }
        }
        public bool ShowTestOptions { get; set; }
        public bool HideRelated { get; set; }
        public bool PushAdvertising { get; set; }
        public int PushTestNumber { get; set; }
        public string SiteUrl { get; set; }
    }
}
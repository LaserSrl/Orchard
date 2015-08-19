using Laser.Orchard.Payment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Payment.ViewModels {
    public class PaymentSettingsVM {
        public string GestpayShopLogin { get; set; }
        public bool GestpayTest { get; set; }
        public string PaymentMethodSelected { get; set; }
        public SelectList ListOfPaymentMethod {
            get {
                SelectList enumToList = new SelectList(Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>().Select(v => new SelectListItem {
                    Text = v.ToString(),
                    Value = v.ToString()
                }).ToList(), "Value", "Text");
                List<SelectListItem> _list = enumToList.ToList();
             //   _list.Insert(0, new SelectListItem() { Value = "All", Text = "All" });
                return new SelectList((IEnumerable<SelectListItem>)_list, "Value", "Text");
            }
        }
    }
}
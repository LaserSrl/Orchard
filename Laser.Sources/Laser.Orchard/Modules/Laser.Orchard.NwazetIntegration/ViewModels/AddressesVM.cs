using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NwazetIntegration.Models;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class AddressesVM {
        public string Submit { get; set; }
        public int OrderId { get; set; }
        public Address ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string SpecialInstructions { get; set; }
    }
}
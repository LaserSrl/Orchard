using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Payment {
    public class Migrations  : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("PaymentSettingsPartRecord", table => table
                       .ContentPartRecord()
                       .Column<string>("GestpayShopLogin")
                       .Column<bool>("GestpayTest")
           );
            return 1;
        }
        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("PaymentSettingsPartRecord", table => table
                       .AddColumn<string>("PaymentMethodSelected")
           );
            return 2;
        }
    }
}
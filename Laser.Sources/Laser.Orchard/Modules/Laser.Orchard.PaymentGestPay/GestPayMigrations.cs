using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay {
    public class GestPayMigrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("PaymentGestPaySettingsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("GestPayShopLogin")
                    .Column<bool>("UseTestEnvironment")
                );


            return 1;
        }
    }
}
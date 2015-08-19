using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.BikeSharing.Models;
using Laser.Orchard.BikeSharing.Services;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.BikeSharing.Handlers {
    public class BikeStationPartHandler : ContentHandler {
        private readonly IBikeServices _bikeServices;

        public BikeStationPartHandler(
            IBikeServices bikeServices) {
            _bikeServices = bikeServices;
        }

        protected override void Loading(LoadContentContext context) {
            base.Loading(context);

            var bikeSharingPart = (BikeStationPart)context.ContentItem.Parts.SingleOrDefault(x => x.PartDefinition.Name == "BikeStationPart");

            if (bikeSharingPart == null) {
                return;
            }
            bikeSharingPart._bikeStation.Loader(x => {
                return _bikeServices.GetStationInfo(bikeSharingPart.BikeStationUName);
            });

        }

    }
}
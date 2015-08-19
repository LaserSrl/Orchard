using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laser.Orchard.Countries.Models;

namespace Laser.Orchard.Countries.Services {
    public interface ICountriesService:IDependency {
        void Inizialize();
        List<LaserCountriesZoneRecord> GetAllZone();
        List<CountriesRecord> GetAllNazione(int idzone=-1);
        CountriesRecord GetNazione(int IdNazione);
        List<LaserCountriesZoneRecord> GetZone(int IdNazione);
    }
}

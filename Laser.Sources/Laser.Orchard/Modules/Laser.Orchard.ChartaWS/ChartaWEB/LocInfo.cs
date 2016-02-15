using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ChartaWS.ChartaWEB
{
    public class LocInfo
    {
        public long Id { get; set; }
        public string Sid { get; set; }
        public long IdOrg { get; set; }
        public string Code { get; set; }
        public string Nome { get; set; }
        public string Tipologia { get; set; }
        public string Indirizzo { get; set; }
        public string Comune { get; set; }
        public string Provincia { get; set; }
        public string Cap { get; set; }
        public string Descrizione { get; set; }
        public string Telefono { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Raggiugibilita { get; set; }
        public string InfoParcheggio { get; set; }
        public string InfoDisabili { get; set; }
        public string OrariApertura { get; set; }
        public string Sito { get; set; }
        public string Immagine { get; set; }
    }
}
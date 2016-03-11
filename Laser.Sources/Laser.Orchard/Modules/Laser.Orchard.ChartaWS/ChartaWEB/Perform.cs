using System;
namespace Laser.Orchard.ChartaWS.ChartaWEB
{
    public class Perform
    {
        public string VCode { get; set; }
        public string Code { get; set; }
        public string Titolo { get; set; }
        public string NomeTeatro { get; set; }
        public string Categoria { get; set; }
        public long IdEvento { get; set; }
        public string Comune { get; set; }
        public string Provincia { get; set; }
        public DateTime DataEOra { get; set; }
        public string Img { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public int Exact { get; set; }
    }
}
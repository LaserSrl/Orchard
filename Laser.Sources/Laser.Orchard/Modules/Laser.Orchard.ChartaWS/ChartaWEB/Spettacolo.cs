using System;

namespace Laser.Orchard.ChartaWS.ChartaWEB
{
    public class Spettacolo
    {
        public string PCode { get; set; }
        public string VCode { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public bool? Retired { get; set; }
        public int Stato { get; set; }
    }
}
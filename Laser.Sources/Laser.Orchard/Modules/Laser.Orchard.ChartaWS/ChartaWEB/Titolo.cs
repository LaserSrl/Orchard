namespace Laser.Orchard.ChartaWS.ChartaWEB
{
    public class Titolo
    {
        public long IdTitolo { get; set; }
        public string VCode { get; set; }
        public string VName { get; set; }
        public string VCity { get; set; }
        public string TitoloOriginale { get; set; }
        public string TitoloEditato { get; set; }
        public string DataInizio { get; set; }
        public string DataFine { get; set; }
        public int NumPerf { get; set; }
        public string Cat { get; set; }
        public string Sottocat { get; set; }
        public string Descrizione { get; set; }
        public string Img { get; set; }
        public string ImgMini { get; set; }
        public string Artista { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public int Exact { get; set; }
        public bool FasceOrarie { get; set; }
    }
}
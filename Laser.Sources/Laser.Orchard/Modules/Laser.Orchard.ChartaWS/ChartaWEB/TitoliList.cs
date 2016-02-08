using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using ChartaDb.ChartaTableAdapters;
using System.Data;
using System.Configuration;

namespace ChartaWEB
{
    public class TitoliList
    {
        private static readonly ILog logger = LogManager.GetLogger("TitoliList");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pidTitolo"></param>
        /// <param name="pvcode"></param>
        /// <param name="pcitta"></param>
        /// <param name="pquando"></param>
        /// <param name="pcatid"></param>
        /// <param name="psottcatid"></param>
        /// <param name="ppcode"></param>
        /// <returns></returns>
        public static string ListaTitoli (string  pidTitolo, string pvcode, string pcitta, string pquando , string pcatid, string psottcatid, string ppcode, string pdata, string titoloartista)
        {
            try{
                int idTitolo = -1 ;
                int idCat = -1;
                int idSottCat = -1;

                TitoliTableAdapter objTATitoli = new TitoliTableAdapter();

                if (pidTitolo != null)
                    idTitolo = int.Parse(pidTitolo);

                if (pcatid != null)
                    idCat = int.Parse(pcatid);

                if (psottcatid!= null)
                    idSottCat = int.Parse(psottcatid);

                ChartaDb.Charta.TitoliDataTable objDTTitoli;

                if (string.IsNullOrEmpty(pdata))
                {
                    objDTTitoli = objTATitoli.GetData(idTitolo, pvcode, pcitta, pquando, idCat, idSottCat, ppcode, null, titoloartista);
                }
                else
                {
                    objDTTitoli = objTATitoli.GetData(idTitolo, pvcode, pcitta, pquando, idCat, idSottCat, ppcode, DateTime.Parse(pdata), titoloartista);
                }

                

                string sReturn = "<reply>";
                sReturn += "<Titoli>";

                foreach (ChartaDb.Charta.TitoliRow dr in objDTTitoli)
                {
                    sReturn += "<titolo id=\"" + Util.ConvertWithANDReplace (dr.id_titolo.ToString()  ) + "\" > ";
                    sReturn += "<vcode>" + Util.ConvertWithANDReplace(dr.vcode) + "</vcode>";
                    sReturn += "<vname>" + Util.ConvertWithANDReplace(dr.vname) + "</vname>";
                    sReturn += "<vcity>" + Util.ConvertWithANDReplace(dr.vcity) + "</vcity>";
                    sReturn += "<titolo_originale>" + Util.ConvertWithANDReplace (dr.titolo_originale ) + "</titolo_originale>";
                    sReturn += "<titolo_editato>" + Util.ConvertWithANDReplace (dr.titolo_editato) + "</titolo_editato>";
                    sReturn += "<datainizio>" + Util.ConvertWithANDReplace(dr.datainizio.ToString()) + "</datainizio>";
                    sReturn += "<datafine>" + Util.ConvertWithANDReplace(dr.datafine.ToString()) + "</datafine>";
                    sReturn += "<numperf>" + Util.ConvertWithANDReplace (dr.numPerf.ToString() ) + "</numperf>";
                    sReturn += "<cat>" + Util.ConvertWithANDReplace(dr.cat) + "</cat>";
                    sReturn += "<sottocat>" + Util.ConvertWithANDReplace(dr.sottocat) + "</sottocat>";
                    sReturn += "<descrizione>" + Util.ConvertWithANDReplace (dr.descrizione) + "</descrizione>";
                    sReturn += "<img>" + Util.ConvertWithANDReplace (dr.immagine) + "</img>";
                    sReturn += "<img_mini>" + Util.ConvertWithANDReplace (dr.immagine_mini) + "</img_mini>";
                    sReturn += "<artista>" + Util.ConvertWithANDReplace (dr.artista ) + "</artista>";
                    sReturn += "<autore>" + Util.ConvertWithANDReplace (dr.autore ) + "</autore>";
                    sReturn += "<catalogo>" + Util.ConvertWithANDReplace (dr.catalogo) + "</catalogo>";
                    sReturn += "<compagnia>" + Util.ConvertWithANDReplace (dr.compagnia) + "</compagnia>";
                    sReturn += "<coreografia>" + Util.ConvertWithANDReplace (dr.coreografia ) + "</coreografia>";
                    sReturn += "<coro>" + Util.ConvertWithANDReplace (dr.coro) + "</coro>";
                    sReturn += "<direttore>" + Util.ConvertWithANDReplace (dr.direttore ) + "</direttore>";
                    sReturn += "<libretto>" + Util.ConvertWithANDReplace (dr.libretto_di ) + "</libretto>";
                    sReturn += "<musiche>" + Util.ConvertWithANDReplace (dr.musiche_di ) + "</musiche>";
                    sReturn += "<orchestra>" + Util.ConvertWithANDReplace (dr.orchestra ) + "</orchestra>";
                    sReturn += "<organizzazione>" + Util.ConvertWithANDReplace (dr.organizzazione ) + "</organizzazione>";
                    sReturn += "<regista>" + Util.ConvertWithANDReplace (dr.regista) + "</regista>";
                    sReturn += "<interprete>" + Util.ConvertWithANDReplace(dr.interprete ) + "</interprete>";
                    sReturn += "</titolo>";
                }

                SupportTableAdapter tableAdapterSupport = new SupportTableAdapter();
                DataRow[] dr1 = tableAdapterSupport.GetData().Select("key = 'baseImagePath'  ");
                string _BaseImagePath = dr1[0]["value"].ToString();
                sReturn += "<baseimagepath>" + _BaseImagePath + "</baseimagepath>";
                sReturn += "</Titoli></reply>";

                tableAdapterSupport.Dispose();
                objDTTitoli.Dispose();
                objTATitoli.Dispose();

                return sReturn;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("Titoli", "999", ex.Message); 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pidTitolo"></param>
        /// <param name="pvcode"></param>
        /// <param name="pcitta"></param>
        /// <param name="pquando"></param>
        /// <param name="pcatid"></param>
        /// <param name="psottcatid"></param>
        /// <param name="ppcode"></param>
        /// <returns></returns>
        public static string ListaTitoliShort (string pidTitolo, string pvcode, string pcitta, string pquando, string pcatid, string psottcatid, string ppcode, string pdata, string artista)
        {
            try
            {
                int idTitolo = -1;
                int idCat = -1;
                int idSottCat = -1;

                //TitoliShortTableAdapter objTATitoli = new TitoliShortTableAdapter();
                TitoliShortWithSearchTableAdapter objTATitoli = new TitoliShortWithSearchTableAdapter();


                if (pidTitolo != null)
                    idTitolo = int.Parse(pidTitolo);

                if (pcatid != null)
                    idCat = int.Parse(pcatid);

                if (psottcatid != null)
                    idSottCat = int.Parse(psottcatid);


                //ChartaDb.Charta.TitoliShortDataTable objDTTitoli;
                ChartaDb.Charta.TitoliShortWithSearchDataTable objDTTitoli;

                if (string.IsNullOrEmpty(pdata))
                    objDTTitoli = objTATitoli.GetData(idTitolo, pvcode, pcitta, pquando, idCat, idSottCat, ppcode, null, artista);
                else
                    objDTTitoli = objTATitoli.GetData(idTitolo, pvcode, pcitta, pquando, idCat, idSottCat, ppcode, DateTime.Parse(pdata), artista);

                string sReturn = "<reply>";
                sReturn += "<Titoli>";

                //foreach (ChartaDb.Charta.TitoliShortRow  dr in objDTTitoli)
                foreach (ChartaDb.Charta.TitoliShortWithSearchRow dr in objDTTitoli)
                {
                    sReturn += "<titolo id=\"" + Util.ConvertWithANDReplace(dr.id_titolo.ToString()) + "\" > ";
                    sReturn += "<vcode>" + Util.ConvertWithANDReplace(dr.vcode) + "</vcode>";
                    sReturn += "<vname>" + Util.ConvertWithANDReplace(dr.vname) + "</vname>";
                    sReturn += "<vcity>" + Util.ConvertWithANDReplace(dr.vcity) + "</vcity>";
                    sReturn += "<titolo_originale>" + Util.ConvertWithANDReplace(dr.titolo_originale) + "</titolo_originale>";
                    sReturn += "<titolo_editato>" + Util.ConvertWithANDReplace(dr.titolo_editato) + "</titolo_editato>";
                    sReturn += "<datainizio>" + Util.ConvertWithANDReplace(dr.datainizio.ToString()) + "</datainizio>";
                    sReturn += "<datafine>" + Util.ConvertWithANDReplace(dr.datafine.ToString()) + "</datafine>";
                    sReturn += "<numperf>" + Util.ConvertWithANDReplace(dr.numPerf.ToString()) + "</numperf>";
                    sReturn += "<cat>" + Util.ConvertWithANDReplace(dr.cat) + "</cat>";
                    sReturn += "<sottocat>" + Util.ConvertWithANDReplace(dr.sottocat) + "</sottocat>";
                    sReturn += "<descrizione>" + Util.ConvertWithANDReplace(dr.descrizione) + "</descrizione>";
                    sReturn += "<img>" + Util.ConvertWithANDReplace(dr.immagine) + "</img>";
                    sReturn += "<img_mini>" + Util.ConvertWithANDReplace(dr.immagine_mini) + "</img_mini>";
                    sReturn += "<artista>" + Util.ConvertWithANDReplace(dr.artista) + "</artista>";
                    sReturn += "<lat>" + Util.ConvertWithANDReplace(dr.LAT) + "</lat>";
                    sReturn += "<lon>" + Util.ConvertWithANDReplace(dr.LON) + "</lon>";
                    sReturn += "<exact>" + Util.ConvertWithANDReplace(dr.EXACT.ToString()) + "</exact>";
                    sReturn += "<fasceOrarie>" + Util.ConvertWithANDReplace(dr.fasceOrarie.ToString()) + "</fasceOrarie>";
                    sReturn += "</titolo>";
                }

                SupportTableAdapter tableAdapterSupport = new SupportTableAdapter();
                DataRow[] dr1 = tableAdapterSupport.GetData().Select("key = 'baseImagePath'  ");
                string _BaseImagePath = dr1[0]["value"].ToString();
                sReturn += "<baseimagepath>" + _BaseImagePath + "</baseimagepath>";
                sReturn += "</Titoli></reply>";

                tableAdapterSupport.Dispose();
                objDTTitoli.Dispose();
                objTATitoli.Dispose();

                return sReturn;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("Titoli", "999", ex.Message);
            }
        }


        public static string ListaTitoliAroundMe(string strlat, string strlon)
        {
            try
            {
                
                TitoliShortAroundMeTableAdapter objTATitoli = new TitoliShortAroundMeTableAdapter();

                double lat = double.Parse(strlat.Replace(",", "."));
                double lon = double.Parse(strlon.Replace(",", "."));

                double metersAroundMe = double.Parse( ConfigurationManager.AppSettings["METERS_AROUND_ME"].ToString());

                double dLat = LatLongDiff.MToLat(metersAroundMe, lat);
                double dLon = LatLongDiff.MToLon(metersAroundMe, lon);


                double tlLat = lat + dLat;
                double tlLon = lon - dLon;
                double brLat = lat - dLat;
                double brLon = lon + dLon;


                logger.Debug("tlLat tlLon brLat brLon: " + tlLat.ToString() + " " + tlLon.ToString() + " " + brLat.ToString() + " " + brLon.ToString());


                ChartaDb.Charta.TitoliShortAroundMeDataTable objDTTitoli = objTATitoli.GetData(tlLat.ToString(), tlLon.ToString(), brLat.ToString(), brLon.ToString());
                
                string sReturn = "<reply>";
                sReturn += "<metersAroundMe>" + metersAroundMe.ToString() + "</metersAroundMe>";
                sReturn += "<Titoli>";
               
                foreach (ChartaDb.Charta.TitoliShortAroundMeRow dr in objDTTitoli)
                {
                    sReturn += "<titolo id=\"" + Util.ConvertWithANDReplace(dr.id_titolo.ToString()) + "\" > ";
                    sReturn += "<vcode>" + Util.ConvertWithANDReplace(dr.vcode) + "</vcode>";
                    sReturn += "<vname>" + Util.ConvertWithANDReplace(dr.vname) + "</vname>";
                    sReturn += "<vcity>" + Util.ConvertWithANDReplace(dr.vcity) + "</vcity>";
                    sReturn += "<titolo_originale>" + Util.ConvertWithANDReplace(dr.titolo_originale) + "</titolo_originale>";
                    sReturn += "<titolo_editato>" + Util.ConvertWithANDReplace(dr.titolo_editato) + "</titolo_editato>";
                    sReturn += "<datainizio>" + Util.ConvertWithANDReplace(dr.datainizio.ToString()) + "</datainizio>";
                    sReturn += "<datafine>" + Util.ConvertWithANDReplace(dr.datafine.ToString()) + "</datafine>";
                    sReturn += "<numperf>" + Util.ConvertWithANDReplace(dr.numPerf.ToString()) + "</numperf>";
                    sReturn += "<cat>" + Util.ConvertWithANDReplace(dr.cat) + "</cat>";
                    sReturn += "<sottocat>" + Util.ConvertWithANDReplace(dr.sottocat) + "</sottocat>";
                    sReturn += "<descrizione>" + Util.ConvertWithANDReplace(dr.descrizione) + "</descrizione>";
                    sReturn += "<img>" + Util.ConvertWithANDReplace(dr.immagine) + "</img>";
                    sReturn += "<img_mini>" + Util.ConvertWithANDReplace(dr.immagine_mini) + "</img_mini>";
                    sReturn += "<artista>" + Util.ConvertWithANDReplace(dr.artista) + "</artista>";
                    sReturn += "<lat>" + Util.ConvertWithANDReplace(dr.lat) + "</lat>";
                    sReturn += "<lon>" + Util.ConvertWithANDReplace(dr.lon) + "</lon>";
                    sReturn += "<exact>" + Util.ConvertWithANDReplace(dr.EXACT.ToString()) + "</exact>";
                    sReturn += "<fasceOrarie>" + Util.ConvertWithANDReplace(dr.fasceOrarie.ToString()) + "</fasceOrarie>";
                    sReturn += "</titolo>";
                }

                SupportTableAdapter tableAdapterSupport = new SupportTableAdapter();
                DataRow[] dr1 = tableAdapterSupport.GetData().Select("key = 'baseImagePath'  ");
                string _BaseImagePath = dr1[0]["value"].ToString();
                sReturn += "<baseimagepath>" + _BaseImagePath + "</baseimagepath>";
                sReturn += "</Titoli></reply>";

                tableAdapterSupport.Dispose();
                objDTTitoli.Dispose();
                objTATitoli.Dispose();


                //logger.Debug("sReturn: " + sReturn);

                return sReturn;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("AroundMe", "999", ex.Message);
            }
        }

    }
}

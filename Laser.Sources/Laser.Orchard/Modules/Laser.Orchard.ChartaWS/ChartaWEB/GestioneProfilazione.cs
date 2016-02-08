using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using System.Xml;
using ChartaDb.ChartaTableAdapters;
using System.Text;
using System.Data;
using ChartaDb;


namespace ChartaWEB
{
    public class GestioneProfilazione
    {

        private static readonly ILog logger = LogManager.GetLogger("GestioneProfilazione");


        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tipoDevice"></param>
        /// <param name="idProfila"></param>
        /// <param name="citta"></param>
        /// <param name="luogo"></param>
        /// <param name="cattid"></param>
        /// <param name="titolo_artista"></param>
        /// <returns></returns>
        public static string RegistraProfilazione (string token, string tipoDevice, string prod , string idProfila, string citta , string luogo ,string cattid , string titolo_artista)
        {

            try
            {
                //Richiamo la registrazione del Token, cosi se non fosse registrato lo inserisce 
                GestioneDeviceToken.RegistraToken(token, tipoDevice, prod);
                
                int idprofilazione = 0;

                int? idCate = null;
                if (!string.IsNullOrEmpty(cattid))
                {
                    int idCategoria = 0;
                    int.TryParse(cattid, out idCategoria);
                    idCate = idCategoria;
                }
 

                if (!string.IsNullOrEmpty(titolo_artista))
                {
                    titolo_artista = titolo_artista.Replace("%",""); //elimino i % che vengon inseriti automaticamente dall pagina di defualt dopo aver letto il parametro titolo.
                }


                PROFILAZIONETableAdapter pr = new PROFILAZIONETableAdapter();

                // verifico la presenza del parametro idProfila
                if (string.IsNullOrEmpty(idProfila))
                {
                    //E' una nuova registrazione
                    object nreturn = pr.InsertAndReturnIdentity(token, citta, luogo, idCate, titolo_artista, DateTime.Now, null);
                    idprofilazione = int.Parse(nreturn.ToString());

                }
                else
                {
                    //Prelevo id della registrazione 
                    ChartaDb.Charta.PROFILAZIONERow[] dr1 = (ChartaDb.Charta.PROFILAZIONERow[])pr.GetData().Select("id_profilazione = " + idProfila + " and DEVICE_TOKEN = '" + token + "' ");
                    bool found = false;
                    if (dr1.Length == 1)
                    {
                        found = true;
                    }
                    else
                    { 
                        //ricerco per tutti i dati tranne token, potrebbe essere cambiato
                        bool searchByCitta = !string.IsNullOrEmpty(citta);
                        bool searchByLuogo = !string.IsNullOrEmpty(luogo);
                        bool searchByCattId = !(idCate == null);
                        bool searchByTitoloArtista = !string.IsNullOrEmpty(titolo_artista);

                        if (searchByCitta) dr1 = (ChartaDb.Charta.PROFILAZIONERow[])pr.GetData().Select("id_profilazione = " + idProfila + " and vcity = '" + SanitizeSQLString(citta) + "'");
                        else if (searchByLuogo) dr1 = (ChartaDb.Charta.PROFILAZIONERow[])pr.GetData().Select("id_profilazione = " + idProfila + " and luogo = '" + SanitizeSQLString(luogo) + "'");
                        else if (searchByCattId) dr1 = (ChartaDb.Charta.PROFILAZIONERow[])pr.GetData().Select("id_profilazione = " + idProfila + " and cod_categoria = '" + cattid + "'");
                        else if (searchByTitoloArtista) dr1 = (ChartaDb.Charta.PROFILAZIONERow[])pr.GetData().Select("id_profilazione = " + idProfila + " and titoloartista = '" + SanitizeSQLString(titolo_artista) + "'");
                        
                        if (dr1.Length == 1)
                            found = true;
                    }


                    if (found)
                    {
                        ChartaDb.Charta.PROFILAZIONERow row = dr1[0];
                        idprofilazione = row.id_profilazione;
                        row.DATA_AGGIORNAMENTO = DateTime.Now;
                        row.DEVICE_TOKEN = token;
                        if (idCate != null)
                            row.cod_categoria = (int)idCate;
                       
                        row.TitoloArtista = titolo_artista;
                        row.vcity = citta;
                        row.luogo = luogo;
                        pr.Update(row);
                    }
                }
                    


                string sReturn = string.Empty;

                sReturn += "<reply>";
                sReturn += "<Profilazione>" + idprofilazione + "</Profilazione>";
                sReturn += "<Esito>OK</Esito>";
                sReturn += "</reply>";

                pr.Dispose();

                return sReturn;

            }
            catch (Exception ex )
            {
                
                logger.Error(ex);
                return Util.GestioneErrore("RegistraProfilazione", "999", ex.Message);
            }
        }


        public static string SanitizeSQLString(string val)
        {
            return val.Replace("'", "''");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="idProfila"></param>
        /// <returns></returns>
        public static string DeleteProfilazione (string token, string idProfila)
        {
            try
            {
                string sReturn = null;

                PROFILAZIONETableAdapter pr = new PROFILAZIONETableAdapter();

                //Prelevo id della registrazione 
                ChartaDb.Charta.PROFILAZIONERow[] dr1 = (ChartaDb.Charta.PROFILAZIONERow[])pr.GetData().Select("id_profilazione = " + idProfila + " and DEVICE_TOKEN = '" + token + "' ");

                if (dr1.Length == 1)
                {
                   //int ret= pr.Delete(dr1[0].id_profilazione, dr1[0].DEVICE_TOKEN); 
                    int ret = pr.DeleteProfilazione(int.Parse(idProfila), token); 
                }

                sReturn += "<reply>";
                sReturn += "<Esito>OK</Esito>";
                sReturn += "</reply>";

                pr.Dispose();

                return sReturn;

            }
            catch (Exception ex )
            {
                
                logger.Error(ex);
                return Util.GestioneErrore("DeleteProfilazione", "999", ex.Message);
            }
        }
    }
}
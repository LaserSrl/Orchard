using System;
using System.Collections.Generic;
using System.Web;
using ChartaDb.ChartaTableAdapters;
using log4net;

namespace ChartaWEB
{
    public static  class Cat
    {
        private static readonly ILog logger = LogManager.GetLogger("Cat");

        public static string ListaCat ()
        {
            try
            {
                CatTableAdapter objTACat = new CatTableAdapter();
                ChartaDb.Charta.CatDataTable objDTCat = objTACat.GetData();
                int idCatOld = -1;

                string sReturn = "<reply>";
                sReturn += "<Categorie>";

                foreach (ChartaDb.Charta.CatRow dr in objDTCat)
                {
                    if (idCatOld != dr.cod_categoria)
                    {
                        if (idCatOld != -1) sReturn += "</categoria>";

                        sReturn += "<categoria id=\"" +  Util.ConvertWithANDReplace(dr.cod_categoria.ToString()) + "\" > ";
                        sReturn += "<nome>" +  Util.ConvertWithANDReplace(dr.nome_cat) + "</nome>";
                        idCatOld = dr.cod_categoria;
                    }
                    sReturn += "<sottocategoria id=\"" + Util.ConvertWithANDReplace(dr.cod_sottocat.ToString())  + "\"> ";
                    sReturn += "<nome>" + Util.ConvertWithANDReplace(dr.nome_sottocat) + "</nome>";
                    sReturn += "</sottocategoria>";
                }
                sReturn += "</categoria>";

                objDTCat.Dispose();
                objTACat.Dispose();

                sReturn += "</Categorie></reply>";

                return sReturn;
            }
            catch ( Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("Cat", "999", ex.Message); 
            }
        }
    }
}

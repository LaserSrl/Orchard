using System;
using System.Collections.Generic;
using System.Web;
using ChartaDb.ChartaTableAdapters;
using log4net;
using Laser.Orchard.Commons.Services;
using System.Text;

namespace ChartaWEB
{
    public static  class Cat
    {
        private static readonly ILog logger = LogManager.GetLogger("Cat");

        public class Sottocategoria
        {
            public long Id { get; set; }
            public string Nome { get; set; }
            public string Sid { get; set; }
        }

        public class Categoria
        {
            public long Id { get; set; }
            public string Nome { get; set; }
            public string Sid { get; set; }
        }

        private static void serializzaCategoria(int index, StringBuilder sb, Categoria categoria, List<Sottocategoria> lista)
        {
            //if (index > 0)
            //{
            //    sb.Append("},{");
            //}
            //// serializza la categoria
            //var dumper = new ObjectDumper(10, null, false, true, null);
            //var dump = dumper.Dump(categoria, string.Format("[{0}]", index));
            //JsonConverter.ConvertToJSon(dump, sb, false, true);

            //// serializza le sottocategorie
            //sb.Append(",\"l\":[{"); // lista start
            //dumper = new ObjectDumper(10, null, false, true, null);
            //dump = dumper.Dump(lista.ToArray(), "Sottocategorie");
            //JsonConverter.ConvertToJSon(dump, sb, false, true);
            //sb.Append("}]"); // lista end

            // serializza la categoria
            sb.AppendFormat("{{\"n\":\"[{0}]\",\"v\":\"Categoria\",\"m\":[", index);
            sb.AppendFormat("{{\"n\":\"Nome\",\"v\":{0}}}", Util.EncodeForJson(categoria.Nome));
            sb.AppendFormat(",{{\"n\":\"Sid\",\"v\":{0}}}", Util.EncodeForJson(categoria.Sid));
            sb.AppendFormat(",{{\"n\":\"Id\",\"v\":{0}}}", categoria.Id);

            // serializza le sottocategorie come attributi multipli
            sb.Append(",{\"n\":\"Sottocategorie\",\"v\":\"Sottocategoria[]\",\"m\":[");
            for (int idx = 0; idx < lista.Count; idx++)
            {
                sb.AppendFormat("{{\"n\":\"[{0}]\",\"v\":\"Sottocategoria\",\"m\":[", idx);
                sb.AppendFormat("{{\"n\":\"Nome\",\"v\":{0}}}", Util.EncodeForJson(lista[idx].Nome));
                sb.AppendFormat(",{{\"n\":\"Sid\",\"v\":{0}}}", Util.EncodeForJson(lista[idx].Sid));
                sb.AppendFormat(",{{\"n\":\"Id\",\"v\":{0}}}", lista[idx].Id);
                sb.Append("]}");
            }
            sb.Append("]}]}");
        }

        public static string ListaCat ()
        {
            try
            {
                //CatTableAdapter objTACat = new CatTableAdapter();
                //ChartaDb.Charta.CatDataTable objDTCat = objTACat.GetData();
                //int idCatOld = -1;

                //string sReturn = "<reply>";
                //sReturn += "<Categorie>";

                //foreach (ChartaDb.Charta.CatRow dr in objDTCat)
                //{
                //    if (idCatOld != dr.cod_categoria)
                //    {
                //        if (idCatOld != -1) sReturn += "</categoria>";

                //        sReturn += "<categoria id=\"" +  Util.ConvertWithANDReplace(dr.cod_categoria.ToString()) + "\" > ";
                //        sReturn += "<nome>" +  Util.ConvertWithANDReplace(dr.nome_cat) + "</nome>";
                //        idCatOld = dr.cod_categoria;
                //    }
                //    sReturn += "<sottocategoria id=\"" + Util.ConvertWithANDReplace(dr.cod_sottocat.ToString())  + "\"> ";
                //    sReturn += "<nome>" + Util.ConvertWithANDReplace(dr.nome_sottocat) + "</nome>";
                //    sReturn += "</sottocategoria>";
                //}
                //sReturn += "</categoria>";

                //objDTCat.Dispose();
                //objTACat.Dispose();

                //sReturn += "</Categorie></reply>";

                //*************************************************
                int index = 0;
                List<Sottocategoria> lista = null;
                Categoria categoria = null;
                Sottocategoria sottocategoria = null;
                var sb = new StringBuilder();
                sb.Append("{\"m\":[{\"n\":\"Reply\", \"v\":\"Reply\"}]"); // json start
                sb.Append(",\"l\":[{ \"n\":\"Categorie\",\"v\":\"Categoria[]\",\"m\":["); // lista start
                using (CatTableAdapter objTACat = new CatTableAdapter())
                {
                    using (ChartaDb.Charta.CatDataTable objDTCat = objTACat.GetData())
                    {
                        int idCatOld = -1;
                        foreach (ChartaDb.Charta.CatRow dr in objDTCat)
                        {
                            if (idCatOld != dr.cod_categoria)
                            {
                                if (idCatOld != -1)
                                {
                                    // serializza la categoria precedente
                                    serializzaCategoria(index, sb, categoria, lista);
                                    index++;
                                }

                                categoria = new Categoria();
                                categoria.Id = dr.cod_categoria;
                                categoria.Sid = "Categoria-" + dr.cod_categoria;
                                categoria.Nome = dr.nome_cat;
                                idCatOld = dr.cod_categoria;
                                lista = new List<Sottocategoria>();
                            }
                            sottocategoria = new Sottocategoria();
                            sottocategoria.Id = dr.cod_sottocat;
                            sottocategoria.Sid = "Sottocategoria-" + dr.cod_categoria + "-" + dr.cod_sottocat;
                            sottocategoria.Nome = dr.nome_sottocat;
                            lista.Add(sottocategoria);
                        }
                        // serializza l'ultima categoria
                        serializzaCategoria(index, sb, categoria, lista);
                    }
                }
                sb.Append("]}]"); // lista end
                sb.Append("}"); // json end

                string sReturn = sb.ToString().Replace("}{", "},{");
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

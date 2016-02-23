using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using ChartaDb.ChartaTableAdapters;
using System.Globalization;
using Laser.Orchard.ChartaWS.ChartaWEB;
using Laser.Orchard.Commons.Services;
using System.Text;

namespace ChartaWEB
{
    public static class Spettacoli
    {
        private static readonly ILog logger = LogManager.GetLogger("Spettacoli");

        public static string ListaSpettacoli  (string  pIdTitolo)
        {
            try
            {
                //SpettacoliTableAdapter  objTASpett = new SpettacoliTableAdapter ();
                //ChartaDb.Charta.SpettacoliDataTable  objDtSpett;

                //if (string.IsNullOrEmpty(pIdTitolo))
                //{
                //    objDtSpett = objTASpett.GetData(null);
                   
                //}
                //else
                //{
                //    objDtSpett = objTASpett.GetData(int.Parse(pIdTitolo));
                //}

                //string sReturn = "<reply>";
                //sReturn += "<Spettacoli>";
                //IFormatProvider culture = System.Globalization.CultureInfo.CurrentCulture;
                

                //foreach (ChartaDb.Charta.SpettacoliRow  dr in objDtSpett)
                //{
                     
                //    DateTime dateSpettacolo = DateTime.ParseExact(dr.date, "yyyyMMdd", culture);
                //    DateTime timeSpettacolo = DateTime.ParseExact(dr.time, "HH.mm", culture);
                //    dateSpettacolo = dateSpettacolo.AddHours(timeSpettacolo.Hour).AddMinutes(timeSpettacolo.Minute);
                  

                //    if (dateSpettacolo >= DateTime.Now)
                //    {
                //        sReturn += "<Spteccolo pcode=\"" + dr.pcode + "\" >";
                //        sReturn += "<vcode>" + dr.vcode + "</vcode>";
                //        sReturn += "<date>" + Util.ConvertWithANDReplace(dr.date.ToString()) + "</date>";
                //        sReturn += "<time>" + Util.ConvertWithANDReplace(dr.time.ToString()) + "</time>";
                //        sReturn += "<title>" + Util.ConvertWithANDReplace(dr.title.ToString()) + "</title>";
                //        sReturn += "<starttime>" + Util.ConvertWithANDReplace(dr.start_time.ToString()) + "</starttime>";
                //        sReturn += "<retired>" + Util.ConvertWithANDReplace(dr.retired.ToString()) + "</retired>";
                //        sReturn += "<stato>" + Util.ConvertWithANDReplace(dr.stato.ToString()) + "</stato>";
                //        sReturn += "</Spteccolo>";
                //    }

                //}
                //objDtSpett.Dispose();
                //objTASpett.Dispose();

                //sReturn += "</Spettacoli></reply>";
                //*****************************************************
                var lista = new List<Spettacolo>();
                Spettacolo spettacolo = null;
                using (SpettacoliTableAdapter objTASpett = new SpettacoliTableAdapter())
                {
                    ChartaDb.Charta.SpettacoliDataTable objDtSpett;

                    if (string.IsNullOrEmpty(pIdTitolo))
                    {
                        objDtSpett = objTASpett.GetData(null);
                    }
                    else
                    {
                        objDtSpett = objTASpett.GetData(int.Parse(pIdTitolo));
                    }
                    try
                    {
                        IFormatProvider culture = System.Globalization.CultureInfo.CurrentCulture;
                        foreach (ChartaDb.Charta.SpettacoliRow dr in objDtSpett)
                        {
                            DateTime dateSpettacolo = DateTime.ParseExact(dr.date, "yyyyMMdd", culture);
                            DateTime timeSpettacolo = DateTime.ParseExact(dr.time, "HH.mm", culture);
                            dateSpettacolo = dateSpettacolo.AddHours(timeSpettacolo.Hour).AddMinutes(timeSpettacolo.Minute);
                            if (dateSpettacolo >= DateTime.Now)
                            {
                                spettacolo = new Spettacolo();
                                spettacolo.PCode = dr.pcode;
                                spettacolo.VCode = dr.vcode;
                                spettacolo.DateAndTime = DateTime.ParseExact(dr.date + dr.time, "yyyyMMddHH.mm", culture);
                                spettacolo.Title = dr.title;
                                spettacolo.StartTime = dr.start_time;
                                spettacolo.Retired = dr.retired;
                                spettacolo.Stato = dr.stato;
                                lista.Add(spettacolo);
                            }
                        }
                    }
                    finally
                    {
                        objDtSpett.Dispose();
                    }
                }

                // serializza il risultato
                var sb = new StringBuilder();
                sb.Append("{\"m\":[{\"n\":\"Reply\",\"v\":\"Reply\"}], \"l\":[{"); // lista start
                var dumper = new ObjectDumper(10, null, false, true, null);
                var dump = dumper.Dump(lista.ToArray(), "Spettacoli");
                JsonConverter.ConvertToJSon(dump, sb, false, true);
                sb.Append("}]}"); // lista end

                string sReturn = sb.ToString().Replace("\t", " ");
                return sReturn;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("Spettacoli", "999", ex.Message); 
            }
        }
    }
    

}

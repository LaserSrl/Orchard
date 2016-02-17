using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Configuration;
using log4net;
using log4net.Config;
using Laser.Orchard.ChartaWS.Models;

namespace ChartaWEB
{
    public class Command
    {
        const string CMDCITYLIST = "CityList";
        const string CMDORGINFOLIST = "OrgInfoList";
        const string CMDLOCINFOLIST = "LocInfoList";
        const string CMDLOCINFOLISTDISTINCT = "LocInfoListDistinct";
        const string CMDCAT = "Cat";
        const string CMDTITOLI = "Titoli";
        const string CMDSPETTACOLI = "Spettacoli";
        const string CMDRICHIESTAPOSTI = "bestseat";
        const string CMDINVIOANAGRAFICA = "customer";
        const string CMDHOTLIST = "HotList";
        const string CMDLASTMINUTE = "LastMinute";
        const string CMDNEWS = "News";
        const string CMDTITOLISHORT = "TitoliShort";
        const string CMDSERVIZIOOK = "ServizioOk";
        const string CMDSTATOTRAN = "StatoTran";
        const string CMDNUMEROSERVIZIO = "NumeroServizio";
        const string CMDREGTOKEN = "RegToken";
        const string CMDAROUNDME = "AroundMe";
        const string CMDPROFILA = "Profila";
        const string CMDDELPROFILA = "DelProfila";
        const string CMDDELREGTOKEN = "DelRegToken";

        string _idorg;
        string _code;
        string _comune;
        string _citta;
        string _provincia;
        string _idtitolo;
        string _quando;
        string _catid;
        string _sottocatid;
        string _queryRequest;
        HttpResponse _Response;
        string _nome;
        string _cognome;
        string _telefono;
        string _idTransazione;
        string _email;
        string _pcode;
        string _data;
        string _titoloartista;
        string _deviceToken;
        string _tipoDevice;
        string _lat;
        string _lon;
        string _idprofila;
        string _luogo;
        string _prod;

        private static readonly ILog logger = LogManager.GetLogger("Default.aspx");
        private readonly ChartaSiteSettingsPart _config;

        #region [ Property ]
        public string Pcode
        {
            get { return _pcode; }
            set { _pcode = value; }
        }
        public string Pdata
        {
            get { return _data; }
            set { _data = value; }
        }
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
        public string IdTransazione
        {
            get { return _idTransazione; }
            set { _idTransazione = value; }
        }
        public string Telefono
        {
            get { return _telefono; }
            set { _telefono = value; }
        }
        public string Cognome
        {
            get { return _cognome; }
            set { _cognome = value; }
        }
        public string Nome
        {
            get { return _nome; }
            set { _nome = value; }
        }
        public string Idorg
        {
            get { return _idorg; }
            set { _idorg = value; }
        }
        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }
        public string Comune
        {
            get { return _comune; }
            set { _comune = value; }
        }
        public string Provincia
        {
            get { return _provincia; }
            set { _provincia = value; }
        }
        public string Idtitolo
        {
            get { return _idtitolo; }
            set { _idtitolo = value; }
        }
        public string Citta
        {
            get { return _citta; }
            set { _citta = value; }
        }
        public string Quando
        {
            get { return _quando; }
            set { _quando = value; }
        }
        public string Catid
        {
            get { return _catid; }
            set { _catid = value; }
        }
        public string Sottocatid
        {
            get { return _sottocatid; }
            set { _sottocatid = value; }
        }
        public string TitoloArtista
        {
            get { return _titoloartista; }
            set { _titoloartista = value; }
        }
        public string QueryRequest
        {
            get { return _queryRequest; }
            set { _queryRequest = value; }
        }
        public HttpResponse Response
        {
            get { return _Response; }
            set { _Response = value; }
        }
        public string DeviceToken
        {
            get { return _deviceToken; }
            set { _deviceToken = value; }
        }
        public string TipoDevice
        {
            get { return _tipoDevice; }
            set { _tipoDevice = value; }
        }
        public string Lat
        {
            get { return _lat; }
            set { _lat = value; }
        }
        public string Lon
        {
            get { return _lon; }
            set { _lon = value; }
        }
        public string Idprofila
        {
            get { return _idprofila; }
            set { _idprofila = value; }
        }
        public string Luogo
        {
            get { return _luogo; }
            set { _luogo = value; }
        }
        public string Prod
        {
            get { return _prod; }
            set { _prod = value; }
        }

        #endregion

        public Command(ChartaSiteSettingsPart config)
        {
            _config = config;
        }

        /// <summary>
        /// Verifica se la stringa di comando è valisa oppure no
        /// </summary>
        /// <param name="pCommand"></param>
        /// <returns></returns>
        public bool VerificaComando(string pCommand)
        {
            if (string.IsNullOrEmpty(pCommand)) return false;

            switch (pCommand)
            {
                case CMDCITYLIST:
                case CMDORGINFOLIST:
                case CMDLOCINFOLIST:
                case CMDLOCINFOLISTDISTINCT:
                case CMDCAT:
                case CMDTITOLI:
                case CMDSPETTACOLI:
                case CMDRICHIESTAPOSTI:
                case CMDINVIOANAGRAFICA:
                case CMDHOTLIST:
                case CMDLASTMINUTE:
                case CMDNEWS:
                case CMDTITOLISHORT:
                case CMDSERVIZIOOK:
                case CMDSTATOTRAN:
                case CMDNUMEROSERVIZIO:
                case CMDAROUNDME:
                case CMDREGTOKEN:
                case CMDDELREGTOKEN:
                case CMDPROFILA:
                case CMDDELPROFILA:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCommand"></param>
        /// <returns></returns>
        public string GestioneComando(string pCommand)
        {
            switch (pCommand)
            {
                case CMDCITYLIST:
                    //http://localhost/Laser.Orchard/charta?cmd=CityList&id=LASER
                    return CityList.ListaCity();

                case CMDORGINFOLIST:
                    //http://localhost/Laser.Orchard/charta?cmd=OrgInfoList&id=LASER   
                    //http://localhost/Laser.Orchard/charta?cmd=OrgInfoList&id=LASER&idOrg=497
                    return OrgInfoList.ListaOrgInfo(_idorg);

                case CMDLOCINFOLIST:
                    //http://localhost/Laser.Orchard/charta?cmd=LocInfoList&id=LASER
                    return LocInfoList.ListaOrgInfo(Code, Comune, Provincia);

                case CMDLOCINFOLISTDISTINCT:
                    //http://localhost/Laser.Orchard/charta?cmd=LocInfoListDistinct&id=LASER
                    return LocInfoList.ListaOrgInfoDistinct(Code, Comune, Provincia);

                case CMDCAT:
                    //http://localhost/Laser.Orchard/charta?cmd=Cat&id=LASER
                    return Cat.ListaCat();

                 //TITOLI -> non viene più chiamata da nuove versioni mobile
                case CMDTITOLI:
                    return TitoliList.ListaTitoli(_idtitolo, _code, _citta, _quando, _catid, _sottocatid, _pcode, _data, _titoloartista);

                case CMDTITOLISHORT:
                    //http://localhost/Laser.Orchard/charta?cmd=TitoliShort&id=LASER
                    return TitoliList.ListaTitoliShort(_idtitolo, _code, _citta, _quando, _catid, _sottocatid, _pcode, _data, _titoloartista);

                case CMDSPETTACOLI:
                    //http://localhost/Laser.Orchard/charta?cmd=Spettacoli&id=LASER
                    return Spettacoli.ListaSpettacoli(_idtitolo);

                case CMDRICHIESTAPOSTI:
                    {
                        //modifica per timeout configurabile
                        string requestOriginal = _queryRequest;
                        int index1 = _queryRequest.IndexOf("timeout=");
                        int index2 = _queryRequest.IndexOf("&macrozone");
                        string subStringRequest = requestOriginal.Substring(index1, (index2 - index1));
                        string requestModified = requestOriginal.Replace(subStringRequest, "timeout=" + _config.Timeout);
                        return GestioneTransazioni.RichiediPosti(_Response, requestModified, _config.RemoteUrl);
                    }
                case CMDINVIOANAGRAFICA:
                    return GestioneTransazioni.InvioAnagrafica(_Response, _queryRequest, _idTransazione, _nome, _cognome, _telefono, _email, _config.RemoteUrl);

                case CMDHOTLIST:
                    //http://localhost/Laser.Orchard/charta?cmd=HotList&id=LASER
                    return HotList.ListaHotList();

                case CMDLASTMINUTE:
                    //http://localhost/Laser.Orchard/charta?cmd=LastMinute&id=LASER
                    return LastMinute.ListaLastMinute();

                case CMDNEWS:
                    //http://localhost/Laser.Orchard/charta?cmd=News&id=LASER
                    return News.ListaNews();

                case CMDSERVIZIOOK:
                    //http://localhost/Laser.Orchard/charta?cmd=ServizioOk&id=LASER
                    return ServizioOK.GetServizioOK();

                case CMDSTATOTRAN:
                    return GestioneTransazioni.StatoTran(_idTransazione);

                case CMDNUMEROSERVIZIO:
                    //http://localhost/Laser.Orchard/charta?cmd=NumeroServizio&id=LASER
                    return ServizioOK.GetNumeroServizio();

                case CMDAROUNDME:
                    //http://localhost:50832/Default.aspx?id=LASER&cmd=AroundMe&lat=45.467&lon=7.867
                    return TitoliList.ListaTitoliAroundMe(_lat, _lon, (double)(_config.MetersAroundMe));

                case CMDREGTOKEN:
                    //http://localhost:50832/Default.aspx?id=LASER&cmd=RegToken&device_token=9656f90ab69e426c7f6d3b9c296a25f92315ef50d1652b2d14ffa45e63ea3f90&tipo_device=android&prod=1
                    return GestioneDeviceToken.RegistraToken(_deviceToken, _tipoDevice, _prod);

                case CMDDELREGTOKEN:
                    //http://localhost:50832/Default.aspx?id=LASER&cmd=DelRegToken&device_token=0bbad7adb5092f48198cb83739c2a128d71b074a1a59051b4547df1f51a7c2f00&tipo_device=android&prod=1
                    return GestioneDeviceToken.DelRegistraToken(_deviceToken, _tipoDevice, _prod);

                case CMDPROFILA:
                    //http://localhost:50832/Default.aspx?id=LASER&cmd=Profila&device_token=9656f90ab69e426c7f6d3b9c296a25f92315ef50d1652b2d14ffa45e63ea3f90&tipo_device=android&prod=1&idprofila=1&Citta=torino&luogo=alcatraz&catid=10&titolo_artista=titolo
                    return GestioneProfilazione.RegistraProfilazione(_deviceToken, _tipoDevice, _prod,  _idprofila, _citta, _luogo, _catid, _titoloartista);  
                    
                case CMDDELPROFILA:
                    //http://localhost:50832/Default.aspx?id=LASER&cmd=DelProfila&device_token=9656f90ab69e426c7f6d3b9c296a25f92315ef50d1652b2d14ffa45e63ea3f90&idprofila=1
                    return GestioneProfilazione.DeleteProfilazione (_deviceToken, _idprofila);
            } 

            return "";
        }
    }
}

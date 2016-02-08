using System;
using System.Collections.Generic;
using System.Web;
using Laser.Orchard.ChartaWS.Models;

namespace ChartaWEB
{
    public class Security
    {
        private readonly ChartaSiteSettingsPart _config;
        int _codError;
        string _ErrorDescr;
        string _codCmd;

        string _pId;
        string _pCmd;
       
        public string Cmd
        {
            get { return _pCmd; }
            set { _pCmd = value; }
        }

        public string Id
        {
            get { return _pId; }
            set { _pId = value; }
        } 

        public int CodError
        {
            get { return _codError; }
        }
       

        public string ErrorDescr
        {
            get { return _ErrorDescr; }
        }

        public string CodCmd
        {
            get { return _codCmd; }
        }

        /// <summary>
        /// Costruttore
        /// </summary>
        public Security (ChartaSiteSettingsPart config)
        {
            _config = config;
        }

        public void VerificaDati ()
        {
            

            Command objCommand = new Command(_config);
            
            if (objCommand.VerificaComando(_pCmd))
            {

                if (_pId.ToUpperInvariant() == _config.ChartaId)
                {
                    _codError = 0;
                    _codCmd = "";
                    _ErrorDescr = "";
                }
                else
                {
                    _codError = -1;
                    _codCmd = "";
                    _ErrorDescr = "Errore caricamento configurazione";
                }
                
            }
            else
            {
                _codError = -2;
                _codCmd = _pCmd;
                _ErrorDescr = "Comando sconosciuto";
                return;
            }
        }
        

    }
}

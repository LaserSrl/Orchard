using Laser.Orchard.FidelityGateway.Models;
using System.Collections.Generic;

namespace Laser.Orchard.FidelityGateway.Services
{
    public interface ISendService
    {
        //invia la richiesta di registrazione di un cliente al provider di fidelity. Incapsula il risultato nel customer.
        //Nel data dell'APIresult deve esserer caricato il customer.
        APIResult<FidelityCustumer> SendCustomerRegistration(FidelitySettingsPart setPart, FidelityCustumer custumer);

        //invia la richiesta per il recupero dell'id di un cliente al provider di fidelity. Incapsula il risultato nel customer.
        //Nel data dell'APIresult deve esserer caricato il customer.
        APIResult<FidelityCustumer> SendGetCustomerId(FidelitySettingsPart setPart, FidelityCustumer custumer);

        //invia la richiesta per il recupero dei dati del cliente. Incapsula il risultato nel customer.
        //Nel data dell'APIresult deve esserer caricato il customer.
        APIResult<FidelityCustumer> SendCustomerDetails(FidelitySettingsPart setPart, FidelityCustumer custumer);

        //invia la richiesta di recupero di tutte le informazioni legate a una campagna, nell'oggeto campaign passato per parametro 
        //dovrebbe sempre essere settato il campo id.
        //nell'Apiresult deve essere caricata quindi la FidelityCampaign con tutti i dati ad essa associata.
        APIResult<FidelityCampaign> SendCampaignData(FidelitySettingsPart setPart, FidelityCampaign campaign);
        
        //invia la richiesta di recupero di tutte le campagne di un determinato negoziante
        //nell'Apiresult deve essere caricata quindi la lista di campaign, non è previsto il caricamento anche dei dettagli di ognuna di esse.
        APIResult<List<FidelityCampaign>> SendCampaignList(FidelitySettingsPart setPart);

        //aggiunge i punti a un determinato cliente su una determinata campagna.
        //ritorna il cliente con i punti aggiornati ?? (ce la fa??) TODO
        APIResult<FidelityCustumer> SendAddPoints(FidelitySettingsPart setPart, FidelityCustumer customer, FidelityCampaign campaign, double points);

        //APIResult SendAddPointsFromAction(FidelitySettingsPart setPart, List<KeyValuePair<string, string>> kvpList);

        //invia richiesta per il ritiro di un determinato premio da un determinato utente.
        APIResult<FidelityReward> SendGiveReward(FidelitySettingsPart setPart, FidelityCustumer custoer, FidelityReward reward, FidelityCampaign campaign);

        //APIResult SendUpdateSocial(FidelitySettingsPart setPart, List<KeyValuePair<string, string>> kvpList);     
   
        //invia la richiesta per il recupero dell'id di una capagna
        //nell'apiResult deve essere caricata la campaign
        APIResult<FidelityCampaign> SendGetCampaignId(FidelitySettingsPart setPart, FidelityCampaign campaign);

        //invia la richiesta per il recupero dell'id del negoziante.
        //Nel data inserice quindi l'id (stringa)
        APIResult<string> SendGetMerchantId(FidelitySettingsPart setPart);
    }
}

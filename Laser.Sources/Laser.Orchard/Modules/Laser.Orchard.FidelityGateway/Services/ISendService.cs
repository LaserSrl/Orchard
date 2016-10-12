using Laser.Orchard.FidelityGateway.Models;
using System.Collections.Generic;
//C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Orchard\Modules\Laser.Orchard.FidelityLoyalzoo
namespace Laser.Orchard.FidelityGateway.Services
{
    public interface ISendService
    {
        //invia la richiesta di registrazione di un cliente al provider di fidelity. Incapsula il risultato nel customer.
        //Nel data dell'APIresult deve esserer caricato il customer.
        APIResult<FidelityCustomer> SendCustomerRegistration(FidelitySettingsPart setPart, FidelityCustomer custumer);

        //invia la richiesta per il recupero dell'id di un cliente al provider di fidelity. Incapsula il risultato nel customer.
        //Nel data dell'APIresult deve esserer caricato il customer.
        string SendGetCustomerSessionId(FidelitySettingsPart setPart, FidelityCustomer custumer);

        //invia la richiesta per il recupero dei dati del cliente. Incapsula il risultato nel customer.
        //Nel data dell'APIresult deve esserer caricato il customer.
        APIResult<FidelityCustomer> SendCustomerDetails(FidelitySettingsPart setPart, FidelityCustomer custumer);

        //invia la richiesta di recupero di tutte le informazioni legate a una campagna, nell'oggeto campaign passato per parametro 
        //dovrebbe sempre essere settato il campo id.
        //nell'Apiresult deve essere caricata quindi la FidelityCampaign con tutti i dati ad essa associata.
        APIResult<FidelityCampaign> SendCampaignData(FidelitySettingsPart setPart, FidelityCampaign campaign);
        
        //invia la richiesta di recupero di tutte le campagne di un determinato negoziante
        //nell'Apiresult deve essere caricata quindi la lista di campaign, non è previsto il caricamento anche dei dettagli di ognuna di esse.
        APIResult<List<FidelityCampaign>> SendCampaignList(FidelitySettingsPart setPart);

        //aggiunge i punti a un determinato cliente su una determinata campagna.
        //ritorna il cliente con i punti aggiornati ?? (ce la fa??) TODO
        APIResult<FidelityCustomer> SendAddPoints(FidelitySettingsPart setPart, FidelityCustomer customer, FidelityCampaign campaign, double points);

        //APIResult SendAddPointsFromAction(FidelitySettingsPart setPart, List<KeyValuePair<string, string>> kvpList);

        //invia richiesta per il ritiro di un determinato premio da un determinato utente.
        APIResult<FidelityReward> SendGiveReward(FidelitySettingsPart setPart, FidelityCustomer custoer, FidelityReward reward, FidelityCampaign campaign);

        //APIResult SendUpdateSocial(FidelitySettingsPart setPart, List<KeyValuePair<string, string>> kvpList);     
   
        //invia la richiesta per il recupero dell'id di una capagna
        //nell'apiResult deve essere caricata la campaign
        APIResult<FidelityCampaign> SendGetCampaignId(FidelitySettingsPart setPart, FidelityCampaign campaign);

        //invia la richiesta per il recupero dell'id del negoziante.
        //Nel data inserice quindi l'id (stringa)
        APIResult<string> SendGetMerchantId(FidelitySettingsPart setPart);
    }
}

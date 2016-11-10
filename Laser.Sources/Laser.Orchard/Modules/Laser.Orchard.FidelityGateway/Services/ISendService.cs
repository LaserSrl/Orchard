using Laser.Orchard.FidelityGateway.Models;
using Orchard;
using System.Collections.Generic;
//C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Orchard\Modules\Laser.Orchard.FidelityLoyalzoo
namespace Laser.Orchard.FidelityGateway.Services
{
    public interface ISendService :IDependency
    {
        /// <summary>
        /// Invia la richiesta di registrazione di un cliente al provider di fidelity.
        /// </summary>
        /// <param name="setPart">Parte con i settings per la comunicazione e i dati del merchant</param>
        /// <param name="custumer">FidelityCustomer con dati settati per la registrazione</param>
        /// <returns>APIResult con incapsulato un FidelityCustomer con almeno l'Id settato e tutti i dati già presenti nel customer param</returns>
        APIResult<FidelityCustomer> SendCustomerRegistration(FidelitySettingsPart setPart, FidelityCustomer custumer, string campaignId);

        /// <summary>
        /// Invia la richiesta per i avere i dati di un cliente.
        /// </summary>
        /// <param name="setPart">Parte con i settings per la comunicazione e i dati del merchant</param>
        /// <param name="custumer">FidelityCustomer con l'id settato per la richiesta </param>
        /// <returns>APIResult con incapsulato un FidelityCustomer con tutti i dati reperiti</returns>
        APIResult<FidelityCustomer> SendCustomerDetails(FidelitySettingsPart setPart, FidelityCustomer custumer);

        /// <summary>
        /// Invia la richiesta per i avere i dati di una campagna.
        /// </summary>
        /// <param name="setPart">Parte con i settings per la comunicazione e i dati del merchant</param>
        /// <param name="custumer">FidelityCampaign con l'id settato per la richiesta </param>
        /// <returns>APIResult con incapsulato un FidelityCampaign con tutti i dati reperiti</returns>
        APIResult<FidelityCampaign> SendCampaignData(FidelitySettingsPart setPart, FidelityCampaign campaign);

        /// <summary>
        /// Invia la richiesta per i avere una lista di tutte le campagne associate al merchant.
        /// </summary>
        /// <param name="setPart">Parte con i settings per la comunicazione e i dati del merchant</param>
        /// <returns>APIResult con incapsulato un elenco di FidelityCampaign con almeno l'Id settato</returns>
        APIResult<IEnumerable<FidelityCampaign>> SendCampaignList(FidelitySettingsPart setPart);

        /// <summary>
        /// Invia la richiesta per aggiungere dei punti a un cliente.
        /// </summary>
        /// <param name="setPart">Parte con i settings per la comunicazione e i dati del merchant</param>
        /// <param name="customer">FidelityCustomer con almeno l'Id settato</param>
        /// <param name="campaign">FidelityCampaign con almeno l'Id settato</param>
        /// <param name="points">Numero di punti da aggiungere</param>
        /// <returns>APIResult con incapsulato il successo/insuccesso dell'operazione</returns>
        APIResult<bool> SendAddPoints(FidelitySettingsPart setPart, FidelityCustomer customer, FidelityCampaign campaign, string points);

        //APIResult SendAddPointsFromAction(FidelitySettingsPart setPart, List<KeyValuePair<string, string>> kvpList);

        /// <summary>
        /// Invia la richiesta per donaer un premio a un cliente, con conseguente diminuzione dei punti.
        /// </summary>
        /// <param name="setPart">Parte con i settings per la comunicazione e i dati del merchant</param>
        /// <param name="customer">FidelityCustomer con almeno l'Id settato</param>
        /// <param name="campaign">FidelityCampaign con almeno l'Id settato</param>
        /// <param name="reward">FidelityReward con almeno l'Id settato</param>
        /// <returns>APIResult con incapsulato il successo/insuccesso dell'operazione</returns>
        APIResult<bool> SendGiveReward(FidelitySettingsPart setPart, FidelityCustomer custoer, FidelityReward reward, FidelityCampaign campaign);

        /// <summary>
        /// Invia la richiesta per il di settings aggiuntive relative ad ogni provider, utilizzato da i servizzi dei moduli specifici dei provider
        /// </summary>
        /// <param name="setPart">Parte con i settings per la comunicazione e i dati del merchant</param>
        /// <returns>APIResult con incapsulato un dizionario di settings</returns>
        APIResult<IDictionary<string, string>> GetOtherSettings(FidelitySettingsPart setPart);

    }
}

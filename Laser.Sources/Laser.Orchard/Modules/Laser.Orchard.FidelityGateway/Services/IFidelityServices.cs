using Laser.Orchard.FidelityGateway.Models;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.FidelityGateway.Services
{
    public interface IFidelityServices : IDependency
    {
        /// <summary>
        /// Crea un account sul provider remoto basandosi sui dati dell'utente orchard autenticato
        /// </summary>
        /// <returns>Id del merchant</returns>
        APIResult<FidelityCustomer> CreateFidelityAccountFromCookie();

        /// <summary>
        /// Crea un account sul provider remoto con i dati passati per parametro
        /// </summary>
        /// <returns>Id del merchant</returns>
        APIResult<FidelityCustomer> CreateFidelityAccount(FidelityUserPart fidelityPart, string username, string email);

        /// <summary>
        /// Richiede tutti i dettagli della fidelity di un utente autenticato in orchard
        /// </summary>
        /// <returns>APIResult con incapsulato il FidelityCustomer con tutti i dati reperiti dal provider</returns>
        APIResult<FidelityCustomer> GetCustomerDetails();

        /// <summary>
        /// Richiede tutti i dettagli di una certa campagna di fidelizzazione
        /// </summary>
        /// <param name="id">Id della campagna per la quale si vogliono i dettagli </param>
        /// <returns>APIResult con incapsulato il FidelityCampaign con tutti i dati reperiti dal provider</returns>
        APIResult<FidelityCampaign> GetCampaignData(string id);

        /// <summary>
        /// Richiede l'aggiunta di punti del cliente autenticato in Orchard su una determinata campagna.
        /// </summary>
        /// <param name="numPoints">numero di punti da aumentare </param>
        /// <param name="campaignId">identificativo della campagna su cui aumentare i punti </param>
        /// <returns>APIResult con incapsulato il successo/insuccesso dell'operazione</returns>  
        APIResult<bool> AddPoints(string numPoints, string campaignId);

        APIResult<bool> AddPointsFromAction(string action);

        /// <summary>
        /// Richiede la donazione di un premio di una certa categoria all'utente autenticato in Orchard
        /// con conseguente decremento dei punti.
        /// </summary>
        /// <param name="rewardId">identificativo del premio da donare</param>
        /// <param name="campaignId">identificativo della campagna su cui è presente il premio da ritirare </param>
        /// <returns>APIResult con incapsulato il successo/insuccesso dell'operazione</returns>
        APIResult<bool> GiveReward(string rewardId, string campaignId);

        APIResult<FidelityCustomer> UpdateSocial(string token, string tokenType); //TODO

        /// <summary>
        /// Richiede la lista di tutte le campagne associate al merchant.
        /// </summary>
        /// <returns>APIResult con incapsulato la lista di tutte le campagne, associate al merchant, in cui almeno l'Id è settato</returns>
        APIResult<IEnumerable<FidelityCampaign>> GetCampaignList();

        /// <summary>
        /// Richiede la lista di tutte le azioni che generano punteggio in una determinata campagna.
        /// </summary>
        /// <returns>APIResult con incapsulato la lista di tutte le azioniincampagna</returns>
        APIResult<IEnumerable<ActionInCampaignRecord>> GetActions();
    }

}

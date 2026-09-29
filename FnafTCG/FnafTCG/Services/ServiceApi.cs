using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using FnafTCG.Modeles;

namespace FnafTCG.Services;

public class ServiceApi
{
    #region Attributs

    private static ServiceApi instance = new ServiceApi();
    private HttpClient client;

    #endregion

    #region Constructeurs

    private ServiceApi()
    {
        client = new HttpClient();
        client.BaseAddress = new Uri(
            "https://api-cartes-g3.fldsio.com/");
        client.Timeout = TimeSpan.FromSeconds(15);
    }

    #endregion

    #region Getter/Setter

    public static ServiceApi Instance
    {
        get { return instance; }
    }

    #endregion

    #region Methodes

    private async Task<string> LireReponseAsync(
        HttpResponseMessage reponse)
    {
        string json = await reponse.Content.ReadAsStringAsync();

        if (!reponse.IsSuccessStatusCode)
        {
            ErreurApi? erreur = null;
            try
            {
                erreur = JsonConvert
                    .DeserializeObject<ErreurApi>(json);
            }
            catch (JsonException)
            {
                // Le serveur peut renvoyer une réponse non JSON.
            }

            if (reponse.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
                throw new Exception(
                    "Connexion refusée ou expirée. Reconnectez-vous.");

            throw new Exception(erreur?.Message ??
                "Le serveur ne peut pas traiter la demande.");
        }

        return json;
    }

    public async Task SeConnecterAsync(
        string identifiant, string motDePasse)
    {
        DemandeConnexion demande = new DemandeConnexion
        {
            Identifiant = identifiant,
            MotDePasse = motDePasse
        };
        string json = JsonConvert.SerializeObject(demande);
        using StringContent contenu = new StringContent(
            json, Encoding.UTF8, "application/json");
        using HttpResponseMessage reponse =
            await client.PostAsync("auth/login", contenu);
        string resultat = await LireReponseAsync(reponse);
        ReponseConnexion? connexion = JsonConvert
            .DeserializeObject<ReponseConnexion>(resultat);

        if (connexion == null ||
            string.IsNullOrWhiteSpace(connexion.JetonAcces))
            throw new Exception("Le jeton est absent.");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", connexion.JetonAcces);
    }

    public async Task<ObservableCollection<CartePossedee>>
        RecupererCartesPossedeesAsync()
    {
        using HttpResponseMessage reponse =
            await client.GetAsync("portfolio");
        string json = await LireReponseAsync(reponse);
        Portefeuille? portefeuille = JsonConvert
            .DeserializeObject<Portefeuille>(json);

        if (portefeuille == null)
            throw new Exception("Le portefeuille est absent.");

        return portefeuille.CartesPossedees;
    }

    public async Task<Offre?> RecupererOffreAsync(
        CartePossedee cartePossedee)
    {
        string adresse = "cards/" +
            cartePossedee.Carte.Identifiant + "/offers";
        using HttpResponseMessage reponse =
            await client.GetAsync(adresse);
        string json = await LireReponseAsync(reponse);
        ReponseOffres? resultat = JsonConvert
            .DeserializeObject<ReponseOffres>(json);

        if (resultat == null)
            throw new Exception("Les offres sont absentes.");

        foreach (Offre offre in resultat.Offres)
        {
            if (offre.IdentifiantExemplaire ==
                cartePossedee.Identifiant)
                return offre;
        }

        return null;
    }

    public async Task<Offre> CreerOffreAsync(
        string identifiantExemplaire, decimal prix)
    {
        DemandeCreationOffre demande = new DemandeCreationOffre
        {
            IdentifiantExemplaire = identifiantExemplaire,
            Prix = prix
        };
        string json = JsonConvert.SerializeObject(demande);
        using StringContent contenu = new StringContent(
            json, Encoding.UTF8, "application/json");
        using HttpResponseMessage reponse =
            await client.PostAsync("offers", contenu);
        string resultat = await LireReponseAsync(reponse);
        Offre? offre = JsonConvert
            .DeserializeObject<Offre>(resultat);

        if (offre == null)
            throw new Exception("L'offre créée est absente.");

        return offre;
    }

    public async Task<Offre> ModifierOffreAsync(
        string identifiantOffre, decimal prix)
    {
        DemandeModificationOffre demande =
            new DemandeModificationOffre { Prix = prix };
        string json = JsonConvert.SerializeObject(demande);
        using StringContent contenu = new StringContent(
            json, Encoding.UTF8, "application/json");
        using HttpRequestMessage requete = new HttpRequestMessage(
            HttpMethod.Patch, "offers/" + identifiantOffre);
        requete.Content = contenu;
        using HttpResponseMessage reponse =
            await client.SendAsync(requete);
        string resultat = await LireReponseAsync(reponse);
        Offre? offre = JsonConvert
            .DeserializeObject<Offre>(resultat);

        if (offre == null)
            throw new Exception("L'offre modifiée est absente.");

        return offre;
    }

    public async Task SupprimerOffreAsync(string identifiantOffre)
    {
        using HttpResponseMessage reponse =
            await client.DeleteAsync("offers/" + identifiantOffre);
        await LireReponseAsync(reponse);
    }

    #endregion
}

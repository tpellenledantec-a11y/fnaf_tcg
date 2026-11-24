using System.Collections.ObjectModel;
using FnafTCG.Modeles;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace FnafTCG.Services;

public class ServiceApi
{
    #region Attributs & Singleton

    private static readonly ServiceApi _instance = new ServiceApi();
    private readonly HttpClient _client;

    public static ServiceApi Instance => _instance;

    private ServiceApi() : this(new HttpClient())
    {
    }

    internal ServiceApi(HttpClient client)
    {
        _client = client;
        _client.BaseAddress ??= new Uri("https://api-cartes-g3.fldsio.com/");
        _client.Timeout = TimeSpan.FromSeconds(15);
    }

    #endregion

    #region Configuration

    public void SetAuthToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    #endregion

    #region Méthodes HTTP Générales (Génériques)

    // GET Générique
    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        using HttpResponseMessage response = await _client.GetAsync(endpoint);
        string json = await ReadResponseOrThrowAsync(response);
        return JsonConvert.DeserializeObject<T>(json);
    }

    // POST Générique
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        string jsonBody = JsonConvert.SerializeObject(data);
        using StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await _client.PostAsync(endpoint, content);
        string json = await ReadResponseOrThrowAsync(response);
        return JsonConvert.DeserializeObject<TResponse>(json);
    }

    // PATCH Générique
    protected async Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        string jsonBody = JsonConvert.SerializeObject(data);
        using StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
        {
            Content = content
        };

        using HttpResponseMessage response = await _client.SendAsync(request);
        string json = await ReadResponseOrThrowAsync(response);
        return JsonConvert.DeserializeObject<TResponse>(json);
    }

    // DELETE Générique
    protected async Task DeleteAsync(string endpoint)
    {
        using HttpResponseMessage response = await _client.DeleteAsync(endpoint);
        await ReadResponseOrThrowAsync(response);
    }

    #endregion

    #region Gestion centralisée des erreurs

    private async Task<string> ReadResponseOrThrowAsync(HttpResponseMessage reponse)
    {
        string json = await reponse.Content.ReadAsStringAsync();

        if (!reponse.IsSuccessStatusCode)
        {
            ErreurApi? erreur = null;
            try
            {
                erreur = JsonConvert.DeserializeObject<ErreurApi>(json);
            }
            catch (JsonException)
            {
                // Le serveur peut renvoyer une réponse non JSON
            }

            if (reponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("Connexion refusée ou expirée. Reconnectez-vous.");

            throw new Exception(erreur?.Message ?? "Le serveur ne peut pas traiter la demande.");
        }

        return json;
    }

    #endregion

    #region Méthodes Métiers (Utilisent les méthodes génériques)

    public async Task<ReponseConnexion?> SeConnecterAsync(string identifiant, string motDePasse)
    {
        var demande = new DemandeConnexion
        {
            Identifiant = identifiant,
            MotDePasse = motDePasse
        };

        var resultat = await PostAsync<DemandeConnexion, ReponseConnexion>("auth/login", demande);

        if (resultat == null || string.IsNullOrWhiteSpace(resultat.JetonAcces))
            throw new Exception("Le jeton est absent.");

        SetAuthToken(resultat.JetonAcces);
        return resultat;
    }

    public async Task<Portefeuille?> RecupererPortefeuilleAsync()
    {
        return await GetAsync<Portefeuille>("portfolio");
    }

    public async Task<ObservableCollection<CartePossedee>> RecupererCartesPossedeesAsync()
    {
        Portefeuille? portefeuille = await RecupererPortefeuilleAsync();
        return portefeuille?.CartesPossedees ??
            throw new Exception("Le portefeuille est absent.");
    }

    public async Task<Offre?> RecupererOffreAsync(CartePossedee cartePossedee)
    {
        ReponseOffres? reponse = await GetAsync<ReponseOffres>(
            $"cards/{cartePossedee.Carte.Identifiant}/offers");
        if (reponse == null)
            throw new Exception("Les offres sont absentes.");

        return reponse.Offres.FirstOrDefault(offre =>
            offre.IdentifiantExemplaire == cartePossedee.Identifiant);
    }

    public async Task<IReadOnlyList<Quote>> RecupererQuotesAsync(string identifiantCarte)
    {
        // Le portefeuille ne contient pas de quote : charger l'historique de la carte.
        ReponseQuotes? reponse = await GetAsync<ReponseQuotes>(
            $"cards/{identifiantCarte}/quotes?limit=120");
        return reponse?.Items ?? throw new Exception("Les cotations sont absentes.");
    }

    public async Task<Offre?> CreerOffreAsync(string identifiantExemplaire, decimal prix)
    {
        var demande = new DemandeCreationOffre
        {
            IdentifiantExemplaire = identifiantExemplaire,
            Prix = prix
        };

        return await PostAsync<DemandeCreationOffre, Offre>("offers", demande);
    }

    public async Task<Offre?> ModifierOffreAsync(string identifiantOffre, decimal prix)
    {
        var demande = new DemandeModificationOffre { Prix = prix };
        return await PatchAsync<DemandeModificationOffre, Offre>($"offers/{identifiantOffre}", demande);
    }

    public async Task SupprimerOffreAsync(string identifiantOffre)
    {
        await DeleteAsync($"offers/{identifiantOffre}");
    }

    #endregion
}

using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Text;
using FnafTCG.Modeles;
using FnafTCG.Services;
using FnafTCG.VuesModeles;
using Newtonsoft.Json;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
int assertions = 0;
void Verifier(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    assertions++;
}

string carteId = "a8018e69-febc-49bb-8d5b-d506759ecb05";
string autreCarteId = "82ac05e0-116d-4296-a824-d1fd18a7fe23";
DateTimeOffset maintenant = DateTimeOffset.UtcNow;
string Reponse(decimal prix, long version = 249) => JsonConvert.SerializeObject(new
{
    cardId = carteId,
    // Ordre volontairement inversé pour vérifier le choix du dernier horodatage.
    items = new[]
    {
        new { cardId = carteId, price = prix, change = -1.25m, currency = "CRD", timestamp = maintenant, version },
        new { cardId = carteId, price = 12m, change = 0.5m, currency = "CRD", timestamp = maintenant.AddSeconds(-10), version = version - 1 }
    }
});
var transport = new FauxServeur { Json = Reponse(17.42m) };
using var client = new HttpClient(transport);
var service = new ServiceApi(client);
var carte = new CartePossedee
{
    Identifiant = "exemplaire-1",
    Carte = new Carte { Identifiant = carteId }
};
var vm = new QuotesVueModele(carte, service);
var notifications = new List<string?>();
vm.PropertyChanged += (_, e) => notifications.Add(e.PropertyName);
Verifier(vm.CurrentPriceText == "—", "Ne pas afficher un prix fictif avant chargement.");
await vm.ChargerQuotesCommand.ExecuteAsync(null);
Verifier(transport.DerniereUri?.PathAndQuery == "/cards/" + carteId + "/quotes?limit=120",
    "Charger les cotations de la carte sélectionnée, pas celles de l'exemplaire.");
Verifier(vm.QuoteCourante?.Price == 17.42m, "Lire price, pas version=249.");
Verifier(vm.QuoteCourante?.Version == 249, "Conserver la version séparément du prix.");
Verifier(vm.CurrentPriceText == "17,42 CRD", "Afficher le prix français et la devise de l'API.");
Verifier(vm.PriceChangeText == "-1,25 %", "Afficher la variation.");
Verifier(vm.Quotes.Count == 2, "Charger l'historique.");
Verifier(vm.Quotes[0].Timestamp < vm.Quotes[1].Timestamp, "Trier le graphique chronologiquement.");
Verifier(notifications.Contains(nameof(vm.CurrentPriceText)), "Notifier le binding du prix.");
Verifier(notifications.Contains(nameof(vm.PriceChangeText)), "Notifier le binding de la variation.");

transport.Json = Reponse(25.99m, 250);
await vm.ChargerQuotesCommand.ExecuteAsync(null);
Verifier(vm.CurrentPriceText == "25,99 CRD", "Une actualisation doit afficher le prix modifié.");
Verifier(vm.QuoteCourante?.Version == 250, "La nouvelle cotation remplace l'ancienne.");
Verifier(!vm.EstOccupe && vm.EstDisponible, "Rétablir les commandes après chargement.");

var autreVm = new QuotesVueModele(new CartePossedee
{
    Carte = new Carte { Identifiant = autreCarteId }
}, service);
transport.Json = Reponse(80m);
await autreVm.ChargerQuotesCommand.ExecuteAsync(null);
Verifier(transport.DerniereUri?.AbsolutePath.Contains(autreCarteId) == true,
    "Changer de produit doit changer l'URL demandée.");
Verifier(autreVm.CurrentPriceText == "80,00 CRD", "Le prix dépend de la réponse du produit.");

transport.Json = "{\"items\":[]}";
await vm.ChargerQuotesCommand.ExecuteAsync(null);
Verifier(vm.QuoteCourante == null && vm.Quotes.Count == 0, "Gérer un historique vide.");
Verifier(vm.Message.Contains("Aucune cotation"), "Expliquer l'absence de données.");

transport.Json = Reponse(99m);
await vm.ChargerQuotesCommand.ExecuteAsync(null);
transport.Statut = HttpStatusCode.ServiceUnavailable;
transport.Json = "{\"message\":\"Service indisponible\"}";
await vm.ChargerQuotesCommand.ExecuteAsync(null);
Verifier(vm.CurrentPriceText == "—", "Ne pas afficher un cours périmé après erreur.");
Verifier(vm.Message == "Service indisponible", "Afficher l'erreur API.");
Verifier(!vm.EstOccupe, "Libérer le chargement même en erreur.");

transport.Statut = HttpStatusCode.OK;
transport.Json = "{\"items\":[{\"price\":55,\"change\":0,\"currency\":\"USD\",\"timestamp\":\""
    + maintenant.ToString("O") + "\",\"version\":249}]}";
await vm.ChargerQuotesCommand.ExecuteAsync(null);
Verifier(vm.CurrentPriceText == "55,00 USD", "Ne pas coder le symbole euro en dur.");
vm.ChangerPeriode(TimeSpan.Zero);
Verifier(vm.Quotes.Count == 0, "Appliquer le filtre temporel à l'historique.");

if (args.Contains("--live"))
{
    using var liveClient = new HttpClient();
    var liveService = new ServiceApi(liveClient);
    foreach (string id in new[] { carteId, autreCarteId })
    {
        IReadOnlyList<Quote> quotes = await liveService.RecupererQuotesAsync(id);
        Verifier(quotes.Count > 0 && quotes.All(quote => quote.Price > 0),
            "Vérification API réelle : cotations présentes et prix positifs.");
        Quote derniere = quotes.OrderBy(quote => quote.Timestamp).Last();
        Console.WriteLine($"API réelle {id} : {derniere.PriceText}, version {derniere.Version}");
    }
}

Console.WriteLine($"{assertions} assertions réussies.");

sealed class FauxServeur : HttpMessageHandler
{
    public string Json { get; set; } = "{}";
    public HttpStatusCode Statut { get; set; } = HttpStatusCode.OK;
    public Uri? DerniereUri { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        DerniereUri = request.RequestUri;
        return Task.FromResult(new HttpResponseMessage(Statut)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });
    }
}

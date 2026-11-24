using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FnafTCG.Modeles;
using FnafTCG.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FnafTCG.VuesModeles;

public partial class QuotesVueModele : ObservableObject
{
    private readonly ServiceApi serviceApi;
    private IReadOnlyList<Quote> historique = Array.Empty<Quote>();
    private TimeSpan periode = TimeSpan.FromDays(1);

    public CartePossedee CartePossedee { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPriceText))]
    [NotifyPropertyChangedFor(nameof(PriceChangeText))]
    [NotifyPropertyChangedFor(nameof(PriceChangeColor))]
    [NotifyPropertyChangedFor(nameof(UpdatedAtText))]
    private Quote? quoteCourante;

    [ObservableProperty]
    private ObservableCollection<Quote> quotes = new();

    [ObservableProperty]
    private string prixSaisi = "";

    [ObservableProperty]
    private string message = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EstDisponible))]
    private bool estOccupe;

    public bool EstDisponible => !EstOccupe;
    public string CurrentPriceText => QuoteCourante?.PriceText ?? "—";
    public string PriceChangeText => QuoteCourante == null ? "—"
        : QuoteCourante.Change.ToString("+0.00;-0.00;0.00", CultureInfo.CurrentCulture) + " %";
    public string PriceChangeColor => QuoteCourante?.Change < 0 ? "#F87171" : "#4ADE80";
    public string UpdatedAtText => QuoteCourante == null ? "Aucune cotation chargée"
        : "Actualisé à " + QuoteCourante.Timestamp.ToLocalTime().ToString("HH:mm:ss");

    public QuotesVueModele(CartePossedee cartePossedee)
        : this(cartePossedee, ServiceApi.Instance)
    {
    }

    internal QuotesVueModele(CartePossedee cartePossedee, ServiceApi serviceApi)
    {
        CartePossedee = cartePossedee;
        this.serviceApi = serviceApi;
    }

    public void ChangerPeriode(TimeSpan nouvellePeriode)
    {
        periode = nouvellePeriode;
        AfficherHistorique();
    }

    private void AfficherHistorique()
    {
        DateTimeOffset debut = DateTimeOffset.UtcNow - periode;
        Quotes = new ObservableCollection<Quote>(historique
            .Where(quote => quote.Timestamp >= debut)
            .OrderBy(quote => quote.Timestamp));
    }

    [RelayCommand]
    private async Task ChargerQuotesAsync()
    {
        if (EstOccupe)
            return;
        EstOccupe = true;
        Message = "";
        try
        {
            historique = await serviceApi.RecupererQuotesAsync(
                CartePossedee.Carte.Identifiant);
            // Ne pas utiliser Version comme prix ni supposer l'ordre du JSON.
            QuoteCourante = historique.OrderBy(quote => quote.Timestamp).LastOrDefault();
            AfficherHistorique();
            if (QuoteCourante == null)
                Message = "Aucune cotation disponible pour cette carte.";
        }
        catch (Exception erreur)
        {
            // Ne pas laisser un ancien prix passer pour un cours à jour.
            historique = Array.Empty<Quote>();
            QuoteCourante = null;
            AfficherHistorique();
            Message = erreur.Message;
        }
        finally
        {
            EstOccupe = false;
        }
    }

    [RelayCommand]
    private async Task CreerOffreAsync()
    {
        if (EstOccupe)
            return;
        EstOccupe = true;
        Message = "";
        try
        {
            string texte = PrixSaisi.Trim().Replace(',', '.');
            if (!decimal.TryParse(texte, NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out decimal prix) || prix <= 0)
                throw new Exception("Saisissez un prix positif.");

            Offre? offre = await serviceApi.CreerOffreAsync(CartePossedee.Identifiant, prix);
            if (offre == null)
                throw new Exception("L'offre créée est absente.");
            Message = $"Offre créée à {offre.Prix:F2} CRD. Le prix de vente est indépendant du cours.";
        }
        catch (Exception erreur)
        {
            Message = erreur.Message;
        }
        finally
        {
            EstOccupe = false;
        }
    }
}

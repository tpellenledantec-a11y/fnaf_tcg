using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FnafTCG.Modeles;
using FnafTCG.Services;
using FnafTCG.Vues;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;

namespace FnafTCG.VuesModeles;

public partial class CartesVueModele : ObservableObject
{
    #region Attributs

    private ServiceApi serviceApi = ServiceApi.Instance;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalCartes))]
    private ObservableCollection<CartePossedee> cartesPossedees
        = new ObservableCollection<CartePossedee>();

    [ObservableProperty]
    private CartePossedee? carteSelectionnee;

    [ObservableProperty]
    private string message = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EstDisponible))]
    private bool estOccupe;

    [ObservableProperty]

    private int nbLegendaire;

    [ObservableProperty]
    private int nbEpic;

    [ObservableProperty]
    private int nbRare;

    [ObservableProperty]
    private int nbCommun;

    [ObservableProperty]
    private decimal valeurTotale;

    [ObservableProperty]
    private double variationPourcentage;

    [ObservableProperty]
    private ObservableCollection<Carte> catalogue = new();
    #endregion

    #region Proprietes

    public bool EstDisponible => !EstOccupe;
    public int TotalCartes => CartesPossedees.Count;
    #endregion

    #region Commandes

    [RelayCommand]
    private async Task ChargerCartesAsync()
    {
        if (EstOccupe)
            return;

        EstOccupe = true;
        Message = "";
        try
        {
            CarteSelectionnee = null;
            CartesPossedees =
                await serviceApi.RecupererCartesPossedeesAsync();
            NbLegendaire = CompterLedandaire();
            NbEpic = CompterEpic();
            NbRare = CompterRare();
            NbCommun = CompterCommun();
            ValeurTotale = CartesPossedees.Sum(c => c.ValeurActuelle);
            if (CartesPossedees.Count == 0)
                Message = "Votre compte ne possède aucune carte.";
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

    [RelayCommand]
    private async Task GererOffreAsync()
    {
        if (EstOccupe)
            return;
        if (CarteSelectionnee == null)
        {
            Message = "Sélectionnez une carte.";
            return;
        }

        EstOccupe = true;
        try
        {
            await Shell.Current.Navigation.PushAsync(
                new OffrePage(CarteSelectionnee));
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

    public static Color CouleurRarete(string? rarete)
    {
        return rarete?.ToUpper() switch
        {
            "RARE" => Color.FromArgb("#3B82F6"),
            "EPIC" => Color.FromArgb("#9B59F6"),
            "LEGENDARY" => Color.FromArgb("#FFD700"),
            _ => Color.FromArgb("#8A8A93")
        };
    }

    private int CompterEpic()
    {
        int total = 0;
        foreach (CartePossedee cartePossedee in CartesPossedees)
        {
            string? rarete = cartePossedee.Carte.Rarete?.ToUpper();
            if (rarete == "EPIC")
                total++;
        }
        return total;
    }
    private int CompterLedandaire()
    {
        int total = 0;
        foreach (CartePossedee cartePossedee in CartesPossedees)
        {
            string? rarete = cartePossedee.Carte.Rarete?.ToUpper();
            if (rarete == "LEGENDARY")
                total++;
        }
        return total;
    }
    private int CompterRare()
    {
        int total = 0;
        foreach (CartePossedee cartePossedee in CartesPossedees)
        {
            string? rarete = cartePossedee.Carte.Rarete?.ToUpper();
            if (rarete == "RARE")
                total++;
        }
        return total;
    }
    private int CompterCommun()
    {
        int total = 0;
        foreach (CartePossedee cartePossedee in CartesPossedees)
        {
            string? rarete = cartePossedee.Carte.Rarete?.ToUpper();
            if (rarete == "COMMON")
                total++;
        }
        return total;
    }
    public async Task ChargerCatalogueAsync()
    {
        List<Carte> cartes = await serviceApi.ObtenirCartesAsync();
        Catalogue = new ObservableCollection<Carte>(cartes);
    }
    #endregion
}
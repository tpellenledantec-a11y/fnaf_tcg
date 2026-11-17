using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FnafTCG.Modeles;
using FnafTCG.Services;
using System.Globalization;

namespace FnafTCG.VuesModeles
{
    public partial class QuotesVueModele : ObservableObject
    {
        private readonly ServiceApi serviceApi = ServiceApi.Instance;

        [ObservableProperty]
        private CartePossedee cartePossedee;

        [ObservableProperty]
        private string prixSaisi = "";

        [ObservableProperty]
        private string message = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EstDisponible))]
        private bool estOccupe;

        public bool EstDisponible => !EstOccupe;

        public QuotesVueModele(CartePossedee cartePossedee)
        {
            CartePossedee = cartePossedee;

            // Pre-fill input price if quote already exists
            if (CartePossedee?.Quote != null)
            {
                PrixSaisi = CartePossedee.Quote.Price.ToString(CultureInfo.CurrentCulture);
            }
        }

        private decimal LirePrix()
        {
            string texte = PrixSaisi.Trim().Replace(',', '.');
            bool valide = decimal.TryParse(texte,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out decimal prix);

            if (!valide || prix <= 0)
                throw new Exception("Saisissez un prix positif.");

            return prix;
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
                decimal prix = LirePrix();
                await serviceApi.CreerOffreAsync(CartePossedee.Identifiant, prix);
                Message = "L'offre a été créée.";
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
}
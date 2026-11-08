using FnafTCG.VuesModeles;

namespace FnafTCG.Vues;

public partial class QuotePage : ContentPage
{
	public QuotePage()
	{
		InitializeComponent();
        BindingContext = new OffreVueModele();

    }
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        // Le chemin ".." indique au Shell de remonter d'un niveau dans la pile de navigation
        await Shell.Current.GoToAsync("..");
    }
}
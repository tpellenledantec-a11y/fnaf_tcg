using FnafTCG.VuesModeles;

namespace FnafTCG.Vues;

public partial class CatalogueVisiteurPage : ContentPage
{
    public CatalogueVisiteurPage()
    {
        InitializeComponent();
        BindingContext = new CartesVueModele();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        CartesVueModele vueModele = (CartesVueModele)BindingContext;
        await vueModele.ChargerCatalogueAsync();
    }
}
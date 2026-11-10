using FnafTCG.VuesModeles;

namespace FnafTCG.Vues;

public partial class ConnexionPage : ContentPage
{
    #region Constructeurs

    public ConnexionPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = new ConnexionVueModele();
    }

    #endregion

}

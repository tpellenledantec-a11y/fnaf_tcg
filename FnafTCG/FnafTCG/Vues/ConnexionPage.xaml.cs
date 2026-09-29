using FnafTCG.VuesModeles;

namespace FnafTCG.Vues;

public partial class ConnexionPage : ContentPage
{
    #region Constructeurs

    public ConnexionPage()
    {
        InitializeComponent();
        BindingContext = new ConnexionVueModele();
    }

    #endregion

}

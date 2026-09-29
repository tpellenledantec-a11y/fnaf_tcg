using FnafTCG.VuesModeles;

namespace FnafTCG.Vues;

public partial class CartesPage : ContentPage
{
    #region Constructeurs

    public CartesPage()
    {
        InitializeComponent();
        BindingContext = new CartesVueModele();
    }

    #endregion


}

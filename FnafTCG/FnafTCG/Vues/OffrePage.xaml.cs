using FnafTCG.VuesModeles;
using FnafTCG.Modeles;

namespace FnafTCG.Vues;

public partial class OffrePage : ContentPage
{
    #region Constructeurs

    public OffrePage(CartePossedee cartePossedee)
    {
        InitializeComponent();
        BindingContext = new OffreVueModele(cartePossedee);
    }

    #endregion


}

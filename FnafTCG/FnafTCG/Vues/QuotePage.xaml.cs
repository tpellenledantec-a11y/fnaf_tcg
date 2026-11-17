using FnafTCG.Modeles;
using FnafTCG.VuesModeles;

namespace FnafTCG.Vues;

public partial class QuotePage : ContentPage
{
    // Now requires the selected card to be passed when opening this page
    public QuotePage(CartePossedee cartePossedee)
    {
        InitializeComponent();
        BindingContext = new QuotesVueModele(cartePossedee);
    }

    private void FilterButton_Clicked(object sender, EventArgs e)
    {
        // Reset all filter buttons to default style
        BtnFilter24h.Style = (Style)Resources["TimeFilterButtonStyle"];
        BtnFilter7d.Style = (Style)Resources["TimeFilterButtonStyle"];
        BtnFilter30d.Style = (Style)Resources["TimeFilterButtonStyle"];
        BtnFilter1y.Style = (Style)Resources["TimeFilterButtonStyle"];

        // Set the clicked button to active style
        if (sender is Button clickedButton)
        {
            clickedButton.Style = (Style)Resources["ActiveTimeFilterButtonStyle"];

            // TODO: Call your API here based on which button was clicked if needed
        }
    }
}
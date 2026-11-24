using FnafTCG.Modeles;
using FnafTCG.VuesModeles;
using System.ComponentModel;

namespace FnafTCG.Vues;

public partial class QuotePage : ContentPage
{
    private readonly QuotesVueModele vueModele;
    private readonly QuoteChartDrawable graphique = new();
    private IDispatcherTimer? timer;
    private bool estVisible;

    public QuotePage(CartePossedee cartePossedee)
    {
        InitializeComponent();
        vueModele = new QuotesVueModele(cartePossedee);
        BindingContext = vueModele;
        PriceChart.Drawable = graphique;
        vueModele.PropertyChanged += VueModele_PropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        estVisible = true;
        await vueModele.ChargerQuotesCommand.ExecuteAsync(null);
        if (!estVisible)
            return;

        if (timer == null)
        {
            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += async (_, _) =>
            {
                if (estVisible && !vueModele.EstOccupe)
                    await vueModele.ChargerQuotesCommand.ExecuteAsync(null);
            };
        }
        timer.Start();
    }

    protected override void OnDisappearing()
    {
        estVisible = false;
        timer?.Stop();
        base.OnDisappearing();
    }

    private void VueModele_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(QuotesVueModele.Quotes))
            return;

        graphique.Quotes = vueModele.Quotes.ToArray();
        PriceChart.Invalidate();
        Label[] axes = { AxisLabel0, AxisLabel1, AxisLabel2, AxisLabel3, AxisLabel4 };
        for (int i = 0; i < axes.Length; i++)
        {
            axes[i].Text = graphique.Quotes.Count == 0 ? ""
                : graphique.Quotes[(graphique.Quotes.Count - 1) * i / 4]
                    .Timestamp.ToLocalTime().ToString("HH:mm");
        }
    }

    private void FilterButton_Clicked(object? sender, EventArgs e)
    {
        foreach (Button bouton in new[] { BtnFilter24h, BtnFilter7d, BtnFilter30d, BtnFilter1y })
            bouton.Style = (Style)Resources["TimeFilterButtonStyle"];

        if (sender is Button clickedButton)
        {
            clickedButton.Style = (Style)Resources["ActiveTimeFilterButtonStyle"];
            int jours = clickedButton.CommandParameter?.ToString() switch
            {
                "7d" => 7,
                "30d" => 30,
                "1y" => 365,
                _ => 1
            };
            vueModele.ChangerPeriode(TimeSpan.FromDays(jours));
        }
    }

    private async void BtnBack_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void NavItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter?.ToString() == "Collection")
        {
            CartesPage? collection = Navigation.NavigationStack.OfType<CartesPage>().LastOrDefault();
            if (collection == null)
                await Navigation.PushAsync(new CartesPage());
            else
                while (Navigation.NavigationStack.Last() != collection)
                    await Navigation.PopAsync();
        }
        else if (e.Parameter?.ToString() == "Terminal")
        {
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync("Navigation", "Cet écran n'est pas encore disponible.", "OK");
        }
    }
}

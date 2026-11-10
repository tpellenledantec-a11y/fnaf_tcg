using FnafTCG.Vues;

namespace FnafTCG;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        ConnexionPage connexionPage = new ConnexionPage();

        NavigationPage navigationPage =
            new NavigationPage(connexionPage);

        return new Window(navigationPage);
    }
}
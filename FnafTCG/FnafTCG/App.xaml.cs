using FnafTCG.Vues;
using Microsoft.Extensions.DependencyInjection;

namespace FnafTCG
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new ConnexionPage()));
        }
    }
}
using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using Microsoft.Maui.Platform;
using System.Reactive.Linq;
using System.Text.Json;
using TestMauiApp.FileSearch;
using TestMauiApp.Views;

namespace TestMauiApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        
        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new AppShell());

            Window secondWindow = new Window(new PopupPage());
            Application.Current?.OpenWindow(secondWindow);


            const int newWidth = 400;
            const int newHeight = 300;

            window.Width = newWidth;
            window.Height = newHeight;
            AppPaths.setup();
            return new Window(new AppShell());

        }
    }
}

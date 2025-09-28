using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TestMauiApp.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Fix: Get the main Window instance and pass it to GlobalHotkey.InitAndRegister
            
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var window = Application.Windows[0].Handler.PlatformView as Microsoft.UI.Xaml.Window;
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            GlobalHotkey.InitAndRegister(window, onHotkey: () =>
            {
                appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            });


            // Hide title bar completely (no X, no minimize/maximize)
            //appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);

            // OR if you just want a borderless window (resizable but no system buttons):
            appWindow.SetPresenter(AppWindowPresenterKind.Default);
            

        }
        

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }

}

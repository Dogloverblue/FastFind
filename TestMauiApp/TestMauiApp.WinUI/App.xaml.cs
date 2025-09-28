using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Reactive.Linq;
using Windows.Storage;
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
        public static AppWindow win;
        public static HotKeyManager _hotKeyManager;
        IDisposable _shift1;
        IDisposable _shift2;
        IDisposable _subscription;

       

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

        

            var window = Application.Windows[0].Handler.PlatformView as Microsoft.UI.Xaml.Window;
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            win = appWindow;

            _hotKeyManager = new HotKeyManager();
            _shift1 = _hotKeyManager.Register(VirtualKeyCode.KEY_A, Modifiers.Alt);
            _shift2 = _hotKeyManager.Register(VirtualKeyCode.VK_ESCAPE, Modifiers.Alt);
            
            int s = 0;
            _subscription = _hotKeyManager.HotKeyPressed
              .ObserveOn(SynchronizationContext.Current)
              .Subscribe(hotKey =>
              {

                  if (hotKey.Id == 0)
                  {

                  appWindow.Show();
                  } else
                  {
                      appWindow.Hide();
                  }
              });

            appWindow.Hide();
            // Replace this line:
            // appWindow.Resize(2);

            // With the following, using a valid SizeInt32 value:
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1200, Height = 700 });
            


            // appWindow.Hide();



            // Hide title bar completely (no X, no minimize/maximize)
            //appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);

            // OR if you just want a borderless window (resizable but no system buttons):
            appWindow.SetPresenter(AppWindowPresenterKind.Default);
            

        }
        

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }

}

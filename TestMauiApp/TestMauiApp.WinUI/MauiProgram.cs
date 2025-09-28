using Microsoft.Maui.Controls;
using Microsoft.Maui.LifecycleEvents;

namespace TestMauiApp.WinUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseSharedMauiApp()
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    events.AddWindows(windowsLifecycleBuilder =>
                    {
                        windowsLifecycleBuilder.OnClosed((window, args) =>
                        {
                            System.Diagnostics.Debug.WriteLine("Windows window closed");
                            App._hotKeyManager.Dispose();
                            Application.Current.Quit();
                        });
                    });
#endif
                    // Other platform-specific lifecycle events can be added here
                });



            return builder.Build();
        }
    }
}



public class SquareContainer : ContentView
{
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        // Keep it square based on the allocated width.
        if (width > 0 && Math.Abs(HeightRequest - width) > 0.5)
            HeightRequest = width;
    }
}


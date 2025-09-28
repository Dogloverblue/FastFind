using Microsoft.Maui.Controls;

namespace TestMauiApp.WinUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseSharedMauiApp();
                 

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


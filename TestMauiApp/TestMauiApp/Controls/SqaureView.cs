using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

namespace TestMauiApp.Controls
{
    // Wrap any content and keep it square
    public class SquareView : ContentView
    {
        public SquareView()
        {
            SizeChanged += (_, __) =>
            {
                // When width changes (due to column width), match height
                if (Width > 0 && double.IsFinite(Width))
                {
                    HeightRequest = Width;
                }
            };
        }
    }
}
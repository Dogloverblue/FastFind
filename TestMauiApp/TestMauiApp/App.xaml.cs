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
            const int newWidth = 400;
            const int newHeight = 300;

            window.Width = newWidth;
            window.Height = newHeight;
            return new Window(new AppShell());
        }
    }
}

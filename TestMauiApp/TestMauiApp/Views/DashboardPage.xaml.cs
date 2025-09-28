using TestMauiApp.ViewModels;

namespace TestMauiApp.Views
{


    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
           
            BindingContext = new DashboardViewModel();
           
            
        }

        async void OnBrowseClicked(object sender, EventArgs e)
        {
            Console.WriteLine("fds");
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose a file"
            });

            if (result != null)
                PathEntry.Text = result.FullPath; // put the chosen path in the textbox
        }

        void OnAddPathClicked(object sender, EventArgs e)
        {
            
            var path = PathEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(path)) return;

            if (BindingContext is DashboardViewModel vm &&
                !vm.FilePaths.Contains(path))
            {
                vm.FilePaths.Add(path);
                PathEntry.Text = string.Empty; // clear after add
            }
        }

    }
}
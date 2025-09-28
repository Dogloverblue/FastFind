using FileSearch;
using Microsoft.Maui.Controls.Shapes;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using TestMauiApp.FileSearch;
using TestMauiApp.ViewModels;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestMauiApp.Views
{


    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
            DashboardViewModel model = new DashboardViewModel();
            BindingContext = model;


            ConfigData config;
            List<string> files = new();
            string jsonPath = AppPaths.SettingsFile;
            if (File.Exists(jsonPath))
            {
                string jsonStringRead = File.ReadAllText(jsonPath);
                config = JsonSerializer.Deserialize<ConfigData>(jsonStringRead);
                files = config.FilePaths;
            }


            foreach (string path in files) {
                model.FilePaths.Add(path);
            }

            foreach (string folderPath in files)
            {
                FileSearch.Program.indexer.addPathToIndex(folderPath);
            }
            _ = Task.Run(async () =>
            {
                await FileSearch.Program.indexer.updateIndex();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var matches = SearchIndex.SearchBestMatches("file", 12);
                    foreach (var match in matches)
                    {
                        var imagePath = "fileicon.png";
                        var imageType = match.FileType;
                        if (imageType.Contains("image"))
                        {
                            imagePath = match.Path;
                        }
                        model.Cards.Add(new CardModel
                        {
                            Title = match.Name,
                            Subtitle = "Some details",
                            ImagePath = imagePath
                        });
                    }
                });
            });

            // Items is name of a collectionView
            // Add every from files to Items





        }

        async void OnBrowseClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose any file in the folder"
            });

            if (result != null)
            {
                var folderPath = System.IO.Path.GetDirectoryName(result.FullPath);
                PathEntry.Text = folderPath;
            }
        }


        static string value = "Amongs";
        void OnAddPathClicked(object sender, EventArgs e)
        {
            
            var path = PathEntry.Text?.Trim();
            ConfigData config;
            if (string.IsNullOrWhiteSpace(path)) return;
            string jsonPath = AppPaths.SettingsFile;
            if (File.Exists(jsonPath))
            {
            string jsonStringRead = File.ReadAllText(jsonPath);
                config = JsonSerializer.Deserialize<ConfigData>(jsonStringRead);
            } else
            {
                config = new ConfigData();
                Directory.CreateDirectory(AppPaths.LocalBase);
            }

            // Convert into a strongly typed object

            ((List<string>)config.FilePaths).Add(path);
            
            // Convert object to JSON string with indentation
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(config, options);

            Trace.WriteLine("BBB");
            Console.WriteLine("DSFSDFSDF");
            value = "111";

            // Write to file


            File.WriteAllText(jsonPath, jsonString);
            value = File.Exists(jsonPath).ToString();
            value = System.IO.Path.GetFullPath(jsonPath);

            Program.indexer.addPathToIndex("path");
            Program.indexer.updateIndex();

            if (BindingContext is DashboardViewModel vm &&
                !vm.FilePaths.Contains(path))
            {
                vm.FilePaths.Add(path);
                PathEntry.Text = string.Empty; // clear after add
            }
        }

    }
}
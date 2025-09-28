using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Input;
using TestMauiApp.FileSearch;
using static Lucene.Net.Queries.Function.ValueSources.MultiFunction;

namespace TestMauiApp.ViewModels;

public partial class DashboardViewModel
{
    public bool Setting1 { get; set; }
    public bool Setting2 { get; set; }
    public bool Setting3 { get; set; }

    public ObservableCollection<CardModel> Cards { get; } = new();



    public ICommand CardTappedCommand => new Command<CardModel>(card =>
    {
        // handle click
        Application.Current?.MainPage?.DisplayAlert("Tapped", card?.Title, "OK");
    });

    public ObservableCollection<string> FilePaths { get; } = new();
    
    public ICommand RemovePathCommand => new Command<string>(path =>
    {
        if (path is null) return;

        FilePaths.Remove(path);
        List<string> files = new();
        string jsonPath = AppPaths.SettingsFile;
        ConfigData config;
        if (File.Exists(jsonPath))
        {
            string jsonStringRead = File.ReadAllText(jsonPath);
            config = JsonSerializer.Deserialize<ConfigData>(jsonStringRead);
            config.FilePaths.Remove(path);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(config, options);

            File.WriteAllText(jsonPath, jsonString);
        }
    });

}

public class CardModel
{
    public string Title { get; set; }
    public string Subtitle { get; set; }

    public String ImagePath { get; set; }
}
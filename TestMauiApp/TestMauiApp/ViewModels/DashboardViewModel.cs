using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TestMauiApp.ViewModels;

public partial class DashboardViewModel
{
    public bool Setting1 { get; set; }
    public bool Setting2 { get; set; }
    public bool Setting3 { get; set; }

    public ObservableCollection<CardModel> Cards { get; } = new(
           Enumerable.Range(1, 12).Select(i => new CardModel
           {
               Title = $"Item {i}",
               Subtitle = "Some details",

               ImagePath = "C:\\Users\\lowen\\Downloads\\sillyGuy.jpg"
           }));


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
    });

}

public class CardModel
{
    public string Title { get; set; }
    public string Subtitle { get; set; }

    public String ImagePath { get; set; }
}
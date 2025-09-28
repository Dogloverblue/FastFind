using FileSearch;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using static Lucene.Net.Util.Fst.Util;

#if WINDOWS
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
#endif

namespace TestMauiApp.Views;

public partial class PopupPage : ContentPage
{
    public ObservableCollection<OptionItem> Items { get; } = new();
    private OptionItem? _selectedItem;
    public OptionItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem == value) return;
            // clear previous
            if (_selectedItem != null) _selectedItem.IsSelected = false;
            _selectedItem = value;
            if (_selectedItem != null) _selectedItem.IsSelected = true;
            OnPropertyChanged();
        }
    }

    private string _centralText = string.Empty;
    public string CentralText
    {
        get => _centralText;
        set { _centralText = value; OnPropertyChanged(); }
    }

    public ICommand ItemTappedCommand { get; }
    static List<string> paths = new List<string>();

    private void CentralEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        Items.Clear();
        paths.Clear();
        List<SearchIndex.BestMatchResult> results = SearchIndex.SearchBestMatches(CentralText, 5);
        foreach (SearchIndex.BestMatchResult result in results)
        {
            Items.Add(new OptionItem(result.Name, "icon1.png"));
            paths.Add(result.Path);
        }
    }

    public static void OpenWithDefaultProgram(string path)
    {
        using Process fileopener = new Process();

        fileopener.StartInfo.FileName = "explorer";
        fileopener.StartInfo.Arguments = "\"" + path + "\"";
        fileopener.Start();
    }
    public PopupPage()
    {
        InitializeComponent();
        BindingContext = this;
        SelectedItem = Items.FirstOrDefault();

        ItemTappedCommand = new Command<OptionItem?>(item =>
        {
            if (item == null) return;
            SelectedItem = item;
            item.Action?.Invoke();
        });

#if WINDOWS
        // Add keyboard accelerators on Windows to handle Up/Down/Enter
        this.Loaded += (_, __) => AttachWindowsKeyboard();
#endif
    }


    private void OptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is OptionItem selected)
        {
            var num = Items.IndexOf(selected);
            OpenWithDefaultProgram(paths[num]);
  
        }
    }
  
    private void OnItemButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is OptionItem item)
        {
            int index = Items.IndexOf(item); // 0–4
            string root = Path.GetDirectoryName(paths[index]);
            Process.Start("explorer.exe", @root);
        }
    }

#if WINDOWS
    private void AttachWindowsKeyboard()
    {
        var handler = this.Handler?.PlatformView as Microsoft.UI.Xaml.FrameworkElement;
        if (handler == null) return;

        // Ensure the element is focusable to receive key events
        handler.IsTabStop = true;
        handler.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);

        handler.KeyDown += OnWindowsKeyDown;
    }

    private void OnWindowsKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (Items.Count == 0) return;

        // Up / Down change selection
        if (e.Key == Windows.System.VirtualKey.Up)
        {
            e.Handled = true;
            MoveSelection(-1);
        }
        else if (e.Key == Windows.System.VirtualKey.Down)
        {
            e.Handled = true;
            MoveSelection(1);
        }
        // Enter -> invoke
        else if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            SelectedItem?.Action?.Invoke();
        }
    }

    private void MoveSelection(int delta)
    {
        var currentIndex = SelectedItem is null ? -1 : Items.IndexOf(SelectedItem);
        var newIndex = currentIndex;

        if (currentIndex < 0)
            newIndex = 0;
        else
            newIndex = Math.Clamp(currentIndex + delta, 0, Items.Count - 1);

        SelectedItem = Items[newIndex];

        // Make sure it’s brought into view
        OptionsList.ScrollTo(SelectedItem, position: ScrollToPosition.MakeVisible);
    }
#endif
}

public class OptionItem : BindableObject
{
    private bool _isSelected;

    public string Title { get; }
    public string Icon { get; }
    public Action? Action { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public OptionItem(string title, string icon, Action? action = null)
    {
        Title = title;
        Icon = icon;
        Action = action;
    }
}

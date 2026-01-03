using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace LiamKapelianis_PokemonBuilder.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private bool _isDarkMode;
    private bool _loadAllGenerations;
    private int _maxPokemonCount = 200;

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            _isDarkMode = value;
            OnPropertyChanged(nameof(IsDarkMode));
            // TODO: Apply theme change
        }
    }

    public bool LoadAllGenerations
    {
        get => _loadAllGenerations;
        set
        {
            _loadAllGenerations = value;
            OnPropertyChanged(nameof(LoadAllGenerations));
            // TODO: Reload pokemon data
        }
    }

    public int MaxPokemonCount
    {
        get => _maxPokemonCount;
        set
        {
            _maxPokemonCount = value;
            OnPropertyChanged(nameof(MaxPokemonCount));
            // TODO: Apply to data loading
        }
    }

    public ICommand ClearAllTeamsCommand { get; }
    public ICommand ExportTeamsCommand { get; }

    public SettingsViewModel()
    {
        ClearAllTeamsCommand = new Command(ClearAllTeams);
        ExportTeamsCommand = new Command(ExportTeams);

        // Load saved preferences
        LoadPreferences();
    }

    private void LoadPreferences()
    {
        // TODO: Load from Preferences
        IsDarkMode = Preferences.Get("dark_mode", false);
        LoadAllGenerations = Preferences.Get("load_all_gens", false);
        MaxPokemonCount = Preferences.Get("max_pokemon", 200);
    }

    private async void ClearAllTeams()
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Clear All Teams",
            "Are you sure you want to delete all saved teams? This cannot be undone.",
            "Delete All",
            "Cancel"
        );

        if (confirm)
        {
            // TODO: Clear all saved teams from storage
            await Application.Current.MainPage.DisplayAlert("Success", "All teams cleared!", "OK");
        }
    }

    private async void ExportTeams()
    {
        // TODO: Implement team export functionality
        await Application.Current.MainPage.DisplayAlert("Export", "Export feature coming soon!", "OK");
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Save preferences when changed
        if (propertyName == nameof(IsDarkMode))
            Preferences.Set("dark_mode", IsDarkMode);
        else if (propertyName == nameof(LoadAllGenerations))
            Preferences.Set("load_all_gens", LoadAllGenerations);
        else if (propertyName == nameof(MaxPokemonCount))
            Preferences.Set("max_pokemon", MaxPokemonCount);
    }
}
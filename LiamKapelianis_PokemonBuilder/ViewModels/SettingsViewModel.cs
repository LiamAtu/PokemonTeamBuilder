using LiamKapelianis_PokemonBuilder.Services;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace LiamKapelianis_PokemonBuilder.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private bool _isDarkMode;
    private int _maxPokemonCount = 200;
    private int _activeTeamIndex;

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            _isDarkMode = value;
            OnPropertyChanged(nameof(IsDarkMode));
            ApplyTheme();
        }
    }

    public int MaxPokemonCount
    {
        get => _maxPokemonCount;
        set
        {
            _maxPokemonCount = value;
            OnPropertyChanged(nameof(MaxPokemonCount));
            Preferences.Set("max_pokemon", value);

            // Notify MainViewModel to reload with new count
            MessagingCenter.Send(this, "ReloadPokemon");
        }
    }

    public int ActiveTeamIndex
    {
        get => _activeTeamIndex;
        set
        {
            _activeTeamIndex = value;
            OnPropertyChanged(nameof(ActiveTeamIndex));
            TeamService.Instance.ActiveTeamNumber = value + 1; // Convert 0-based to 1-based
            UpdateActiveTeamInfo();
        }
    }

    public string ActiveTeamInfo
    {
        get
        {
            var team = TeamService.Instance.CurrentTeam;
            return $"Currently editing: {TeamService.Instance.GetActiveTeamName()} ({team.Count}/6 Pokemon)";
        }
    }

    public ICommand ClearCurrentTeamCommand { get; }
    public ICommand ClearAllTeamsCommand { get; }
    public ICommand ClearCacheCommand { get; }

    public SettingsViewModel()
    {
        ClearCurrentTeamCommand = new Command(ClearCurrentTeam);
        ClearAllTeamsCommand = new Command(ClearAllTeams);
        ClearCacheCommand = new Command(ClearCache);

        // Subscribe to team changes
        TeamService.Instance.TeamChanged += (s, e) => UpdateActiveTeamInfo();
        TeamService.Instance.ActiveTeamChanged += (s, e) => UpdateActiveTeamInfo();

        LoadPreferences();
    }

    private void LoadPreferences()
    {
        IsDarkMode = Preferences.Get("dark_mode", false);
        MaxPokemonCount = Preferences.Get("max_pokemon", 200);

        // Load active team
        int savedTeam = Preferences.Get("active_team", 1);
        ActiveTeamIndex = savedTeam - 1; // Convert 1-based to 0-based
    }

    private void ApplyTheme()
    {
        Preferences.Set("dark_mode", IsDarkMode);

        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }
    }

    private async void ClearCurrentTeam()
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Clear Current Team",
            $"Are you sure you want to clear {TeamService.Instance.GetActiveTeamName()}?",
            "Clear",
            "Cancel"
        );

        if (confirm)
        {
            TeamService.Instance.ClearCurrentTeam();
            await Application.Current.MainPage.DisplayAlert("Success", $"{TeamService.Instance.GetActiveTeamName()} cleared!", "OK");
        }
    }

    private async void ClearAllTeams()
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Clear All Teams",
            "Are you sure you want to delete BOTH teams? This cannot be undone.",
            "Delete All",
            "Cancel"
        );

        if (confirm)
        {
            TeamService.Instance.ClearAllTeams();
            await Application.Current.MainPage.DisplayAlert("Success", "All teams cleared!", "OK");
        }
    }

    private async void ClearCache()
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Clear Cache",
            "This will clear all cached Pokemon data and reload from the internet. This may take a moment.",
            "Clear",
            "Cancel"
        );

        if (confirm)
        {
            // Clear preferences cache
            Preferences.Remove("cached_pokemon_data");

            // Notify MainViewModel to clear and reload
            MessagingCenter.Send(this, "ClearCache");

            await Application.Current.MainPage.DisplayAlert(
                "Cache Cleared",
                "Pokemon data has been cleared and is reloading now.",
                "OK"
            );
        }
    }

    private void UpdateActiveTeamInfo()
    {
        OnPropertyChanged(nameof(ActiveTeamInfo));
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Save active team preference
        if (propertyName == nameof(ActiveTeamIndex))
        {
            Preferences.Set("active_team", ActiveTeamIndex + 1);
        }
    }
}
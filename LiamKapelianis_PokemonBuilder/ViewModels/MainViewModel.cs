using LiamKapelianis_PokemonBuilder.Models;
using LiamKapelianis_PokemonBuilder.Services;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.ComponentModel;

namespace LiamKapelianis_PokemonBuilder.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private static readonly HttpClient _httpClient = new();
    private string _searchText;
    private List<Pokemon> _allPokemon = new();
    private bool _isLoading;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<Pokemon> PokemonList { get; } = new();
    public ObservableCollection<Pokemon> MyTeam => TeamService.Instance.CurrentTeam;
    public ObservableCollection<TypeFilter> TypeFilters { get; } = new();

    public ICommand AddToTeamCommand { get; }
    public ICommand RemoveFromTeamCommand { get; }
    public ICommand NavigateToDetailCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand TypeFilterTappedCommand { get; }

    public string ActiveTeamName => TeamService.Instance.GetActiveTeamName();

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged(nameof(SearchText));
            ApplyFilters();
        }
    }

    public MainViewModel()
    {
        AddToTeamCommand = new Command<Pokemon>(AddToTeam);
        RemoveFromTeamCommand = new Command<Pokemon>(RemoveFromTeam);
        NavigateToDetailCommand = new Command<Pokemon>(NavigateToDetail);
        SearchCommand = new Command(OnSearch);
        TypeFilterTappedCommand = new Command<TypeFilter>(OnTypeFilterTapped);

        // Subscribe to team changes to update UI
        TeamService.Instance.TeamChanged += (s, e) => OnPropertyChanged(nameof(MyTeam));
        TeamService.Instance.ActiveTeamChanged += (s, e) => OnPropertyChanged(nameof(ActiveTeamName));

        // Subscribe to settings changes
        MessagingCenter.Subscribe<SettingsViewModel>(this, "ReloadPokemon", (sender) =>
        {
            _ = ReloadPokemonAsync();
        });

        MessagingCenter.Subscribe<SettingsViewModel>(this, "ClearCache", (sender) =>
        {
            ClearCacheAndReload();
        });

        InitializeTypeFilters();
        _ = LoadPokemonAsync();
    }

    private void InitializeTypeFilters()
    {
        var types = new[] { "fire", "water", "grass", "electric", "psychic", "ice",
                           "dragon", "dark", "fairy", "fighting", "poison", "ground",
                           "rock", "bug", "ghost", "steel", "normal", "flying" };

        foreach (var type in types)
        {
            TypeFilters.Add(new TypeFilter
            {
                TypeName = char.ToUpper(type[0]) + type[1..],
                IsSelected = false,
                BackgroundColor = GetTypeColor(type)
            });
        }
    }

    private void OnTypeFilterTapped(TypeFilter filter)
    {
        if (filter == null) return;

        filter.IsSelected = !filter.IsSelected;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allPokemon.AsEnumerable();

        // Apply text search
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Apply type filters
        var selectedTypes = TypeFilters.Where(f => f.IsSelected).Select(f => f.TypeName.ToLower()).ToList();
        if (selectedTypes.Any())
        {
            filtered = filtered.Where(p =>
                p.Types.Any(t => selectedTypes.Contains(t.ToLower())));
        }

        PokemonList.Clear();
        foreach (var pokemon in filtered)
        {
            PokemonList.Add(pokemon);
        }
    }

    private async Task LoadPokemonAsync()
    {
        if (IsLoading) return;

        IsLoading = true;

        // Get max count from settings
        int maxCount = Preferences.Get("max_pokemon", 200);

        var tasks = Enumerable.Range(1, maxCount).Select(async i =>
        {
            try
            {
                return await _httpClient
                    .GetFromJsonAsync<PokemonResponse>($"https://pokeapi.co/api/v2/pokemon/{i}");
            }
            catch
            {
                return null;
            }
        });

        var results = (await Task.WhenAll(tasks)).Where(r => r != null);

        _allPokemon.Clear();
        foreach (var p in results)
        {
            var firstType = p.types?.FirstOrDefault()?.type?.name ?? "normal";
            var spriteUrl = p.sprites?.front_default;

            var pokemon = new Pokemon
            {
                Name = string.IsNullOrEmpty(p.name) ? "Unknown" : char.ToUpper(p.name[0]) + p.name[1..],
                Sprite = spriteUrl,
                Types = p.types?.Select(t => t.type.name).ToList() ?? new List<string> { "normal" },
                BackgroundColor = GetTypeColor(firstType)
            };

            _allPokemon.Add(pokemon);
        }

        ApplyFilters();
        IsLoading = false;
    }

    public async Task ReloadPokemonAsync()
    {
        _allPokemon.Clear();
        PokemonList.Clear();
        await LoadPokemonAsync();
    }

    private void ClearCacheAndReload()
    {
        _allPokemon.Clear();
        PokemonList.Clear();
        _ = LoadPokemonAsync();
    }

    private void AddToTeam(Pokemon pokemon)
    {
        if (pokemon == null) return;

        if (TeamService.Instance.IsInTeam(pokemon))
        {
            Application.Current?.MainPage?.DisplayAlert("Already Added", $"{pokemon.Name} is already in your team!", "OK");
            return;
        }

        if (TeamService.Instance.AddToTeam(pokemon))
        {
            Application.Current?.MainPage?.DisplayAlert("Added!", $"{pokemon.Name} added to your team!", "OK");
        }
        else
        {
            Application.Current?.MainPage?.DisplayAlert("Team Full", "You can only have 6 Pokemon in your team!", "OK");
        }
    }

    private void RemoveFromTeam(Pokemon pokemon)
    {
        if (pokemon == null) return;

        TeamService.Instance.RemoveFromTeam(pokemon);
    }

    private async void NavigateToDetail(Pokemon pokemon)
    {
        if (pokemon == null) return;

        await Shell.Current.GoToAsync($"PokemonDetailPage",
            new Dictionary<string, object>
            {
                { "Pokemon", pokemon }
            });
    }

    private void OnSearch()
    {
        ApplyFilters();
    }

    private Color GetTypeColor(string type) =>
        (type ?? "").ToLower() switch
        {
            "fire" => Colors.OrangeRed,
            "water" => Colors.DodgerBlue,
            "grass" => Colors.Green,
            "electric" => Colors.Yellow,
            "psychic" => Colors.MediumPurple,
            "ice" => Colors.LightCyan,
            "dragon" => Colors.MediumBlue,
            "dark" => Colors.Gray,
            "fairy" => Colors.Pink,
            "fighting" => Colors.Brown,
            "poison" => Colors.Purple,
            "ground" => Colors.SandyBrown,
            "rock" => Colors.DarkGoldenrod,
            "bug" => Colors.OliveDrab,
            "ghost" => Colors.Indigo,
            "steel" => Colors.LightGray,
            "normal" => Colors.Beige,
            "flying" => Colors.LightBlue,
            _ => Colors.Red
        };

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// TypeFilter model
public class TypeFilter : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private bool _isSelected;

    public string TypeName { get; set; }
    public Color BackgroundColor { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BorderColor)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextColor)));
            }
        }
    }

    public Color BorderColor => IsSelected ? Colors.Black : Colors.Transparent;
    public Color TextColor => IsSelected ? Colors.Black : Colors.White;
}
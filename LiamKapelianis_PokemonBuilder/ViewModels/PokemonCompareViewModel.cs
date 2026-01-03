using LiamKapelianis_PokemonBuilder.Models;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace LiamKapelianis_PokemonBuilder.ViewModels;

public class PokemonCompareViewModel : INotifyPropertyChanged
{
    private static readonly HttpClient _httpClient = new();
    private Pokemon _pokemon1;
    private Pokemon _pokemon2;
    private PokemonStats _stats1;
    private PokemonStats _stats2;

    public event PropertyChangedEventHandler PropertyChanged;

    public Pokemon Pokemon1
    {
        get => _pokemon1;
        set
        {
            _pokemon1 = value;
            OnPropertyChanged(nameof(Pokemon1));
            OnPropertyChanged(nameof(HasPokemon1));
            OnPropertyChanged(nameof(CanCompare));
            if (value != null) _ = LoadStats1Async();
        }
    }

    public Pokemon Pokemon2
    {
        get => _pokemon2;
        set
        {
            _pokemon2 = value;
            OnPropertyChanged(nameof(Pokemon2));
            OnPropertyChanged(nameof(HasPokemon2));
            OnPropertyChanged(nameof(CanCompare));
            if (value != null) _ = LoadStats2Async();
        }
    }

    public PokemonStats Stats1
    {
        get => _stats1;
        set
        {
            _stats1 = value;
            OnPropertyChanged(nameof(Stats1));
            UpdateComparisons();
        }
    }

    public PokemonStats Stats2
    {
        get => _stats2;
        set
        {
            _stats2 = value;
            OnPropertyChanged(nameof(Stats2));
            UpdateComparisons();
        }
    }

    public bool HasPokemon1 => Pokemon1 != null;
    public bool HasPokemon2 => Pokemon2 != null;
    public bool CanCompare => HasPokemon1 && HasPokemon2 && Stats1 != null && Stats2 != null;

    // Progress bars
    public double HP1Progress => Stats1?.HP / 255.0 ?? 0;
    public double HP2Progress => Stats2?.HP / 255.0 ?? 0;
    public double Attack1Progress => Stats1?.Attack / 255.0 ?? 0;
    public double Attack2Progress => Stats2?.Attack / 255.0 ?? 0;
    public double Defense1Progress => Stats1?.Defense / 255.0 ?? 0;
    public double Defense2Progress => Stats2?.Defense / 255.0 ?? 0;
    public double Speed1Progress => Stats1?.Speed / 255.0 ?? 0;
    public double Speed2Progress => Stats2?.Speed / 255.0 ?? 0;

    // Colors for comparison
    public Color HPColor1 => GetComparisonColor(Stats1?.HP ?? 0, Stats2?.HP ?? 0);
    public Color HPColor2 => GetComparisonColor(Stats2?.HP ?? 0, Stats1?.HP ?? 0);
    public Color AttackColor1 => GetComparisonColor(Stats1?.Attack ?? 0, Stats2?.Attack ?? 0);
    public Color AttackColor2 => GetComparisonColor(Stats2?.Attack ?? 0, Stats1?.Attack ?? 0);
    public Color DefenseColor1 => GetComparisonColor(Stats1?.Defense ?? 0, Stats2?.Defense ?? 0);
    public Color DefenseColor2 => GetComparisonColor(Stats2?.Defense ?? 0, Stats1?.Defense ?? 0);
    public Color SpeedColor1 => GetComparisonColor(Stats1?.Speed ?? 0, Stats2?.Speed ?? 0);
    public Color SpeedColor2 => GetComparisonColor(Stats2?.Speed ?? 0, Stats1?.Speed ?? 0);

    // Total stats
    public int TotalStats1 => Stats1 == null ? 0 :
        Stats1.HP + Stats1.Attack + Stats1.Defense + Stats1.SpecialAttack + Stats1.SpecialDefense + Stats1.Speed;
    public int TotalStats2 => Stats2 == null ? 0 :
        Stats2.HP + Stats2.Attack + Stats2.Defense + Stats2.SpecialAttack + Stats2.SpecialDefense + Stats2.Speed;

    public ICommand SelectPokemon1Command { get; }
    public ICommand SelectPokemon2Command { get; }

    public PokemonCompareViewModel()
    {
        SelectPokemon1Command = new Command(SelectPokemon1);
        SelectPokemon2Command = new Command(SelectPokemon2);
    }

    private async void SelectPokemon1()
    {
        string pokemonName = await Application.Current.MainPage.DisplayPromptAsync(
            "Select Pokémon",
            "Enter Pokémon name:",
            "OK",
            "Cancel",
            placeholder: "e.g., Pikachu"
        );

        if (!string.IsNullOrWhiteSpace(pokemonName))
        {
            await LoadPokemon1(pokemonName.Trim().ToLower());
        }
    }

    private async void SelectPokemon2()
    {
        string pokemonName = await Application.Current.MainPage.DisplayPromptAsync(
            "Select Pokémon",
            "Enter Pokémon name:",
            "OK",
            "Cancel",
            placeholder: "e.g., Charizard"
        );

        if (!string.IsNullOrWhiteSpace(pokemonName))
        {
            await LoadPokemon2(pokemonName.Trim().ToLower());
        }
    }

    private async Task LoadPokemon1(string name)
    {
        try
        {
            var url = $"https://pokeapi.co/api/v2/pokemon/{name}";
            var data = await _httpClient.GetFromJsonAsync<PokemonResponse>(url);

            if (data != null)
            {
                Pokemon1 = new Pokemon
                {
                    Name = char.ToUpper(data.name[0]) + data.name[1..],
                    Sprite = data.sprites?.front_default,
                    Types = data.types?.Select(t => t.type.name).ToList() ?? new List<string>(),
                    BackgroundColor = GetTypeColor(data.types?.FirstOrDefault()?.type.name)
                };
            }
        }
        catch
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Could not find Pokémon '{name}'", "OK");
        }
    }

    private async Task LoadPokemon2(string name)
    {
        try
        {
            var url = $"https://pokeapi.co/api/v2/pokemon/{name}";
            var data = await _httpClient.GetFromJsonAsync<PokemonResponse>(url);

            if (data != null)
            {
                Pokemon2 = new Pokemon
                {
                    Name = char.ToUpper(data.name[0]) + data.name[1..],
                    Sprite = data.sprites?.front_default,
                    Types = data.types?.Select(t => t.type.name).ToList() ?? new List<string>(),
                    BackgroundColor = GetTypeColor(data.types?.FirstOrDefault()?.type.name)
                };
            }
        }
        catch
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Could not find Pokémon '{name}'", "OK");
        }
    }

    private async Task LoadStats1Async()
    {
        if (Pokemon1 == null) return;

        try
        {
            var url = $"https://pokeapi.co/api/v2/pokemon/{Pokemon1.Name.ToLower()}";
            var data = await _httpClient.GetFromJsonAsync<PokemonDetailResponse>(url);

            if (data != null)
            {
                Stats1 = new PokemonStats
                {
                    HP = data.stats?.FirstOrDefault(s => s.stat.name == "hp")?.base_stat ?? 0,
                    Attack = data.stats?.FirstOrDefault(s => s.stat.name == "attack")?.base_stat ?? 0,
                    Defense = data.stats?.FirstOrDefault(s => s.stat.name == "defense")?.base_stat ?? 0,
                    SpecialAttack = data.stats?.FirstOrDefault(s => s.stat.name == "special-attack")?.base_stat ?? 0,
                    SpecialDefense = data.stats?.FirstOrDefault(s => s.stat.name == "special-defense")?.base_stat ?? 0,
                    Speed = data.stats?.FirstOrDefault(s => s.stat.name == "speed")?.base_stat ?? 0
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading stats: {ex.Message}");
        }
    }

    private async Task LoadStats2Async()
    {
        if (Pokemon2 == null) return;

        try
        {
            var url = $"https://pokeapi.co/api/v2/pokemon/{Pokemon2.Name.ToLower()}";
            var data = await _httpClient.GetFromJsonAsync<PokemonDetailResponse>(url);

            if (data != null)
            {
                Stats2 = new PokemonStats
                {
                    HP = data.stats?.FirstOrDefault(s => s.stat.name == "hp")?.base_stat ?? 0,
                    Attack = data.stats?.FirstOrDefault(s => s.stat.name == "attack")?.base_stat ?? 0,
                    Defense = data.stats?.FirstOrDefault(s => s.stat.name == "defense")?.base_stat ?? 0,
                    SpecialAttack = data.stats?.FirstOrDefault(s => s.stat.name == "special-attack")?.base_stat ?? 0,
                    SpecialDefense = data.stats?.FirstOrDefault(s => s.stat.name == "special-defense")?.base_stat ?? 0,
                    Speed = data.stats?.FirstOrDefault(s => s.stat.name == "speed")?.base_stat ?? 0
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading stats: {ex.Message}");
        }
    }

    private void UpdateComparisons()
    {
        OnPropertyChanged(nameof(HP1Progress));
        OnPropertyChanged(nameof(HP2Progress));
        OnPropertyChanged(nameof(Attack1Progress));
        OnPropertyChanged(nameof(Attack2Progress));
        OnPropertyChanged(nameof(Defense1Progress));
        OnPropertyChanged(nameof(Defense2Progress));
        OnPropertyChanged(nameof(Speed1Progress));
        OnPropertyChanged(nameof(Speed2Progress));
        OnPropertyChanged(nameof(HPColor1));
        OnPropertyChanged(nameof(HPColor2));
        OnPropertyChanged(nameof(AttackColor1));
        OnPropertyChanged(nameof(AttackColor2));
        OnPropertyChanged(nameof(DefenseColor1));
        OnPropertyChanged(nameof(DefenseColor2));
        OnPropertyChanged(nameof(SpeedColor1));
        OnPropertyChanged(nameof(SpeedColor2));
        OnPropertyChanged(nameof(TotalStats1));
        OnPropertyChanged(nameof(TotalStats2));
        OnPropertyChanged(nameof(CanCompare));
    }

    private Color GetComparisonColor(int stat1, int stat2)
    {
        if (stat1 > stat2) return Colors.Green;
        if (stat1 < stat2) return Colors.Red;
        return Colors.Gray;
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
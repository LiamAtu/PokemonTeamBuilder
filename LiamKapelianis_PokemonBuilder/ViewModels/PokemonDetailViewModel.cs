using LiamKapelianis_PokemonBuilder.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace LiamKapelianis_PokemonBuilder.ViewModels;

[QueryProperty(nameof(Pokemon), "Pokemon")]
public class PokemonDetailViewModel : INotifyPropertyChanged
{
    private static readonly HttpClient _httpClient = new();
    private Pokemon _pokemon;
    private PokemonStats _stats;

    public event PropertyChangedEventHandler PropertyChanged;

    public Pokemon Pokemon
    {
        get => _pokemon;
        set
        {
            _pokemon = value;
            OnPropertyChanged(nameof(Pokemon));
            _ = LoadDetailedDataAsync();
        }
    }

    public PokemonStats Stats
    {
        get => _stats;
        set
        {
            _stats = value;
            OnPropertyChanged(nameof(Stats));
            OnPropertyChanged(nameof(HPProgress));
            OnPropertyChanged(nameof(AttackProgress));
            OnPropertyChanged(nameof(DefenseProgress));
            OnPropertyChanged(nameof(SpAttackProgress));
            OnPropertyChanged(nameof(SpDefenseProgress));
            OnPropertyChanged(nameof(SpeedProgress));
        }
    }

    public ObservableCollection<string> Abilities { get; } = new();

    public double HPProgress => Stats?.HP / 255.0 ?? 0;
    public double AttackProgress => Stats?.Attack / 255.0 ?? 0;
    public double DefenseProgress => Stats?.Defense / 255.0 ?? 0;
    public double SpAttackProgress => Stats?.SpecialAttack / 255.0 ?? 0;
    public double SpDefenseProgress => Stats?.SpecialDefense / 255.0 ?? 0;
    public double SpeedProgress => Stats?.Speed / 255.0 ?? 0;

    public ICommand AddToTeamCommand { get; }

    public PokemonDetailViewModel()
    {
        AddToTeamCommand = new Command(AddToTeam);
    }

    private async Task LoadDetailedDataAsync()
    {
        if (Pokemon == null) return;

        try
        {
            var url = $"https://pokeapi.co/api/v2/pokemon/{Pokemon.Name.ToLower()}";
            var detailedData = await _httpClient.GetFromJsonAsync<PokemonDetailResponse>(url);

            if (detailedData != null)
            {
                Stats = new PokemonStats
                {
                    HP = detailedData.stats?.FirstOrDefault(s => s.stat.name == "hp")?.base_stat ?? 0,
                    Attack = detailedData.stats?.FirstOrDefault(s => s.stat.name == "attack")?.base_stat ?? 0,
                    Defense = detailedData.stats?.FirstOrDefault(s => s.stat.name == "defense")?.base_stat ?? 0,
                    SpecialAttack = detailedData.stats?.FirstOrDefault(s => s.stat.name == "special-attack")?.base_stat ?? 0,
                    SpecialDefense = detailedData.stats?.FirstOrDefault(s => s.stat.name == "special-defense")?.base_stat ?? 0,
                    Speed = detailedData.stats?.FirstOrDefault(s => s.stat.name == "speed")?.base_stat ?? 0
                };

                Abilities.Clear();
                foreach (var ability in detailedData.abilities ?? new List<AbilitySlot>())
                {
                    var abilityName = char.ToUpper(ability.ability.name[0]) + ability.ability.name[1..].Replace("-", " ");
                    Abilities.Add(abilityName);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading pokemon details: {ex.Message}");
        }
    }

    private void AddToTeam()
    {
        Application.Current?.MainPage?.DisplayAlert("Add to Team", $"{Pokemon?.Name} would be added to team (feature coming soon!)", "OK");
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PokemonStats
{
    public int HP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int SpecialAttack { get; set; }
    public int SpecialDefense { get; set; }
    public int Speed { get; set; }
}

public class PokemonDetailResponse
{
    public List<StatSlot> stats { get; set; }
    public List<AbilitySlot> abilities { get; set; }
}

public class StatSlot
{
    public int base_stat { get; set; }
    public StatInfo stat { get; set; }
}

public class StatInfo
{
    public string name { get; set; }
}

public class AbilitySlot
{
    public AbilityInfo ability { get; set; }
}

public class AbilityInfo
{
    public string name { get; set; }
}
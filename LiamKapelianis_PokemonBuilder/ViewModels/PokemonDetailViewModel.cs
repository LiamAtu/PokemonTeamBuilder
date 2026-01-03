using LiamKapelianis_PokemonBuilder.Models;
using LiamKapelianis_PokemonBuilder.Services;
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
    public ObservableCollection<EvolutionStage> EvolutionChain { get; } = new();

    public bool HasEvolutions => EvolutionChain.Count > 1;

    // Progress values (0-1) for ProgressBars - max stat typically 255
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
            // Fetch detailed data from API using pokemon name
            var url = $"https://pokeapi.co/api/v2/pokemon/{Pokemon.Name.ToLower()}";
            var detailedData = await _httpClient.GetFromJsonAsync<PokemonDetailResponse>(url);

            if (detailedData != null)
            {
                // Load stats
                Stats = new PokemonStats
                {
                    HP = detailedData.stats?.FirstOrDefault(s => s.stat.name == "hp")?.base_stat ?? 0,
                    Attack = detailedData.stats?.FirstOrDefault(s => s.stat.name == "attack")?.base_stat ?? 0,
                    Defense = detailedData.stats?.FirstOrDefault(s => s.stat.name == "defense")?.base_stat ?? 0,
                    SpecialAttack = detailedData.stats?.FirstOrDefault(s => s.stat.name == "special-attack")?.base_stat ?? 0,
                    SpecialDefense = detailedData.stats?.FirstOrDefault(s => s.stat.name == "special-defense")?.base_stat ?? 0,
                    Speed = detailedData.stats?.FirstOrDefault(s => s.stat.name == "speed")?.base_stat ?? 0
                };

                // Load abilities
                Abilities.Clear();
                foreach (var ability in detailedData.abilities ?? new List<AbilitySlot>())
                {
                    var abilityName = char.ToUpper(ability.ability.name[0]) + ability.ability.name[1..].Replace("-", " ");
                    Abilities.Add(abilityName);
                }

                // Load evolution chain
                await LoadEvolutionChainAsync(detailedData.species?.url);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading pokemon details: {ex.Message}");
        }
    }

    private async Task LoadEvolutionChainAsync(string speciesUrl)
    {
        if (string.IsNullOrEmpty(speciesUrl)) return;

        try
        {
            var speciesData = await _httpClient.GetFromJsonAsync<PokemonSpecies>(speciesUrl);
            if (speciesData?.evolution_chain?.url == null) return;

            var evolutionData = await _httpClient.GetFromJsonAsync<EvolutionChainResponse>(speciesData.evolution_chain.url);
            if (evolutionData == null) return;

            EvolutionChain.Clear();
            ParseEvolutionChain(evolutionData.chain);
            OnPropertyChanged(nameof(HasEvolutions));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading evolution chain: {ex.Message}");
        }
    }

    private void ParseEvolutionChain(ChainLink chain, string evolutionMethod = "")
    {
        if (chain == null) return;

        var pokemonName = char.ToUpper(chain.species.name[0]) + chain.species.name[1..];
        var spriteUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{GetPokemonIdFromUrl(chain.species.url)}.png";

        EvolutionChain.Add(new EvolutionStage
        {
            Name = pokemonName,
            Sprite = spriteUrl,
            EvolutionMethod = evolutionMethod
        });

        if (chain.evolves_to != null && chain.evolves_to.Count > 0)
        {
            foreach (var evolution in chain.evolves_to)
            {
                var method = evolution.evolution_details?.FirstOrDefault()?.trigger?.name ?? "evolves";
                var level = evolution.evolution_details?.FirstOrDefault()?.min_level;
                var displayMethod = level.HasValue ? $"Lvl {level}" : method;

                ParseEvolutionChain(evolution, displayMethod);
            }
        }
    }

    private int GetPokemonIdFromUrl(string url)
    {
        var parts = url.TrimEnd('/').Split('/');
        return int.TryParse(parts[^1], out var id) ? id : 0;
    }

    private void AddToTeam()
    {
        if (Pokemon == null) return;

        if (TeamService.Instance.AddToTeam(Pokemon))
        {
            Application.Current?.MainPage?.DisplayAlert("Success!", $"{Pokemon.Name} added to your team!", "OK");
        }
        else if (TeamService.Instance.IsInTeam(Pokemon))
        {
            Application.Current?.MainPage?.DisplayAlert("Already Added", $"{Pokemon.Name} is already in your team!", "OK");
        }
        else
        {
            Application.Current?.MainPage?.DisplayAlert("Team Full", "You can only have 6 Pokémon in your team!", "OK");
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Additional models for detail page
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
    public SpeciesRef species { get; set; }
}

public class SpeciesRef
{
    public string url { get; set; }
}

public class PokemonSpecies
{
    public EvolutionChainRef evolution_chain { get; set; }
}

public class EvolutionChainRef
{
    public string url { get; set; }
}

public class EvolutionChainResponse
{
    public ChainLink chain { get; set; }
}

public class ChainLink
{
    public SpeciesInfo species { get; set; }
    public List<ChainLink> evolves_to { get; set; }
    public List<EvolutionDetail> evolution_details { get; set; }
}

public class SpeciesInfo
{
    public string name { get; set; }
    public string url { get; set; }
}

public class EvolutionDetail
{
    public TriggerInfo trigger { get; set; }
    public int? min_level { get; set; }
}

public class TriggerInfo
{
    public string name { get; set; }
}

public class EvolutionStage
{
    public string Name { get; set; }
    public string Sprite { get; set; }
    public string EvolutionMethod { get; set; }
    public bool HasEvolutionMethod => !string.IsNullOrEmpty(EvolutionMethod);
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
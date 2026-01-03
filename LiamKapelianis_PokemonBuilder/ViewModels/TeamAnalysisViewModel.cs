using LiamKapelianis_PokemonBuilder.Models;
using LiamKapelianis_PokemonBuilder.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace LiamKapelianis_PokemonBuilder.ViewModels;

public class TeamAnalysisViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string _coverageDescription;
    private string _suggestions;

    public ObservableCollection<TypeCoverage> OffensiveTypes { get; } = new();
    public ObservableCollection<TypeWeakness> Weaknesses { get; } = new();
    public ObservableCollection<TypeWeakness> Resistances { get; } = new();

    public string CoverageDescription
    {
        get => _coverageDescription;
        set
        {
            _coverageDescription = value;
            OnPropertyChanged(nameof(CoverageDescription));
        }
    }

    public string Suggestions
    {
        get => _suggestions;
        set
        {
            _suggestions = value;
            OnPropertyChanged(nameof(Suggestions));
        }
    }

    public ICommand RefreshAnalysisCommand { get; }

    public TeamAnalysisViewModel()
    {
        RefreshAnalysisCommand = new Command(AnalyzeTeam);

        // Subscribe to team changes
        TeamService.Instance.TeamChanged += (s, e) => AnalyzeTeam();

        AnalyzeTeam();
    }

    private void AnalyzeTeam()
    {
        // Get actual team data from the shared service
        var team = TeamService.Instance.GetTeamCopy();

        if (team.Count == 0)
        {
            CoverageDescription = "Add Pokémon to your team to see analysis.";
            Suggestions = "Build a team of 6 Pokémon for the best analysis.";
            OffensiveTypes.Clear();
            Weaknesses.Clear();
            Resistances.Clear();
            return;
        }

        AnalyzeOffensiveCoverage(team);
        AnalyzeDefensiveWeaknesses(team);
        GenerateSuggestions(team);
    }

    private void AnalyzeOffensiveCoverage(List<Pokemon> team)
    {
        OffensiveTypes.Clear();
        var typeSet = new HashSet<string>();

        foreach (var pokemon in team)
        {
            foreach (var type in pokemon.Types)
            {
                typeSet.Add(type);
            }
        }

        foreach (var type in typeSet)
        {
            OffensiveTypes.Add(new TypeCoverage
            {
                TypeName = char.ToUpper(type[0]) + type[1..],
                Color = GetTypeColor(type)
            });
        }

        CoverageDescription = $"Your team covers {OffensiveTypes.Count} different types.";
    }

    private void AnalyzeDefensiveWeaknesses(List<Pokemon> team)
    {
        Weaknesses.Clear();
        Resistances.Clear();

        var weaknessCount = new Dictionary<string, int>();
        var resistanceCount = new Dictionary<string, int>();

        foreach (var pokemon in team)
        {
            var weaknesses = GetTypeWeaknesses(pokemon.Types);
            var resistances = GetTypeResistances(pokemon.Types);

            foreach (var weakness in weaknesses)
            {
                weaknessCount[weakness] = weaknessCount.GetValueOrDefault(weakness, 0) + 1;
            }

            foreach (var resistance in resistances)
            {
                resistanceCount[resistance] = resistanceCount.GetValueOrDefault(resistance, 0) + 1;
            }
        }

        foreach (var kvp in weaknessCount.OrderByDescending(x => x.Value))
        {
            Weaknesses.Add(new TypeWeakness
            {
                TypeName = char.ToUpper(kvp.Key[0]) + kvp.Key[1..],
                Count = kvp.Value
            });
        }

        foreach (var kvp in resistanceCount.OrderByDescending(x => x.Value))
        {
            Resistances.Add(new TypeWeakness
            {
                TypeName = char.ToUpper(kvp.Key[0]) + kvp.Key[1..],
                Count = kvp.Value
            });
        }
    }

    private void GenerateSuggestions(List<Pokemon> team)
    {
        var suggestions = new List<string>();

        if (team.Count < 6)
        {
            suggestions.Add($"Add {6 - team.Count} more Pokémon to complete your team.");
        }

        if (Weaknesses.Any(w => w.Count >= 3))
        {
            var topWeakness = Weaknesses.First();
            suggestions.Add($"Your team is very weak to {topWeakness.TypeName} type attacks. Consider adding a Pokémon that resists {topWeakness.TypeName}.");
        }

        if (OffensiveTypes.Count < 6)
        {
            suggestions.Add("Try to diversify your team's type coverage for better offensive options.");
        }

        if (!suggestions.Any())
        {
            suggestions.Add("Your team has good type balance!");
        }

        Suggestions = string.Join("\n\n", suggestions);
    }

    private List<string> GetTypeWeaknesses(List<string> types)
    {
        // Simplified type chart - you'd want a complete one
        var weaknessMap = new Dictionary<string, List<string>>
        {
            { "fire", new List<string> { "water", "ground", "rock" } },
            { "water", new List<string> { "electric", "grass" } },
            { "grass", new List<string> { "fire", "ice", "poison", "flying", "bug" } },
            { "electric", new List<string> { "ground" } },
            { "normal", new List<string> { "fighting" } },
            { "fighting", new List<string> { "flying", "psychic", "fairy" } },
            { "psychic", new List<string> { "bug", "ghost", "dark" } },
            { "dragon", new List<string> { "ice", "dragon", "fairy" } },
            { "dark", new List<string> { "fighting", "bug", "fairy" } },
            { "fairy", new List<string> { "poison", "steel" } }
        };

        var weaknesses = new List<string>();
        foreach (var type in types)
        {
            if (weaknessMap.ContainsKey(type.ToLower()))
            {
                weaknesses.AddRange(weaknessMap[type.ToLower()]);
            }
        }

        return weaknesses.Distinct().ToList();
    }

    private List<string> GetTypeResistances(List<string> types)
    {
        // Simplified resistance chart
        var resistanceMap = new Dictionary<string, List<string>>
        {
            { "fire", new List<string> { "fire", "grass", "ice", "bug", "steel", "fairy" } },
            { "water", new List<string> { "fire", "water", "ice", "steel" } },
            { "grass", new List<string> { "water", "grass", "electric", "ground" } },
            { "electric", new List<string> { "electric", "flying", "steel" } },
            { "steel", new List<string> { "normal", "grass", "ice", "flying", "psychic", "bug", "rock", "dragon", "steel", "fairy" } }
        };

        var resistances = new List<string>();
        foreach (var type in types)
        {
            if (resistanceMap.ContainsKey(type.ToLower()))
            {
                resistances.AddRange(resistanceMap[type.ToLower()]);
            }
        }

        return resistances.Distinct().ToList();
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

// Supporting models
public class TypeCoverage
{
    public string TypeName { get; set; }
    public Color Color { get; set; }
}

public class TypeWeakness
{
    public string TypeName { get; set; }
    public int Count { get; set; }
}
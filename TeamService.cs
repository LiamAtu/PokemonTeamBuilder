using LiamKapelianis_PokemonBuilder.Models;
using System.Collections.ObjectModel;

namespace LiamKapelianis_PokemonBuilder.Services;

public class TeamService
{
    private static TeamService _instance;
    public static TeamService Instance => _instance ??= new TeamService();

    private TeamService() { }

    public ObservableCollection<Pokemon> CurrentTeam { get; } = new();

    public event EventHandler TeamChanged;

    public bool AddToTeam(Pokemon pokemon)
    {
        if (pokemon == null) return false;

        // Check if already in team
        if (CurrentTeam.Any(p => p.Name == pokemon.Name))
            return false;

        // Check if team is full
        if (CurrentTeam.Count >= 6)
            return false;

        CurrentTeam.Add(pokemon);
        TeamChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void RemoveFromTeam(Pokemon pokemon)
    {
        if (pokemon == null) return;

        var toRemove = CurrentTeam.FirstOrDefault(p => p.Name == pokemon.Name);
        if (toRemove != null)
        {
            CurrentTeam.Remove(toRemove);
            TeamChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsInTeam(Pokemon pokemon)
    {
        return pokemon != null && CurrentTeam.Any(p => p.Name == pokemon.Name);
    }

    public void ClearTeam()
    {
        CurrentTeam.Clear();
        TeamChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<Pokemon> GetTeamCopy()
    {
        return new List<Pokemon>(CurrentTeam);
    }
}
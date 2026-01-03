using LiamKapelianis_PokemonBuilder.Models;
using System.Collections.ObjectModel;

namespace LiamKapelianis_PokemonBuilder.Services;

public class TeamService
{
    private static TeamService _instance;
    public static TeamService Instance => _instance ??= new TeamService();

    private TeamService() { }

    // Two teams
    public ObservableCollection<Pokemon> Team1 { get; } = new();
    public ObservableCollection<Pokemon> Team2 { get; } = new();

    private int _activeTeamNumber = 1;
    public int ActiveTeamNumber
    {
        get => _activeTeamNumber;
        set
        {
            _activeTeamNumber = value;
            ActiveTeamChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // Returns the currently active team
    public ObservableCollection<Pokemon> CurrentTeam => ActiveTeamNumber == 1 ? Team1 : Team2;

    public event EventHandler TeamChanged;
    public event EventHandler ActiveTeamChanged;

    public bool AddToTeam(Pokemon pokemon)
    {
        if (pokemon == null) return false;

        var team = CurrentTeam;

        if (team.Any(p => p.Name == pokemon.Name))
            return false;

        if (team.Count >= 6)
            return false;

        team.Add(pokemon);
        TeamChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void RemoveFromTeam(Pokemon pokemon)
    {
        if (pokemon == null) return;

        var team = CurrentTeam;
        var toRemove = team.FirstOrDefault(p => p.Name == pokemon.Name);
        if (toRemove != null)
        {
            team.Remove(toRemove);
            TeamChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsInTeam(Pokemon pokemon)
    {
        return pokemon != null && CurrentTeam.Any(p => p.Name == pokemon.Name);
    }

    public void ClearCurrentTeam()
    {
        CurrentTeam.Clear();
        TeamChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearAllTeams()
    {
        Team1.Clear();
        Team2.Clear();
        TeamChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<Pokemon> GetTeamCopy()
    {
        return new List<Pokemon>(CurrentTeam);
    }

    public string GetActiveTeamName()
    {
        return ActiveTeamNumber == 1 ? "Team 1" : "Team 2";
    }
}
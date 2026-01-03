namespace LiamKapelianis_PokemonBuilder;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("PokemonDetailPage", typeof(PokemonDetailPage));
        Routing.RegisterRoute("TeamAnalysisPage", typeof(TeamAnalysisPage));
        Routing.RegisterRoute("PokemonComparePage", typeof(PokemonComparePage));
    }
}
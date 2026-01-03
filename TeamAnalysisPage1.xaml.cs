using LiamKapelianis_PokemonBuilder.ViewModels;

namespace LiamKapelianis_PokemonBuilder;

public partial class TeamAnalysisPage : ContentPage
{
    public TeamAnalysisPage()
    {
        InitializeComponent();
        BindingContext = new TeamAnalysisViewModel();
    }
}
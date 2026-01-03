using LiamKapelianis_PokemonBuilder.ViewModels;

namespace LiamKapelianis_PokemonBuilder;

public partial class PokemonComparePage : ContentPage
{
    public PokemonComparePage()
    {
        InitializeComponent();
        BindingContext = new PokemonCompareViewModel();
    }
}
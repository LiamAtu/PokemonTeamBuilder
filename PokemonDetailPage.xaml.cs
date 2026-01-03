using LiamKapelianis_PokemonBuilder.ViewModels;
using LiamKapelianis_PokemonBuilder.Models;

namespace LiamKapelianis_PokemonBuilder;

[QueryProperty(nameof(PokemonData), "Pokemon")]
public partial class PokemonDetailPage : ContentPage
{
    private PokemonDetailViewModel _viewModel;

    public Pokemon PokemonData
    {
        set
        {
            if (_viewModel == null)
            {
                _viewModel = new PokemonDetailViewModel();
                BindingContext = _viewModel;
            }
            _viewModel.Pokemon = value;
        }
    }

    public PokemonDetailPage()
    {
        InitializeComponent();
    }
}
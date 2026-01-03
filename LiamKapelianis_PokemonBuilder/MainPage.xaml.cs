using LiamKapelianis_PokemonBuilder.ViewModels;
using LiamKapelianis_PokemonBuilder.Models;

namespace LiamKapelianis_PokemonBuilder;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();

        // Subscribe to scroll message
        MessagingCenter.Subscribe<MainViewModel, Pokemon>(this, "ScrollToPokemon", (sender, pokemon) =>
        {
            PokeShowcaseGrid.ScrollTo(pokemon, animate: true);
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<MainViewModel, Pokemon>(this, "ScrollToPokemon");
    }
}
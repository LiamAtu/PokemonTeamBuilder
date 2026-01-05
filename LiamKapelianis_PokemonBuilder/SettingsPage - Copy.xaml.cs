using LiamKapelianis_PokemonBuilder.ViewModels;

namespace LiamKapelianis_PokemonBuilder;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel();
    }
}
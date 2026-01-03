using LiamKapelianis_PokemonBuilder.Models;
using Microsoft.Maui.Graphics;
using System.Net.Http.Json;
using System.Windows.Input;

namespace LiamKapelianis_PokemonBuilder;

public partial class MainPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    public ICommand PokemonTappedCommand { get; }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        PokemonTappedCommand = new Command<Pokemon>(OnPokemonTapped);

        _ = LoadThePokemonAsync();
    }

    private void OnPokemonTapped(Pokemon pokemon)
    {
        // Example: toggle selection background
        if (pokemon.BackgroundColor != Colors.Gold)
            pokemon.BackgroundColor = Colors.Gold;
        else
            pokemon.BackgroundColor = GetTypeColor(pokemon.Types.FirstOrDefault());

        // Refresh UI
        PokeShowcaseGrid.ItemsSource = PokeShowcaseGrid.ItemsSource;
    }

    private async Task LoadThePokemonAsync()
    {
        var allPokemon = new List<Pokemon>();

        for (int i = 1; i <= 9; i++) // load first 9 Pokemon
        {
            string url = $"https://pokeapi.co/api/v2/pokemon/{i}";
            try
            {
                var p = await _httpClient.GetFromJsonAsync<PokeAPIModels.PokemonResponse>(url);
                if (p != null)
                {
                    allPokemon.Add(new Pokemon
                    {
                        Name = char.ToUpper(p.name[0]) + p.name.Substring(1),
                        Sprite = p.sprites.front_default,
                        Types = p.types.Select(t => t.type.name).ToList(),
                        BackgroundColor = GetTypeColor(p.types.FirstOrDefault()?.type.name)
                    });
                }
            }
            catch { }
        }

        PokeShowcaseGrid.ItemsSource = allPokemon;
    }

    private Color GetTypeColor(string type)
    {
        return type?.ToLower() switch
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
            _ => Colors.Red
        };
    }

    private async void SearchButton_Clicked(object sender, EventArgs e)
    {
        string query = SearchBar.Text?.Trim().ToLower();
        if (string.IsNullOrEmpty(query))
        {
            await DisplayAlert("Error", "Please enter a Pokémon name.", "OK");
            return;
        }

        var pokemon = ((List<Pokemon>)PokeShowcaseGrid.ItemsSource)
                      .FirstOrDefault(p => p.Name.ToLower() == query);

        if (pokemon != null)
        {
            await DisplayAlert("Found!", $"{pokemon.Name} found!", "OK");
            PokeShowcaseGrid.ScrollTo(pokemon);
        }
        else
        {
            await DisplayAlert("Not Found", $"No Pokémon named {query}", "OK");
        }
    }
}

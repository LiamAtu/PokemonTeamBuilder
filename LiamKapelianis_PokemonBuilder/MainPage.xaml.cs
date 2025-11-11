using System.Net.Http.Json;
using System.Windows.Input;

namespace LiamKapelianis_PokemonBuilder;

public partial class MainPage : ContentPage
{
    //requeast to the pokemon api (read only so wee dont change the data,just grab it)
    private readonly HttpClient _httpClient = new();

    public MainPage()
    {
        InitializeComponent();
        //needed so binding background and binding name can work
        BindingContext = this;

        //creates a command that runs when a pokemnon frtame is tapped
        PokemonTappedCommand = new Command<PokemonHolder>(OnPokemonTapped);

        // loads pokemon on startup
        _ = LoadThePokemonAsync();
    }

    //tap actionfor the collection view items
    public ICommand PokemonTappedCommand { get; }

    private void OnPokemonTapped(PokemonHolder pokemon)
    {
        // Example: swap background color by player
        

        //forces a UI refresh for user
        PokeShowcaseGrid.ItemsSource = PokeShowcaseGrid.ItemsSource;
    }

    // This displays a single pokemon's info-name,sprite,type and the bg colour
    public class PokemonHolder
    {
        public string Name { get; set; }
        public string Sprite { get; set; }
        public Color BackgroundColor { get; set; } = Colors.Red;
        public List<string> Types { get; set; } = new();
    }

    // PokéAPI
    public class PokemonResponse
    {
        public string name { get; set; }
        public Sprites sprites { get; set; }
        public List<TypeSlot> types { get; set; }
    }

    public class Sprites
    {
        public string front_default { get; set; }
    }

    public class TypeSlot
    {
        public TypeInfo type { get; set; }
    }

    public class TypeInfo
    {
        public string name { get; set; }
    }

    private async Task LoadThePokemonAsync()
    {
        var allPokemon = new List<PokemonHolder>();

        for (int i = 1; i <= 9; i++) // the higher the number the more pokemon to load
        {
            string url = $"https://pokeapi.co/api/v2/pokemon/{i}";
            try
            {
                var p = await _httpClient.GetFromJsonAsync<PokemonResponse>(url);
                if (p != null)
                {
                    allPokemon.Add(new PokemonHolder
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
    //Maps a Pokémon’s main type to a colour.
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

    //search bar button
    private async void SearchButton_Clicked(object sender, EventArgs e)
    {
       /* string query = SearchBar.Text?.Trim().ToLower();
        if (string.IsNullOrEmpty(query))
        {
            await DisplayAlert("Error", "Please enter a Pokémon name.", "OK");
            return;
        }

        var pokemon = ((List<PokemonHolder>)PokeShowcaseGrid.ItemsSource)
                      .FirstOrDefault(p => p.Name.ToLower() == query);

        if (pokemon != null)
        {
            await DisplayAlert("Found!", $"{pokemon.Name} found!", "OK");
            PokeShowcaseGrid.ScrollTo(pokemon);
        }
        else
        {
            await DisplayAlert("Not Found", $"No Pokémon named {query}", "OK");
        }*/
    }
}
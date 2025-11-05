namespace LiamKapelianis_PokemonBuilder;

public partial class MainPage : ContentPage
{
    //variables 
    int count = 0;
    private int numbRows = 3;
    private int player = 1;

    public MainPage()
    {
        InitializeComponent();
        //Calls the grid holdimg pokeon sprites and names and types as soon as app starts
        TheGrid();
    }
    //Searches for the pokemon typed into the search bar
    private void SearchButton_Clicked(object sender, EventArgs e)
    {

    }
    //the grid that will be created holding the pokemon(PokeShowcase)
    private void TheGrid()
    {
        //Create numbRows rows and numbrows columns
        for (int i = 0; i < numbRows; ++i)
        {
            PokeShowcase.AddRowDefinition(new RowDefinition());
            PokeShowcase.AddColumnDefinition(new ColumnDefinition());
        }

        //Populate the grid with Borders
        for (int i = 0; i < numbRows; ++i)
        {
            for (int j = 0; j < numbRows; ++j)
            {
                Border styledBorder = new Border
                {
                    BackgroundColor = Colors.Red,
                    Stroke = Colors.Black,
                    StrokeThickness = 3

                };
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += OnBorderTapped;
                styledBorder.GestureRecognizers.Add(tapGestureRecognizer);
                PokeShowcase.Add(styledBorder, j, i);
            }
        }
    }

    private void OnBorderTapped(object? sender, TappedEventArgs e)
    {
        Border border = (Border)sender;
        if (border != null)
        {
          // makeAnimation();
        }
    }

   /* private void makeAnimation(Border border)
    {
        
    }*/


}
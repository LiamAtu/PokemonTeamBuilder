namespace LiamKapelianis_PokemonBuilder;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Load saved theme preference
        bool isDarkMode = Preferences.Get("dark_mode", false);
        UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;

        MainPage = new AppShell();
    }
}
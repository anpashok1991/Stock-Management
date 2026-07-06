using MudBlazor;

namespace StockManagement.Web.Services;

public class ThemeService
{
    public bool IsDarkMode { get; set; }
    public MudTheme Theme { get; }
    public event Action? OnChange;

    public ThemeService()
    {
        Theme = new MudTheme()
        {
            Palette = new Palette()
            {
                Primary = "#594AE2",
                Secondary = "#546E7A",
                AppbarBackground = "#594AE2",
                Background = "#F5F5F5",
            },
            PaletteDark = new Palette()
            {
                Primary = "#7C6FFF",
                Secondary = "#546E7A",
                AppbarBackground = "#1E1E2D",
                Background = "#1E1E2D",
                Surface = "#2A2A3C",
            }
        };
    }

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        OnChange?.Invoke();
    }
}

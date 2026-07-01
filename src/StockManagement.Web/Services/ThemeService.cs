namespace StockManagement.Web.Services;

public class ThemeService
{
    private bool _isDarkMode;
    public bool IsDarkMode => _isDarkMode;
    public event Action? OnChange;

    public void ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
        OnChange?.Invoke();
    }
}

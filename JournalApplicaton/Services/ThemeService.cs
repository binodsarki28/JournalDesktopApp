public class ThemeService
{
    public bool IsDark { get; private set; }

    public event Action? OnThemeChanged;

    public void SetTheme(bool isDark)
    {
        IsDark = isDark;
        OnThemeChanged?.Invoke();
    }

    public void ToggleTheme()
    {
        IsDark = !IsDark;
        OnThemeChanged?.Invoke();
    }
}

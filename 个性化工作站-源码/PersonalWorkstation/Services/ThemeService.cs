using System.Windows;
using Microsoft.Win32;

namespace PersonalWorkstation.Services;

public static class ThemeService
{
    public const string Light = "light";
    public const string Dark = "dark";
    public const string System = "system";

    public static string Resolve(string theme)
        => theme == System ? (IsSystemDark() ? Dark : Light) : (theme == Dark ? Dark : Light);

    public static void Apply(string theme)
    {
        var actual = Resolve(theme);
        var source = new Uri($"Resources/{(actual == Dark ? "Dark" : "Light")}.xaml", UriKind.Relative);
        var dict = new ResourceDictionary { Source = source };

        var merged = Application.Current.Resources.MergedDictionaries;
        for (var i = 0; i < merged.Count; i++)
        {
            var existing = merged[i].Source?.OriginalString ?? "";
            if (existing.Contains("Light.xaml", StringComparison.OrdinalIgnoreCase) ||
                existing.Contains("Dark.xaml", StringComparison.OrdinalIgnoreCase))
            {
                merged[i] = dict;
                return;
            }
        }
        merged.Insert(0, dict);
    }

    public static bool IsSystemDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int number) return number == 0;
        }
        catch
        {
            // 读不到就按浅色处理
        }
        return false;
    }
}
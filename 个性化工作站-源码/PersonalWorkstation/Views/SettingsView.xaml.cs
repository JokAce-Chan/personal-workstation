using System.Windows;
using System.Windows.Controls;

namespace PersonalWorkstation.Views;

public partial class SettingsView : UserControl
{
    private string _section = "settings-data";

    public SettingsView()
    {
        InitializeComponent();
        ApplySection();
    }

    /// <summary>settings-data / settings-appearance / settings-about</summary>
    public string Section
    {
        get => _section;
        set
        {
            _section = value;
            ApplySection();
        }
    }

    private void ApplySection()
    {
        if (PanelData is null) return;
        PanelData.Visibility = _section == "settings-data" ? Visibility.Visible : Visibility.Collapsed;
        PanelAppearance.Visibility = _section == "settings-appearance" ? Visibility.Visible : Visibility.Collapsed;
        PanelAbout.Visibility = _section == "settings-about" ? Visibility.Visible : Visibility.Collapsed;
    }
}
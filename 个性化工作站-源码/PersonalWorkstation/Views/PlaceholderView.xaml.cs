using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace PersonalWorkstation.Views;

public partial class PlaceholderView : UserControl, INotifyPropertyChanged
{
    private string _heading = "";
    private string _stage = "";
    private string _note = "";
    private List<string> _bullets = new();

    public PlaceholderView()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string Heading
    {
        get => _heading;
        set => SetField(ref _heading, value);
    }

    public string Stage
    {
        get => _stage;
        set => SetField(ref _stage, value);
    }

    public string Note
    {
        get => _note;
        set => SetField(ref _note, value);
    }

    public List<string> Bullets
    {
        get => _bullets;
        set => SetField(ref _bullets, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
using System.Collections.ObjectModel;
using System.Windows;

namespace PersonalWorkstation.ViewModels;

/// <summary>左侧导航的一个分组（可折叠）。</summary>
public sealed class NavGroup : ObservableObject
{
    private bool _isExpanded = true;

    public string Key { get; init; } = "";
    public string Title { get; init; } = "";
    public ObservableCollection<NavItem> Items { get; } = new();

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (Set(ref _isExpanded, value)) Raise(nameof(ItemsVisibility));
        }
    }

    public Visibility ItemsVisibility => IsExpanded ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>没有标题的分组不显示分组头（用于置顶的首页总览）。</summary>
    public Visibility HeaderVisibility => string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;
}

/// <summary>左侧导航里的一个模块。</summary>
public sealed class NavItem : ObservableObject
{
    private bool _isSelected;

    public string Key { get; init; } = "";
    public string Title { get; init; } = "";
    /// <summary>Segoe MDL2 Assets 字形</summary>
    public string Icon { get; init; } = "";
    public string Summary { get; init; } = "";
    public bool IsPinned { get; init; }
    public ObservableCollection<SubTab> Tabs { get; } = new();
    public RelayCommand SelectCommand { get; set; } = null!;

    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }
}

/// <summary>右侧内容区顶部的详细标签页。</summary>
public sealed class SubTab : ObservableObject
{
    private bool _isSelected;

    public string Key { get; init; } = "";
    public string Title { get; init; } = "";
    public RelayCommand SelectCommand { get; set; } = null!;
    /// <summary>已经创建过的视图，切换标签时复用。</summary>
    public object? View { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }
}
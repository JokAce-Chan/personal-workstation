using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using PersonalWorkstation.Models;
using PersonalWorkstation.Services;

namespace PersonalWorkstation.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly DataStoreService _store;
    private readonly Func<NavItem, SubTab, object> _viewFactory;
    private readonly DispatcherTimer _saveTimer;

    private NavItem? _selectedItem;
    private SubTab? _selectedTab;
    private object? _currentView;
    private bool _navVisible;
    private bool _navCollapsed;
    private string _saveStatusText = "";
    private bool _hasSaveError;
    private string _themeMode = ThemeService.Light;

    public MainViewModel(DataStoreService store, Func<NavItem, SubTab, object> viewFactory)
    {
        _store = store;
        _viewFactory = viewFactory;

        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _saveTimer.Tick += (_, _) =>
        {
            _saveTimer.Stop();
            SaveWithStatus();
        };

        BuildCatalog();
        WireUp();

        _themeMode = Data.Settings.Theme;
        ThemeService.Apply(_themeMode);
        if (ThemeService.Resolve(_themeMode) == ThemeService.Dark) IsDarkTheme = true;

        _navVisible = Data.Settings.NavVisible;
        _navCollapsed = Data.Settings.NavCollapsed;

        RefreshBackups();
        SaveStatusText = _store.LastSavedAt is null
            ? "尚未保存"
            : $"已保存 {_store.LastSavedAt:HH:mm:ss}";

        SelectItem(AllItems.First());
    }

    // ── 数据 ────────────────────────────────────────────────────────────

    public AppData Data => _store.Data;

    public DataStoreService Store => _store;

    // ── 导航 ────────────────────────────────────────────────────────────

    public ObservableCollection<NavGroup> Groups { get; } = new();

    /// <summary>固定在左下角的「数据与设置」。</summary>
    public NavItem PinnedItem { get; private set; } = null!;

    public List<NavItem> AllItems { get; } = new();

    public NavItem? SelectedItem
    {
        get => _selectedItem;
        private set
        {
            if (!Set(ref _selectedItem, value)) return;
            Raise(nameof(ModuleTitle));
            Raise(nameof(ModuleSummary));
            Raise(nameof(VisibleTabs));
            Raise(nameof(ShowTabs));
        }
    }

    public string ModuleTitle => SelectedItem?.Title ?? "";

    public string ModuleSummary => SelectedItem?.Summary ?? "";

    public ObservableCollection<SubTab> VisibleTabs { get; } = new();

    public bool ShowTabs => VisibleTabs.Count > 1;

    public SubTab? SelectedTab
    {
        get => _selectedTab;
        private set => Set(ref _selectedTab, value);
    }

    public object? CurrentView
    {
        get => _currentView;
        private set => Set(ref _currentView, value);
    }

    public bool NavVisible
    {
        get => _navVisible;
        set
        {
            if (!Set(ref _navVisible, value)) return;
            Data.Settings.NavVisible = value;
            Raise(nameof(NavColumnWidth));
            Raise(nameof(ShowNavText));
            Raise(nameof(NavToggleText));
            MarkDirty();
        }
    }

    public bool NavCollapsed
    {
        get => _navCollapsed;
        set
        {
            if (!Set(ref _navCollapsed, value)) return;
            Data.Settings.NavCollapsed = value;
            Raise(nameof(NavColumnWidth));
            Raise(nameof(ShowNavText));
            Raise(nameof(NavCollapseGlyph));
            MarkDirty();
        }
    }

    public GridLength NavColumnWidth => new(NavVisible ? (NavCollapsed ? 58 : 236) : 0);

    public bool ShowNavText => NavVisible && !NavCollapsed;

    public string NavCollapseGlyph => NavCollapsed ? "\uE76C" : "\uE76B";

    public string NavToggleText => NavVisible ? "隐藏导航栏" : "显示导航栏";

    // ── 状态栏 / 顶栏 ───────────────────────────────────────────────────

    public string TodayText => DateTime.Now.ToString("yyyy-MM-dd dddd", CultureInfo.GetCultureInfo("zh-CN"));

    public string VersionText => "版本 1.0.0";

    public string DataFileDisplay => _store.DataFile;

    public string BackupCountText { get; private set; } = "";

    public string ThemeLabel => _themeMode switch
    {
        ThemeService.Dark => "深色",
        ThemeService.System => "跟随系统",
        _ => "浅色",
    };

    public bool IsDarkTheme { get; private set; }

    public string SaveStatusText
    {
        get => _saveStatusText;
        private set => Set(ref _saveStatusText, value);
    }

    /// <summary>保存失败时状态栏变红，方便一眼看到。</summary>
    public bool HasSaveError
    {
        get => _hasSaveError;
        private set => Set(ref _hasSaveError, value);
    }

    public ObservableCollection<BackupInfo> Backups { get; } = new();

    public string? StartupNotice { get; set; }

    // ── 命令 ────────────────────────────────────────────────────────────

    public RelayCommand ToggleNavVisibleCommand => new(() => NavVisible = !NavVisible);

    public RelayCommand ToggleNavCollapsedCommand => new(() => NavCollapsed = !NavCollapsed);

    public RelayCommand ToggleThemeCommand => new(() =>
    {
        ThemeMode = _themeMode switch
        {
            ThemeService.Light => ThemeService.Dark,
            ThemeService.Dark => ThemeService.System,
            _ => ThemeService.Light,
        };
    });

    public string ThemeMode
    {
        get => _themeMode;
        set
        {
            if (!Set(ref _themeMode, value)) return;
            Data.Settings.Theme = value;
            ThemeService.Apply(value);
            IsDarkTheme = ThemeService.Resolve(value) == ThemeService.Dark;
            Raise(nameof(IsDarkTheme));
            Raise(nameof(ThemeLabel));
            MarkDirty();
        }
    }

    public RelayCommand SetThemeCommand => new(parameter =>
    {
        if (parameter is string mode) ThemeMode = mode;
    });

    public RelayCommand SaveNowCommand => new(() => SaveWithStatus());

    public RelayCommand BackupNowCommand => new(() =>
    {
        var name = _store.CreateBackup();
        RefreshBackups();
        SaveStatusText = $"已备份 {name}";
    });

    public RelayCommand OpenDataFolderCommand => new(() => OpenFolder(_store.DataDir));

    public RelayCommand OpenBackupFolderCommand => new(() => OpenFolder(_store.BackupDir));

    public RelayCommand ReloadCommand => new(() =>
    {
        _store.LoadWithRecovery();
        NavCollapsed = Data.Settings.NavCollapsed;
        ThemeMode = Data.Settings.Theme;
        RefreshAll();
        SaveStatusText = "已重新加载数据文件";
    });

    public RelayCommand ImportBackupCommand => new(() =>
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择要导入的数据文件",
            Filter = "数据文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            InitialDirectory = _store.BackupDir,
        };
        if (dialog.ShowDialog() != true) return;
        if (!_store.ImportFromFile(dialog.FileName, out var message))
        {
            MessageBox.Show(message, "导入失败", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        RefreshAll();
        SaveStatusText = "导入完成";
        MessageBox.Show(message, "导入完成", MessageBoxButton.OK, MessageBoxImage.Information);
    });

    public RelayCommand RestoreBackupCommand => new(parameter =>
    {
        if (parameter is not BackupInfo info) return;
        var confirm = MessageBox.Show(
            $"确定要用这份备份覆盖当前数据吗？\n\n备份时间：{info.CreatedText}\n文件：{info.FileName}\n\n" +
            "当前数据会先被自动另存一份，所以仍然可以找回。",
            "恢复备份", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        if (!_store.RestoreBackup(info.FileName, out var message))
        {
            MessageBox.Show(message, "恢复失败", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        RefreshAll();
        SaveStatusText = "已恢复备份";
        MessageBox.Show(message, "恢复完成", MessageBoxButton.OK, MessageBoxImage.Information);
    });

    public RelayCommand ResetDataCommand => new(() =>
    {
        var confirm = MessageBox.Show(
            "确定要清空所有数据吗？\n\n" +
            "所有模块的记录都会被删除，只保留一个空的数据文件；" +
            "清空前会自动做一份备份，可以从备份里找回来。",
            "清空所有数据", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        _store.CreateBackup("清空前自动备份");
        _store.ResetToEmpty();
        RefreshAll();
        RefreshBackups();
        SaveStatusText = "已清空数据";
    });

    // ── 行为 ────────────────────────────────────────────────────────────

    public void ToggleGroup(NavGroup group)
    {
        group.IsExpanded = !group.IsExpanded;
        var list = Data.Settings.CollapsedGroups;
        if (group.IsExpanded) list.Remove(group.Key);
        else if (!string.IsNullOrEmpty(group.Key) && !list.Contains(group.Key)) list.Add(group.Key);
        MarkDirty();
    }

    private void SelectItem(NavItem item)
    {
        foreach (var other in AllItems) other.IsSelected = ReferenceEquals(other, item);
        SelectedItem = item;

        VisibleTabs.Clear();
        foreach (var tab in item.Tabs) VisibleTabs.Add(tab);
        Raise(nameof(ShowTabs));

        SelectTab(VisibleTabs.FirstOrDefault());
    }

    private void SelectTab(SubTab? tab)
    {
        foreach (var other in VisibleTabs) other.IsSelected = ReferenceEquals(other, tab);
        SelectedTab = tab;
        CurrentView = tab is null ? null : EnsureView(tab);
    }

    private object EnsureView(SubTab tab)
    {
        if (tab.View is null && SelectedItem is not null)
        {
            var view = _viewFactory(SelectedItem, tab);
            if (view is FrameworkElement element && element.DataContext is null)
            {
                element.DataContext = this;
            }
            tab.View = view;
        }
        return tab.View!;
    }

    /// <summary>数据被修改后调用：500ms 防抖后自动写盘。</summary>
    public void MarkDirty()
    {
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    public void SaveWithStatus()
    {
        _saveTimer.Stop();
        var ok = _store.Save();
        HasSaveError = !ok;
        SaveStatusText = ok
            ? $"已保存 {DateTime.Now:HH:mm:ss}"
            : $"保存失败：{_store.LastError}";
    }

    public void RefreshAll()
    {
        RefreshBackups();
        Raise(nameof(ModuleTitle));
        Raise(nameof(ModuleSummary));
        Raise(nameof(TodayText));
        RefreshCurrentView();
    }

    private void RefreshCurrentView()
    {
        CurrentView = SelectedTab is null ? null : EnsureView(SelectedTab);
    }

    public void RefreshBackups()
    {
        Backups.Clear();
        foreach (var item in _store.ListBackups()) Backups.Add(item);
        BackupCountText = Backups.Count == 0 ? "暂无备份" : $"{Backups.Count} 份备份";
    }

    private static void OpenFolder(string path)
    {
        try
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LogService.Error($"打开目录失败：{path}", ex);
        }
    }

    // ── 模块目录 ────────────────────────────────────────────────────────

    private void BuildCatalog()
    {
        var home = NewItem("home", "首页总览", "\uE80F", "今日计划、快速备忘与各模块摘要");
        home.Tabs.Add(NewTab("home-overview", "今日总览"));

        var plan = NewItem("plan", "今日计划", "\uE787", "当天要做的事、优先级与顺延");
        plan.Tabs.Add(NewTab("plan-tasks", "任务清单"));
        plan.Tabs.Add(NewTab("plan-calendar", "月历与完成率"));

        var dev = NewItem("dev", "开发工作", "\uE943", "项目、待办与缺陷、代码片段");
        dev.Tabs.Add(NewTab("dev-projects", "项目与待办"));
        dev.Tabs.Add(NewTab("dev-snippets", "代码片段"));
        dev.Tabs.Add(NewTab("dev-notes", "项目笔记"));

        var consulting = NewItem("consulting", "咨询工作", "\uE77B", "客户、项目、沟通记录与时长费用");
        consulting.Tabs.Add(NewTab("consulting-clients", "客户"));
        consulting.Tabs.Add(NewTab("consulting-projects", "咨询项目"));
        consulting.Tabs.Add(NewTab("consulting-logs", "沟通记录"));
        consulting.Tabs.Add(NewTab("consulting-fee", "费用统计"));

        var social = NewItem("social", "自媒体", "\uE714", "选题、排期与发布后数据");
        social.Tabs.Add(NewTab("social-schedule", "内容排期"));
        social.Tabs.Add(NewTab("social-ideas", "灵感池"));
        social.Tabs.Add(NewTab("social-review", "数据复盘"));
        social.Tabs.Add(NewTab("social-platforms", "平台管理"));

        var fitness = NewItem("fitness", "健身计划", "\uEB51", "本周安排、训练打卡与身体数据");
        fitness.Tabs.Add(NewTab("fitness-week", "本周安排"));
        fitness.Tabs.Add(NewTab("fitness-sessions", "训练记录"));
        fitness.Tabs.Add(NewTab("fitness-body", "身体数据"));
        fitness.Tabs.Add(NewTab("fitness-goals", "目标"));

        var diet = NewItem("diet", "饮食计划", "\uE7C1", "四餐记录、食物库与营养趋势");
        diet.Tabs.Add(NewTab("diet-today", "今日记录"));
        diet.Tabs.Add(NewTab("diet-foods", "食物库"));
        diet.Tabs.Add(NewTab("diet-trend", "趋势与目标"));

        var entertainment = NewItem("entertainment", "游戏娱乐", "\uE734", "收藏库、游玩时长与统计");
        entertainment.Tabs.Add(NewTab("ent-collection", "收藏库"));
        entertainment.Tabs.Add(NewTab("ent-sessions", "时长记录"));
        entertainment.Tabs.Add(NewTab("ent-stats", "统计"));

        var settings = NewItem("settings", "数据与设置", "\uE713", "数据文件位置、备份恢复、外观与危险操作", pinned: true);
        settings.Tabs.Add(NewTab("settings-data", "数据与备份"));
        settings.Tabs.Add(NewTab("settings-appearance", "外观与导航"));
        settings.Tabs.Add(NewTab("settings-about", "关于与危险操作"));

        AllItems.AddRange(new[] { home, plan, dev, consulting, social, fitness, diet, entertainment, settings });
        PinnedItem = settings;

        var top = new NavGroup { Key = "top", Title = "" };
        top.Items.Add(home);

        var work = new NavGroup { Key = "work", Title = "工作事务" };
        work.Items.Add(plan);
        work.Items.Add(dev);
        work.Items.Add(consulting);

        var create = new NavGroup { Key = "create", Title = "内容创作" };
        create.Items.Add(social);

        var life = new NavGroup { Key = "life", Title = "生活健康" };
        life.Items.Add(fitness);
        life.Items.Add(diet);
        life.Items.Add(entertainment);

        foreach (var group in new[] { top, work, create, life })
        {
            group.IsExpanded = !Data.Settings.CollapsedGroups.Contains(group.Key);
            Groups.Add(group);
        }
    }

    private NavItem NewItem(string key, string title, string icon, string summary, bool pinned = false)
        => new NavItem
        {
            Key = key,
            Title = title,
            Icon = icon,
            Summary = summary,
            IsPinned = pinned,
        };

    private SubTab NewTab(string key, string title) => new() { Key = key, Title = title };

    /// <summary>把所有命令挂上（构建目录后调用一次）。</summary>
    public void WireUp()
    {
        foreach (var item in AllItems)
        {
            var captured = item;
            captured.SelectCommand = new RelayCommand(() => SelectItem(captured));
            foreach (var tab in captured.Tabs)
            {
                var capturedTab = tab;
                capturedTab.SelectCommand = new RelayCommand(() => SelectTab(capturedTab));
            }
        }

    }

    public void SelectByKey(string key)
    {
        var item = AllItems.FirstOrDefault(x => x.Key == key);
        if (item is not null) SelectItem(item);
    }

    public void SelectTabByKey(string key)
    {
        var tab = VisibleTabs.FirstOrDefault(x => x.Key == key);
        if (tab is not null) SelectTab(tab);
    }
}

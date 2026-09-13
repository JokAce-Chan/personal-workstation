using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PersonalWorkstation.Services;
using PersonalWorkstation.ViewModels;

namespace PersonalWorkstation;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow(MainViewModel viewModel)
    {
        _vm = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        RestoreWindowBounds();
        PreviewKeyDown += OnPreviewKeyDown;
        Closing += OnClosing;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                var index = e.Key - Key.D1;
                if (index < _vm.AllItems.Count)
                {
                    _vm.SelectByKey(_vm.AllItems[index].Key);
                    e.Handled = true;
                }
                return;
            }

            if (e.Key == Key.S)
            {
                _vm.SaveWithStatus();
                e.Handled = true;
                return;
            }
        }

        if (e.Key == Key.F5)
        {
            _vm.ReloadCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void RestoreWindowBounds()
    {
        var settings = _vm.Data.Settings;
        if (settings.WindowWidth > 800) Width = settings.WindowWidth;
        if (settings.WindowHeight > 600) Height = settings.WindowHeight;

        if (settings.WindowLeft >= 0 && settings.WindowTop >= 0)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = settings.WindowLeft;
            Top = settings.WindowTop;

            // 避免上次在副屏关闭、这次副屏不在的情况
            var virtualLeft = SystemParameters.VirtualScreenLeft;
            var virtualTop = SystemParameters.VirtualScreenTop;
            var virtualRight = virtualLeft + SystemParameters.VirtualScreenWidth;
            var virtualBottom = virtualTop + SystemParameters.VirtualScreenHeight;
            if (Left + 120 < virtualLeft || Top + 60 < virtualTop || Left > virtualRight - 120 || Top > virtualBottom - 60)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Left = double.NaN;
                Top = double.NaN;
            }
        }

        if (settings.WindowMaximized) WindowState = WindowState.Maximized;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        try
        {
            var settings = _vm.Data.Settings;
            var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
            settings.WindowMaximized = WindowState == WindowState.Maximized;
            if (!bounds.IsEmpty && bounds.Width > 100 && bounds.Height > 100)
            {
                settings.WindowWidth = bounds.Width;
                settings.WindowHeight = bounds.Height;
                settings.WindowLeft = (int)bounds.Left;
                settings.WindowTop = (int)bounds.Top;
            }
            _vm.SaveWithStatus();
            _vm.Store.AutoBackupIfNeeded();
        }
        catch (Exception ex)
        {
            LogService.Error("退出时保存失败", ex);
        }
    }
}
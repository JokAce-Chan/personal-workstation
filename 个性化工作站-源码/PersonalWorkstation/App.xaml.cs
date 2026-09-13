using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using PersonalWorkstation.Services;
using PersonalWorkstation.Views;
using PersonalWorkstation.ViewModels;

namespace PersonalWorkstation;

public partial class App : Application
{
    private Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var options = StartupOptions.Parse(e.Args);
        LogService.LogDir = Path.Combine(options.DataDir ?? AppContext.BaseDirectory, "logs");

        DispatcherUnhandledException += (_, args) =>
        {
            LogService.Error("界面线程出现未处理异常", args.Exception);
            MessageBox.Show(
                $"程序遇到一个错误：{args.Exception.Message}\n\n详细信息已写入 logs 目录，程序会尽量继续运行。",
                "出错了", MessageBoxButton.OK, MessageBoxImage.Warning);
            args.Handled = true;
        };

        if (options.SelfTest)
        {
            var selfTestCode = SelfTest.Run(options.DataDir, options.ReportPath);
            Shutdown(selfTestCode);
            return;
        }

        if (options.InitData)
        {
            // 只负责生成一份默认的空数据文件，已存在就不动它
            new DataStoreService(options.DataDir).Initialize();
            Shutdown(0);
            return;
        }

        // 单实例：已经开着的话就不再开第二个窗口
        _singleInstanceMutex = new Mutex(true, @"Local\PersonalWorkstation.SingleInstance", out var isFirst);
        if (!isFirst)
        {
            MessageBox.Show("个性化工作站已经在运行了，请在任务栏里切换到已打开的窗口。",
                "个性化工作站", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown(0);
            return;
        }

        var store = new DataStoreService(options.DataDir);
        store.Initialize();
        store.LoadWithRecovery();

        var viewModel = new MainViewModel(store, (item, tab) => ViewFactory.Create(item, tab));
        var window = new MainWindow(viewModel);
        MainWindow = window;

        if (options.UiCheckDir is not null)
        {
            // 截图自检时把窗口放到屏幕外，不打扰正在用电脑的人
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = -4000;
            window.Top = -4000;
            window.ShowInTaskbar = false;
        }

        window.Show();

        store.AutoBackupIfNeeded();
        viewModel.RefreshBackups();

        if (!string.IsNullOrEmpty(store.LastLoadNotice))
        {
            MessageBox.Show(store.LastLoadNotice, "数据文件提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        if (options.UiCheckDir is not null)
        {
            UiSnapshot.Run(window, viewModel, options.UiCheckDir);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}

/// <summary>命令行参数：--data-dir 指定数据目录；--selftest 跑数据层自检；--uicheck 输出各页面截图。</summary>
public sealed class StartupOptions
{
    public string? DataDir { get; private set; }
    public bool SelfTest { get; private set; }
    public string? ReportPath { get; private set; }
    public string? UiCheckDir { get; private set; }
    public bool InitData { get; private set; }

    public static StartupOptions Parse(string[] args)
    {
        var options = new StartupOptions();
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--data-dir" when i + 1 < args.Length:
                    options.DataDir = args[++i];
                    break;
                case "--selftest":
                    options.SelfTest = true;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--")) options.ReportPath = args[++i];
                    break;
                case "--uicheck" when i + 1 < args.Length:
                    options.UiCheckDir = args[++i];
                    break;
                case "--init-data":
                    options.InitData = true;
                    break;
            }
        }
        return options;
    }
}
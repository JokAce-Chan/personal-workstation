using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PersonalWorkstation.ViewModels;

namespace PersonalWorkstation.Services;

/// <summary>开发期用：把每个模块、每个标签页渲染成 PNG，用来检查界面结构。</summary>
public static class UiSnapshot
{
    public static void Run(Window window, MainViewModel viewModel, string outDir)
    {
        Directory.CreateDirectory(outDir);

        var jobs = new List<(string Module, string Tab)>();
        foreach (var item in viewModel.AllItems)
        {
            foreach (var tab in item.Tabs) jobs.Add((item.Key, tab.Key));
        }

        var index = 0;
        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(220) };
        timer.Tick += (_, _) =>
        {
            try
            {
                if (index >= jobs.Count)
                {
                    timer.Stop();
                    Application.Current.Shutdown(0);
                    return;
                }

                var job = jobs[index];
                viewModel.SelectByKey(job.Module);
                viewModel.SelectTabByKey(job.Tab);
                window.UpdateLayout();

                var file = Path.Combine(outDir, $"{index + 1:00}-{job.Module}-{job.Tab}.png");
                Capture(window, file);
                index++;
            }
            catch (Exception ex)
            {
                LogService.Error("界面截图失败", ex);
                timer.Stop();
                Application.Current.Shutdown(2);
            }
        };
        timer.Start();
    }

    private static void Capture(Window window, string file)
    {
        var width = (int)Math.Max(window.ActualWidth, 900);
        var height = (int)Math.Max(window.ActualHeight, 640);

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(file);
        encoder.Save(stream);
    }
}
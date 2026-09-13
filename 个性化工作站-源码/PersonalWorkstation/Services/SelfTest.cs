using System.IO;
using System.Text;
using PersonalWorkstation.Models;

namespace PersonalWorkstation.Services;

/// <summary>数据层自检：不打开窗口，验证空数据文件、保存、备份、恢复、导入、损坏兜底。</summary>
public static class SelfTest
{
    public static int Run(string? dataDir, string? reportPath)
    {
        var lines = new List<string>();
        var ok = true;
        var root = string.IsNullOrWhiteSpace(dataDir)
            ? Path.Combine(Path.GetTempPath(), "pw-selftest-" + Guid.NewGuid().ToString("n")[..8])
            : dataDir;

        void Check(string name, bool passed, string? detail = null)
        {
            ok &= passed;
            lines.Add($"{(passed ? "PASS" : "FAIL")}  {name}{(detail is null ? "" : "  → " + detail)}");
        }

        try
        {
            Directory.CreateDirectory(root);
            var store = new DataStoreService(root);
            store.Initialize();

            Check("初始化后数据文件存在", File.Exists(store.DataFile));
            var raw = File.ReadAllText(store.DataFile, Encoding.UTF8);
            Check("空数据文件非空", raw.Length > 10, $"{raw.Length} 字符");

            store.LoadWithRecovery();
            var data = store.Data;
            Check("空数据文件可解析", data.Social.Contents.Count == 0 && data.Consulting.Clients.Count == 0);
            Check("模块结构齐全",
                data.Settings is not null && data.Plans is not null && data.Fitness.Plan.Count == 0 &&
                data.Diet.Foods.Count == 0 && data.Entertainment.Library.Count == 0);

            data.Consulting.Clients.Add(new ConsultClient
            {
                Id = Helper.NewId(),
                Name = "测试客户",
                Status = "active",
                NextContactAt = Helper.Today(),
            });
            data.Social.Platforms.Add("微信公众号");
            Check("保存成功", store.Save());

            var saved = File.ReadAllText(store.DataFile, Encoding.UTF8);
            Check("中文按原样写入（未转义成 \\uXXXX）", saved.Contains("测试客户") && saved.Contains("微信公众号"));

            var backupName = store.CreateBackup();
            var backupPath = Path.Combine(store.BackupDir, backupName);
            Check("备份文件已生成", File.Exists(backupPath), backupName);
            Check("备份列表能读到", store.ListBackups().Count >= 1);

            store.ResetToEmpty();
            store.LoadWithRecovery();
            Check("清空后没有任何记录", store.Data.Consulting.Clients.Count == 0);

            var restored = store.RestoreBackup(backupName, out var restoreMessage);
            Check("恢复备份成功", restored, restoreMessage);
            store.LoadWithRecovery();
            Check("恢复后记录回来了", store.Data.Consulting.Clients.Count == 1);

            var importSource = Path.Combine(root, "import-test.json");
            data.Consulting.Clients[0].Name = "导入后的客户";
            File.WriteAllText(importSource, AppJson.Serialize(data), new UTF8Encoding(false));
            var imported = store.ImportFromFile(importSource, out var importMessage);
            Check("从文件导入成功", imported, importMessage);
            Check("导入内容生效", store.Data.Consulting.Clients[0].Name == "导入后的客户");

            File.WriteAllText(store.DataFile, "{ 这不是合法的 JSON", new UTF8Encoding(false));
            store.LoadWithRecovery();
            var valid = AppJson.Deserialize<AppData>(File.ReadAllText(store.DataFile, Encoding.UTF8));
            Check("数据文件损坏时自动兜底", valid is not null && store.Data is not null,
                store.LastLoadNotice ?? "无提示");
            Check("损坏文件被另存保留",
                Directory.EnumerateFiles(store.DataDir, "损坏的数据_*.json").Any());

            var auto = store.AutoBackupIfNeeded();
            Check("自动备份逻辑可运行", true, auto is null ? "本次无需备份" : auto);
        }
        catch (Exception ex)
        {
            ok = false;
            lines.Add($"FAIL  自检过程中抛出异常  → {ex}");
        }

        lines.Insert(0, $"个性化工作站 数据层自检  {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        lines.Insert(1, $"数据目录：{root}");
        lines.Insert(2, ok ? "结论：全部通过" : "结论：存在失败项");
        lines.Insert(3, "");

        var report = reportPath ?? Path.Combine(root, "selftest-report.txt");
        try
        {
            File.WriteAllText(report, string.Join(Environment.NewLine, lines), new UTF8Encoding(false));
        }
        catch
        {
            // 报告写不出来也不影响退出码
        }

        Console.WriteLine(string.Join(Environment.NewLine, lines));
        return ok ? 0 : 1;
    }
}
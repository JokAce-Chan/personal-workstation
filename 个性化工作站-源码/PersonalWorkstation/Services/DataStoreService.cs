using System.IO;
using System.Text;
using PersonalWorkstation.Models;

namespace PersonalWorkstation.Services;

public sealed class BackupInfo
{
    public string FileName { get; set; } = "";
    public string FullPath { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    public string SizeText => SizeBytes < 1024
        ? $"{SizeBytes} B"
        : SizeBytes < 1024 * 1024
            ? $"{SizeBytes / 1024.0:0.#} KB"
            : $"{SizeBytes / 1024.0 / 1024.0:0.##} MB";

    public string CreatedText => CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
}

/// <summary>
/// 数据文件与备份的管理者：负责读写、原子替换、备份、恢复、导入。
/// 目录结构：&lt;程序目录&gt;\data\工作站数据.json 与 &lt;程序目录&gt;\backups\。
/// </summary>
public sealed class DataStoreService
{
    public const string DataFileName = "工作站数据.json";
    private const int MaxBackups = 60;

    private readonly object _gate = new();

    public string AppDir { get; }
    public string DataDir { get; }
    public string BackupDir { get; }
    public string DataFile { get; }

    public AppData Data { get; private set; } = new();

    /// <summary>最近一次加载或保存的错误信息，供界面提示。</summary>
    public string? LastError { get; private set; }

    public string? LastLoadNotice { get; private set; }

    public DateTime? LastSavedAt { get; private set; }

    public DataStoreService(string? dataDirOverride = null)
    {
        AppDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        DataDir = string.IsNullOrWhiteSpace(dataDirOverride)
            ? Path.Combine(AppDir, "data")
            : Path.GetFullPath(dataDirOverride);
        BackupDir = Path.Combine(DataDir, "backups");
        DataFile = Path.Combine(DataDir, DataFileName);
    }

    /// <summary>确保 data / backups 目录和空数据文件存在。</summary>
    public void Initialize()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(BackupDir);
        if (!File.Exists(DataFile) || new FileInfo(DataFile).Length == 0)
        {
            WriteAtomic(DataFile, AppJson.Serialize(new AppData()));
            LogService.Info($"已创建空数据文件：{DataFile}");
        }
    }

    public void Load()
    {
        lock (_gate)
        {
            LastError = null;
            LastLoadNotice = null;
            Data = LoadFrom(DataFile, out var notice) ?? new AppData();
            if (notice is not null) LastLoadNotice = notice;
            LastSavedAt = File.Exists(DataFile) ? File.GetLastWriteTime(DataFile) : null;
        }
    }

    /// <summary>从指定文件读取；失败时返回 null，并给出提示文本。</summary>
    private AppData? LoadFrom(string path, out string? notice)
    {
        notice = null;
        try
        {
            if (!File.Exists(path)) return null;
            var text = File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(text)) return null;
            var data = AppJson.Deserialize<AppData>(text);
            if (data is null) return null;
            Normalize(data);
            return data;
        }
        catch (Exception ex)
        {
            notice = $"数据文件无法解析：{ex.Message}";
            LogService.Error($"读取数据文件失败：{path}", ex);
            return null;
        }
    }

    /// <summary>把数据文件恢复成可用状态：优先用最近备份，否则保留坏文件并重建空数据。</summary>
    public void LoadWithRecovery()
    {
        lock (_gate)
        {
            var data = LoadFrom(DataFile, out var notice);
            if (data is not null)
            {
                Data = data;
                LastSavedAt = File.GetLastWriteTime(DataFile);
                return;
            }

            // 先把手里的坏文件挪到一边留底，绝不直接覆盖
            string? brokenName = null;
            if (File.Exists(DataFile) && new FileInfo(DataFile).Length > 0)
            {
                brokenName = $"损坏的数据_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                try
                {
                    File.Move(DataFile, Path.Combine(DataDir, brokenName), true);
                }
                catch (Exception ex)
                {
                    LogService.Error("保留损坏数据文件失败", ex);
                    brokenName = null;
                }
            }

            var newest = ListBackups().FirstOrDefault();
            if (newest is not null)
            {
                var fromBackup = LoadFrom(newest.FullPath, out _);
                if (fromBackup is not null)
                {
                    Data = fromBackup;
                    Save();
                    LastLoadNotice =
                        $"数据文件异常，已自动用最近备份恢复（{newest.CreatedText}）。" +
                        (brokenName is null ? "" : $" 出问题的原文件已另存为 {brokenName}，没有丢。");
                    LogService.Info(LastLoadNotice);
                    return;
                }
            }

            LastLoadNotice = brokenName is null
                ? (notice ?? "数据文件不存在，已重建空数据文件。")
                : $"{(notice ?? "数据文件不可用")}，原文件已另存为 {brokenName}，已重建空数据文件。";

            Data = new AppData();
            Save();
        }
    }

    public bool Save()
    {
        lock (_gate)
        {
            try
            {
                Data.UpdatedAt = Helper.Now();
                WriteAtomic(DataFile, AppJson.Serialize(Data));
                LastSavedAt = DateTime.Now;
                LastError = null;
                return true;
            }
            catch (Exception ex)
            {
                LastError = $"保存失败：{ex.Message}";
                LogService.Error("保存数据文件失败", ex);
                return false;
            }
        }
    }

    public void ReplaceData(AppData data)
    {
        lock (_gate)
        {
            Normalize(data);
            Data = data;
        }
    }

    /// <summary>重建为空数据（各模块无任何记录）。</summary>
    public void ResetToEmpty()
    {
        ReplaceData(new AppData());
        Save();
    }

    // ── 备份 ────────────────────────────────────────────────────────────

    public string CreateBackup(string prefix = "工作站备份")
    {
        lock (_gate)
        {
            Directory.CreateDirectory(BackupDir);
            if (!File.Exists(DataFile)) Save();

            var name = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var target = Path.Combine(BackupDir, name);
            var index = 1;
            while (File.Exists(target))
            {
                name = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}_{index}.json";
                target = Path.Combine(BackupDir, name);
                index++;
            }

            File.Copy(DataFile, target, true);
            LogService.Info($"已创建备份：{name}");
            PruneBackups();
            return name;
        }
    }

    /// <summary>数据文件比最新备份更新时自动备份，避免无意义的重复备份。</summary>
    public string? AutoBackupIfNeeded()
    {
        lock (_gate)
        {
            if (!File.Exists(DataFile)) return null;
            if (new FileInfo(DataFile).Length == 0) return null;
            var newest = ListBackups().FirstOrDefault();
            if (newest is not null && newest.CreatedAt >= File.GetLastWriteTime(DataFile)) return null;
            return CreateBackup();
        }
    }

    public List<BackupInfo> ListBackups()
    {
        var list = new List<BackupInfo>();
        try
        {
            if (!Directory.Exists(BackupDir)) return list;
            foreach (var file in Directory.EnumerateFiles(BackupDir, "*.json"))
            {
                var info = new FileInfo(file);
                list.Add(new BackupInfo
                {
                    FileName = info.Name,
                    FullPath = info.FullName,
                    SizeBytes = info.Length,
                    CreatedAt = info.LastWriteTime,
                });
            }
            list.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
        }
        catch (Exception ex)
        {
            LogService.Error("列出备份失败", ex);
        }
        return list;
    }

    public bool RestoreBackup(string fileName, out string message)
    {
        lock (_gate)
        {
            message = "";
            var safeName = Path.GetFileName(fileName);
            if (safeName != fileName)
            {
                message = "备份文件名不合法。";
                return false;
            }
            var path = Path.Combine(BackupDir, safeName);
            if (!File.Exists(path))
            {
                message = "找不到该备份文件。";
                return false;
            }

            var data = LoadFrom(path, out var notice);
            if (data is null)
            {
                message = $"备份内容不可用：{notice}";
                return false;
            }

            CreateBackup("恢复前自动备份");
            Data = data;
            if (!Save())
            {
                message = LastError ?? "恢复后保存失败。";
                return false;
            }
            message = $"已恢复到 {safeName}（恢复前的数据已另存为备份）。";
            return true;
        }
    }

    public bool ImportFromFile(string path, out string message)
    {
        lock (_gate)
        {
            message = "";
            if (!File.Exists(path))
            {
                message = "找不到要导入的文件。";
                return false;
            }
            var data = LoadFrom(path, out var notice);
            if (data is null)
            {
                message = $"导入失败，文件内容不是有效的数据文件：{notice}";
                return false;
            }
            CreateBackup("导入前自动备份");
            Data = data;
            Save();
            message = "导入完成，导入前的数据已另存为备份。";
            return true;
        }
    }

    private void PruneBackups()
    {
        try
        {
            var all = ListBackups();
            foreach (var item in all.Skip(MaxBackups))
            {
                File.Delete(item.FullPath);
                LogService.Info($"已清理旧备份：{item.FileName}");
            }
        }
        catch (Exception ex)
        {
            LogService.Error("清理旧备份失败", ex);
        }
    }

    // ── 基础设施 ────────────────────────────────────────────────────────

    private static void WriteAtomic(string path, string text)
    {
        var dir = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(dir);
        var temp = Path.Combine(dir, Path.GetFileName(path) + ".tmp");
        File.WriteAllText(temp, text, new UTF8Encoding(false));

        if (File.Exists(path))
        {
            try
            {
                File.Replace(temp, path, null, true);
                return;
            }
            catch
            {
                // 某些磁盘不支持 Replace，退回直接覆盖
            }
        }
        File.Move(temp, path, true);
    }

    /// <summary>补齐缺失的字段，保证界面不会因为旧文件缺字段而崩溃。</summary>
    public static void Normalize(AppData data)
    {
        data.Settings ??= new AppSettings();
        data.QuickNotes ??= new List<QuickNote>();
        data.Plans ??= new Dictionary<string, List<PlanTask>>();
        data.Social ??= new SocialData();
        data.Social.Platforms ??= new List<string>();
        data.Social.Contents ??= new List<SocialContent>();
        data.Social.Ideas ??= new List<SocialIdea>();
        data.Dev ??= new DevData();
        data.Dev.Projects ??= new List<DevProject>();
        data.Dev.Items ??= new List<DevItem>();
        data.Dev.Snippets ??= new List<DevSnippet>();
        data.Consulting ??= new ConsultingData();
        data.Consulting.Clients ??= new List<ConsultClient>();
        data.Consulting.Projects ??= new List<ConsultProject>();
        data.Consulting.Logs ??= new List<ConsultLog>();
        data.Fitness ??= new FitnessData();
        data.Fitness.Goals ??= new FitnessGoals();
        data.Fitness.Plan ??= new List<FitnessDayPlan>();
        data.Fitness.Sessions ??= new List<FitnessSession>();
        data.Fitness.Body ??= new List<BodyRecord>();
        data.Diet ??= new DietData();
        data.Diet.Targets ??= new DietTargets();
        data.Diet.Foods ??= new List<FoodItem>();
        data.Diet.Days ??= new Dictionary<string, DietDay>();
        data.Entertainment ??= new EntertainmentData();
        data.Entertainment.Library ??= new List<MediaItem>();
        data.Entertainment.Sessions ??= new List<MediaSession>();
    }
}
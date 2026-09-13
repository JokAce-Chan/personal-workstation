using System.Collections.Generic;

namespace PersonalWorkstation.Models;

/// <summary>数据文件的根对象。所有模块的数据都挂在这里。</summary>
public sealed class AppData
{
    public int Version { get; set; } = 1;
    public string UpdatedAt { get; set; } = "";

    public AppSettings Settings { get; set; } = new();
    public List<QuickNote> QuickNotes { get; set; } = new();
    public Dictionary<string, List<PlanTask>> Plans { get; set; } = new();

    public SocialData Social { get; set; } = new();
    public DevData Dev { get; set; } = new();
    public ConsultingData Consulting { get; set; } = new();
    public FitnessData Fitness { get; set; } = new();
    public DietData Diet { get; set; } = new();
    public EntertainmentData Entertainment { get; set; } = new();
}

public sealed class AppSettings
{
    /// <summary>主题：light / dark / system</summary>
    public string Theme { get; set; } = "light";
    /// <summary>数据与设置以外的左侧导航是否可见</summary>
    public bool NavVisible { get; set; } = true;
    /// <summary>左侧导航是否折叠为窄条</summary>
    public bool NavCollapsed { get; set; } = false;
    /// <summary>被折叠的导航分组 Key</summary>
    public List<string> CollapsedGroups { get; set; } = new();
    public double WindowWidth { get; set; } = 1360;
    public double WindowHeight { get; set; } = 860;
    public int WindowLeft { get; set; } = -1;
    public int WindowTop { get; set; } = -1;
    public bool WindowMaximized { get; set; } = false;
    public string FontScale { get; set; } = "normal";
}

public sealed class QuickNote
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public bool Pinned { get; set; }
}

public sealed class PlanTask
{
    public string Id { get; set; } = "";
    /// <summary>high / normal / low</summary>
    public string Priority { get; set; } = "normal";
    public string Title { get; set; } = "";
    /// <summary>模块标签，例如 开发 / 咨询 / 自媒体 / 健身 / 饮食 / 娱乐 / 生活 / 其他</summary>
    public string Tag { get; set; } = "";
    public bool Done { get; set; }
    public string CreatedAt { get; set; } = "";
    public string DoneAt { get; set; } = "";
    public int Sort { get; set; }
}

// ── 自媒体 ──────────────────────────────────────────────────────────────
public sealed class SocialData
{
    public List<string> Platforms { get; set; } = new();
    public List<SocialContent> Contents { get; set; } = new();
    public List<SocialIdea> Ideas { get; set; } = new();
}

public sealed class SocialContent
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public List<string> Platforms { get; set; } = new();
    /// <summary>idea / draft / ready / published / dropped</summary>
    public string Status { get; set; } = "idea";
    public string PlanDate { get; set; } = "";
    public string PublishDate { get; set; } = "";
    public string Script { get; set; } = "";
    public List<AssetItem> Assets { get; set; } = new();
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public int Followers { get; set; }
    public string Retro { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public string CreatedAt { get; set; } = "";
}

public sealed class AssetItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Done { get; set; }
}

public sealed class SocialIdea
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public string CreatedAt { get; set; } = "";
}

// ── 开发工作 ────────────────────────────────────────────────────────────
public sealed class DevData
{
    public List<DevProject> Projects { get; set; } = new();
    public List<DevItem> Items { get; set; } = new();
    public List<DevSnippet> Snippets { get; set; } = new();
}

public sealed class DevProject
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Summary { get; set; } = "";
    /// <summary>planning / active / paused / done</summary>
    public string Status { get; set; } = "active";
    /// <summary>high / normal / low</summary>
    public string Priority { get; set; } = "normal";
    public string Deadline { get; set; } = "";
    public string Repo { get; set; } = "";
    public string Note { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
}

public sealed class DevItem
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    /// <summary>task / bug / idea</summary>
    public string Type { get; set; } = "task";
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    /// <summary>high / normal / low</summary>
    public string Priority { get; set; } = "normal";
    /// <summary>todo / doing / done</summary>
    public string Status { get; set; } = "todo";
    public string CreatedAt { get; set; } = "";
    public string DoneAt { get; set; } = "";
}

public sealed class DevSnippet
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Language { get; set; } = "";
    public string Code { get; set; } = "";
    public string Note { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}

// ── 咨询工作 ────────────────────────────────────────────────────────────
public sealed class ConsultingData
{
    public List<ConsultClient> Clients { get; set; } = new();
    public List<ConsultProject> Projects { get; set; } = new();
    public List<ConsultLog> Logs { get; set; } = new();
}

public sealed class ConsultClient
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Company { get; set; } = "";
    public string Contact { get; set; } = "";
    public string Source { get; set; } = "";
    /// <summary>lead / active / paused / closed</summary>
    public string Status { get; set; } = "lead";
    public string Note { get; set; } = "";
    public string NextContactAt { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}

public sealed class ConsultProject
{
    public string Id { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Requirement { get; set; } = "";
    /// <summary>talking / active / delivering / done / paused</summary>
    public string Status { get; set; } = "active";
    public string StartDate { get; set; } = "";
    public string Deadline { get; set; } = "";
    public string NextContactAt { get; set; } = "";
    public List<Deliverable> Deliverables { get; set; } = new();
    public List<FollowUp> FollowUps { get; set; } = new();
    public string Note { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}

public sealed class Deliverable
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Due { get; set; } = "";
    /// <summary>todo / doing / done</summary>
    public string Status { get; set; } = "todo";
    public bool Done { get; set; }
}

public sealed class FollowUp
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string Due { get; set; } = "";
    public bool Done { get; set; }
}

public sealed class ConsultLog
{
    public string Id { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string Date { get; set; } = "";
    /// <summary>meeting / call / chat / mail / note</summary>
    public string Type { get; set; } = "meeting";
    public int Minutes { get; set; }
    public string Summary { get; set; } = "";
    public string Notes { get; set; } = "";
    /// <summary>none / hourly / fixed</summary>
    public string Billing { get; set; } = "none";
    public double Rate { get; set; }
    public double Amount { get; set; }
}

// ── 健身计划 ────────────────────────────────────────────────────────────
public sealed class FitnessData
{
    public FitnessGoals Goals { get; set; } = new();
    public List<FitnessDayPlan> Plan { get; set; } = new();
    public List<FitnessSession> Sessions { get; set; } = new();
    public List<BodyRecord> Body { get; set; } = new();
}

public sealed class FitnessGoals
{
    public double TargetWeight { get; set; }
    public int WeeklySessions { get; set; }
    public string Note { get; set; } = "";
}

public sealed class FitnessDayPlan
{
    public string Id { get; set; } = "";
    /// <summary>1=周一 ... 7=周日</summary>
    public int Weekday { get; set; }
    public string Title { get; set; } = "";
    public string Note { get; set; } = "";
    public List<FitnessExercise> Exercises { get; set; } = new();
}

public sealed class FitnessExercise
{
    public string Name { get; set; } = "";
    public int Sets { get; set; }
    public string Reps { get; set; } = "";
    public string Weight { get; set; } = "";
    public string Note { get; set; } = "";
}

public sealed class FitnessSession
{
    public string Id { get; set; } = "";
    public string Date { get; set; } = "";
    public string Title { get; set; } = "";
    public int Duration { get; set; }
    /// <summary>great / ok / tired</summary>
    public string Feeling { get; set; } = "ok";
    public string Note { get; set; } = "";
    public List<FitnessSetLog> Logs { get; set; } = new();
}

public sealed class FitnessSetLog
{
    public string Exercise { get; set; } = "";
    public int Sets { get; set; }
    public string Reps { get; set; } = "";
    public string Weight { get; set; } = "";
}

public sealed class BodyRecord
{
    public string Id { get; set; } = "";
    public string Date { get; set; } = "";
    public double Weight { get; set; }
    public double BodyFat { get; set; }
    public double Waist { get; set; }
    public double Chest { get; set; }
    public double Arm { get; set; }
    public double Thigh { get; set; }
    public string Note { get; set; } = "";
}

// ── 饮食计划 ────────────────────────────────────────────────────────────
public sealed class DietData
{
    public DietTargets Targets { get; set; } = new();
    public List<FoodItem> Foods { get; set; } = new();
    public Dictionary<string, DietDay> Days { get; set; } = new();
}

public sealed class DietTargets
{
    public double Kcal { get; set; } = 2000;
    public double Protein { get; set; } = 120;
    public double Carb { get; set; } = 230;
    public double Fat { get; set; } = 65;
    public int Water { get; set; } = 2000;
}

public sealed class FoodItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>单位说明，例如 100克 / 1个 / 1勺</summary>
    public string Unit { get; set; } = "100克";
    public double Kcal { get; set; }
    public double Protein { get; set; }
    public double Carb { get; set; }
    public double Fat { get; set; }
    public bool Favorite { get; set; }
    public string Note { get; set; } = "";
}

public sealed class DietDay
{
    /// <summary>Key: breakfast / lunch / dinner / snack</summary>
    public Dictionary<string, List<DietEntry>> Meals { get; set; } = new();
    public int Water { get; set; }
    public string Note { get; set; } = "";
}

public sealed class DietEntry
{
    public string Id { get; set; } = "";
    public string FoodId { get; set; } = "";
    public string Name { get; set; } = "";
    public double Amount { get; set; } = 1;
    public string Unit { get; set; } = "";
    public double Kcal { get; set; }
    public double Protein { get; set; }
    public double Carb { get; set; }
    public double Fat { get; set; }
}

// ── 游戏娱乐 ────────────────────────────────────────────────────────────
public sealed class EntertainmentData
{
    public List<MediaItem> Library { get; set; } = new();
    public List<MediaSession> Sessions { get; set; } = new();
}

public sealed class MediaItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>game / movie / tv / book</summary>
    public string Type { get; set; } = "game";
    /// <summary>wishlist / playing / finished / dropped</summary>
    public string Status { get; set; } = "wishlist";
    public string Platform { get; set; } = "";
    public double Rating { get; set; }
    public string Price { get; set; } = "";
    public string Note { get; set; } = "";
    public string StartedAt { get; set; } = "";
    public string FinishedAt { get; set; } = "";
}

public sealed class MediaSession
{
    public string Id { get; set; } = "";
    public string ItemId { get; set; } = "";
    public string Date { get; set; } = "";
    public int Minutes { get; set; }
    public string Note { get; set; } = "";
}
using System.Collections.Generic;

namespace PersonalWorkstation.Models;

/// <summary>
/// 数据文件的根对象。所有模块的数据都挂在这里。
/// 模块与标签页的分布以《界面设计验收.html》和《初版PRD.md》第 2、5 章为准。
/// 导航顺序：首页总览 / 今日计划 / 课程与作业 / 项目实战 / 刷题算法 /
///           实习求职 / 竞赛与证书 / 技术输出 / 健身计划 / 饮食计划 / 游戏娱乐 / 数据与设置。
/// </summary>
public sealed class AppData
{
    /// <summary>数据结构版本。工作版为 1，按验收稿改为学生版后升到 2。</summary>
    public int Version { get; set; } = 2;
    public string UpdatedAt { get; set; } = "";

    public AppSettings Settings { get; set; } = new();
    public HomeData Home { get; set; } = new();
    public List<QuickNote> QuickNotes { get; set; } = new();
    /// <summary>今日计划。键是日期 yyyy-MM-dd，值是该天的任务列表。</summary>
    public Dictionary<string, List<PlanTask>> Plans { get; set; } = new();

    public CourseData Course { get; set; } = new();
    public ProjectData Project { get; set; } = new();
    public AlgoData Algo { get; set; } = new();
    public CareerData Career { get; set; } = new();
    public ContestData Contest { get; set; } = new();
    public BlogData Blog { get; set; } = new();
    public FitnessData Fitness { get; set; } = new();
    public DietData Diet { get; set; } = new();
    public EntertainmentData Entertainment { get; set; } = new();
}

public sealed class AppSettings
{
    /// <summary>主题：light / dark / system</summary>
    public string Theme { get; set; } = "light";
    /// <summary>左侧导航是否可见</summary>
    public bool NavVisible { get; set; } = true;
    /// <summary>左侧导航是否折叠为窄图标条</summary>
    public bool NavCollapsed { get; set; } = false;
    /// <summary>导航栏宽度，设计范围为 180 ~ 420，默认 236</summary>
    public double NavWidth { get; set; } = 236;
    /// <summary>被折叠的导航分组 Key</summary>
    public List<string> CollapsedGroups { get; set; } = new();
    /// <summary>当前学期，用于课程与作业模块按学期切换</summary>
    public string CurrentTerm { get; set; } = "";
    public double WindowWidth { get; set; } = 1360;
    public double WindowHeight { get; set; } = 860;
    public int WindowLeft { get; set; } = -1;
    public int WindowTop { get; set; } = -1;
    public bool WindowMaximized { get; set; } = false;
    public string FontScale { get; set; } = "normal";
}

/// <summary>首页总览：只存布局状态，不存业务数据。</summary>
public sealed class HomeData
{
    /// <summary>卡片顺序（卡片 Key 列表）</summary>
    public List<string> CardOrder { get; set; } = new();
    /// <summary>被隐藏的卡片 Key</summary>
    public List<string> HiddenCards { get; set; } = new();
    /// <summary>提醒条最后已读时间</summary>
    public string ReminderReadAt { get; set; } = "";
}

public sealed class QuickNote
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public bool Pinned { get; set; }
}

// ── 今日计划 ────────────────────────────────────────────────────────────

public sealed class PlanTask
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>high / normal / low</summary>
    public string Priority { get; set; } = "normal";
    /// <summary>模块标签：课程 / 项目 / 刷题 / 求职 / 竞赛 / 技术输出 / 健身 / 饮食 / 娱乐 / 生活 / 其他</summary>
    public string Tag { get; set; } = "";
    /// <summary>关联来源，如某条 DDL 或某个项目待办的标识；只做跳转，不复制数据</summary>
    public string Source { get; set; } = "";
    public bool Done { get; set; }
    public string CreatedAt { get; set; } = "";
    public string DoneAt { get; set; } = "";
    public int Sort { get; set; }
}

// ── 课程与作业 ──────────────────────────────────────────────────────────

public sealed class CourseData
{
    public List<Course> Courses { get; set; } = new();
    public List<CourseSlot> Slots { get; set; } = new();
    public List<Homework> Homeworks { get; set; } = new();
    public List<Exam> Exams { get; set; } = new();
    public List<ReviewItem> ReviewItems { get; set; } = new();
}

public sealed class Course
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Teacher { get; set; } = "";
    public string Room { get; set; } = "";
    public double Credits { get; set; }
    /// <summary>起始周</summary>
    public int WeekFrom { get; set; } = 1;
    /// <summary>结束周</summary>
    public int WeekTo { get; set; } = 18;
    /// <summary>单双周：all / odd / even</summary>
    public string WeekParity { get; set; } = "all";
    /// <summary>颜色标签</summary>
    public string Color { get; set; } = "";
    public string Term { get; set; } = "";
    public string Note { get; set; } = "";
}

/// <summary>课表中的一个格子：星期 × 节次 → 课程，可标记停课与调课。</summary>
public sealed class CourseSlot
{
    public string Id { get; set; } = "";
    public string CourseId { get; set; } = "";
    /// <summary>1=周一 ... 7=周日</summary>
    public int Weekday { get; set; }
    /// <summary>起始节次</summary>
    public int PeriodFrom { get; set; }
    /// <summary>结束节次</summary>
    public int PeriodTo { get; set; }
    public string Room { get; set; } = "";
    public bool Cancelled { get; set; }
    /// <summary>调课说明，例如「调至周三 5-6 节」</summary>
    public string AdjustedTo { get; set; } = "";
    public string Note { get; set; } = "";
}

public sealed class Homework
{
    public string Id { get; set; } = "";
    public string CourseId { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>homework / lab / design / report</summary>
    public string Type { get; set; } = "homework";
    public string AssignedAt { get; set; } = "";
    public string DueAt { get; set; } = "";
    /// <summary>todo / doing / tosubmit / submitted / graded</summary>
    public string Status { get; set; } = "todo";
    public string SubmittedAt { get; set; } = "";
    public string Score { get; set; } = "";
    public string Note { get; set; } = "";
}

public sealed class Exam
{
    public string Id { get; set; } = "";
    public string CourseId { get; set; } = "";
    public string Title { get; set; } = "";
    public string ExamAt { get; set; } = "";
    public string Room { get; set; } = "";
    /// <summary>closed / open / machine / project</summary>
    public string Mode { get; set; } = "closed";
    public string Note { get; set; } = "";
}

public sealed class ReviewItem
{
    public string Id { get; set; } = "";
    public string CourseId { get; set; } = "";
    /// <summary>知识点条目</summary>
    public string Title { get; set; } = "";
    public bool Mastered { get; set; }
    public string Note { get; set; } = "";
}

// ── 项目实战 ────────────────────────────────────────────────────────────

public sealed class ProjectData
{
    public List<Project> Projects { get; set; } = new();
    public List<ProjectMember> Members { get; set; } = new();
    public List<ProjectTask> Tasks { get; set; } = new();
    public List<Milestone> Milestones { get; set; } = new();
    public List<MeetingNote> Meetings { get; set; } = new();
    public List<CodeSnippet> Snippets { get; set; } = new();
    public List<TechNote> Notes { get; set; } = new();
}

public sealed class Project
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>course / group / personal / opensource</summary>
    public string Type { get; set; } = "personal";
    public string Summary { get; set; } = "";
    /// <summary>planning / active / paused / done</summary>
    public string Status { get; set; } = "active";
    /// <summary>技术栈，多个用逗号分隔</summary>
    public string TechStack { get; set; } = "";
    /// <summary>仓库地址，纯文本，不做跳转</summary>
    public string Repo { get; set; } = "";
    public string StartDate { get; set; } = "";
    public string Deadline { get; set; } = "";
    public string Note { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
}

public sealed class ProjectMember
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string Name { get; set; } = "";
    public string StudentNo { get; set; } = "";
    /// <summary>leader / frontend / backend / test / doc</summary>
    public string Role { get; set; } = "backend";
    public string Contact { get; set; } = "";
}

public sealed class ProjectTask
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    /// <summary>requirement / dev / bug / doc / idea</summary>
    public string Type { get; set; } = "dev";
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    /// <summary>high / normal / low</summary>
    public string Priority { get; set; } = "normal";
    /// <summary>负责人，存成员 Id</summary>
    public string OwnerId { get; set; } = "";
    /// <summary>todo / doing / done</summary>
    public string Status { get; set; } = "todo";
    public string CreatedAt { get; set; } = "";
    public string DoneAt { get; set; } = "";
}

public sealed class Milestone
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    /// <summary>需求评审 / 概要设计 / 编码完成 / 测试 / 验收</summary>
    public string Name { get; set; } = "";
    public string DueAt { get; set; } = "";
    public bool Done { get; set; }
}

public sealed class MeetingNote
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string Date { get; set; } = "";
    public string Attendees { get; set; } = "";
    public string Conclusion { get; set; } = "";
    /// <summary>会议产生的待办，多条用换行分隔</summary>
    public string Todos { get; set; } = "";
}

public sealed class CodeSnippet
{
    public string Id { get; set; } = "";
    /// <summary>所属项目，可空表示公共片段</summary>
    public string ProjectId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Language { get; set; } = "";
    public string Code { get; set; } = "";
    public string Note { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public string CreatedAt { get; set; } = "";
}

public sealed class TechNote
{
    public string Id { get; set; } = "";
    /// <summary>所属项目，可空</summary>
    public string ProjectId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
}

// ── 刷题算法 ────────────────────────────────────────────────────────────

public sealed class AlgoData
{
    public List<AlgoProblem> Problems { get; set; } = new();
    public List<AlgoTopic> Topics { get; set; } = new();
    // 错题本不单独存储：由状态为 review 的题目自动汇聚。
}

public sealed class AlgoProblem
{
    public string Id { get; set; } = "";
    /// <summary>LeetCode / 洛谷 / 牛客 / Codeforces</summary>
    public string Platform { get; set; } = "LeetCode";
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>easy / medium / hard</summary>
    public string Difficulty { get; set; } = "medium";
    public List<string> Tags { get; set; } = new();
    public string SolvedDate { get; set; } = "";
    /// <summary>todo / solved / review / dropped</summary>
    public string Status { get; set; } = "todo";
    /// <summary>耗时（分钟）</summary>
    public int Minutes { get; set; }
    public int SubmitCount { get; set; }
    /// <summary>思路与关键代码要点</summary>
    public string Thinking { get; set; } = "";
    public string Code { get; set; } = "";
    public int ReviewCount { get; set; }
    public string LastReviewAt { get; set; } = "";
    /// <summary>下次复习日期，按 1 / 3 / 7 / 30 天节奏推进</summary>
    public string NextReviewAt { get; set; } = "";
}

public sealed class AlgoTopic
{
    public string Id { get; set; } = "";
    /// <summary>数组、字符串、链表、栈与队列、树、图、排序、二分、双指针、贪心、动态规划、回溯……</summary>
    public string Name { get; set; } = "";
    /// <summary>目标题数</summary>
    public int Target { get; set; }
    /// <summary>已刷题数</summary>
    public int Done { get; set; }
    /// <summary>掌握度 0-100</summary>
    public int Mastery { get; set; }
    public string Note { get; set; } = "";
}

// ── 实习求职 ────────────────────────────────────────────────────────────

public sealed class CareerData
{
    public List<Company> Companies { get; set; } = new();
    public List<Application> Applications { get; set; } = new();
    public List<Interview> Interviews { get; set; } = new();
    public List<Offer> Offers { get; set; } = new();
    public List<ResumeVersion> Resumes { get; set; } = new();
}

public sealed class Company
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Industry { get; set; } = "";
    /// <summary>岗位方向</summary>
    public string Position { get; set; } = "";
    public string City { get; set; } = "";
    /// <summary>high / normal / low</summary>
    public string Priority { get; set; } = "normal";
    /// <summary>招聘时间线</summary>
    public string Timeline { get; set; } = "";
    /// <summary>todo / applied / written / interviewing / closed</summary>
    public string Status { get; set; } = "todo";
    public string Note { get; set; } = "";
}

public sealed class Application
{
    public string Id { get; set; } = "";
    public string CompanyId { get; set; } = "";
    public string Position { get; set; } = "";
    /// <summary>官网 / 内推 / BOSS / 牛客 / 实习僧</summary>
    public string Channel { get; set; } = "";
    public string AppliedAt { get; set; } = "";
    /// <summary>所用简历版本</summary>
    public string ResumeVersion { get; set; } = "";
    /// <summary>sent / written / interviewing / offer / rejected</summary>
    public string Status { get; set; } = "sent";
    public string NextAction { get; set; } = "";
    public string NextActionDue { get; set; } = "";
    public string Note { get; set; } = "";
}

public sealed class Interview
{
    public string Id { get; set; } = "";
    public string ApplicationId { get; set; } = "";
    public string CompanyId { get; set; } = "";
    public string Position { get; set; } = "";
    /// <summary>第几轮</summary>
    public int Round { get; set; } = 1;
    public string Date { get; set; } = "";
    /// <summary>phone / video / onsite</summary>
    public string Mode { get; set; } = "video";
    public string Interviewers { get; set; } = "";
    /// <summary>问题清单（技术题、项目追问、HR 问题）</summary>
    public string Questions { get; set; } = "";
    /// <summary>回答要点</summary>
    public string Answers { get; set; } = "";
    /// <summary>复盘：答得好的、答砸的、要补的知识点</summary>
    public string Reflection { get; set; } = "";
    public string Result { get; set; } = "";
    public string NextRoundAt { get; set; } = "";
}

public sealed class Offer
{
    public string Id { get; set; } = "";
    public string CompanyId { get; set; } = "";
    public string Position { get; set; } = "";
    /// <summary>日薪或月薪，自由文本</summary>
    public string Salary { get; set; } = "";
    public string City { get; set; } = "";
    public string RegularChance { get; set; } = "";
    public string Benefits { get; set; } = "";
    public string OnboardDate { get; set; } = "";
    public string ReplyDeadline { get; set; } = "";
    /// <summary>considering / accepted / declined</summary>
    public string Status { get; set; } = "considering";
    public string Note { get; set; } = "";
}

public sealed class ResumeVersion
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
}

// ── 竞赛与证书 ──────────────────────────────────────────────────────────

public sealed class ContestData
{
    public List<Contest> Contests { get; set; } = new();
    public List<Certification> Certifications { get; set; } = new();
}

public sealed class Contest
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>school / provincial / national / international</summary>
    public string Level { get; set; } = "school";
    /// <summary>algorithm / modeling / innovation / application</summary>
    public string Type { get; set; } = "algorithm";
    public string RegisterAt { get; set; } = "";
    public string RaceAt { get; set; } = "";
    /// <summary>组队成员，多人用逗号分隔</summary>
    public string Members { get; set; } = "";
    public string Advisor { get; set; } = "";
    /// <summary>preparing / joined / resultout</summary>
    public string Status { get; set; } = "preparing";
    public string Award { get; set; } = "";
    public string Rank { get; set; } = "";
    /// <summary>备赛笔记</summary>
    public string Note { get; set; } = "";
}

public sealed class Certification
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string RegisterAt { get; set; } = "";
    public string ExamAt { get; set; } = "";
    public string Site { get; set; } = "";
    /// <summary>toregister / registered / taken / passed / failed</summary>
    public string Status { get; set; } = "toregister";
    public string Score { get; set; } = "";
    public string CertNo { get; set; } = "";
    public string ValidUntil { get; set; } = "";
    /// <summary>证书文件路径，只做记录</summary>
    public string FilePath { get; set; } = "";
}

// ── 技术输出 ────────────────────────────────────────────────────────────

public sealed class BlogData
{
    /// <summary>平台清单：CSDN / 掘金 / 知乎 / GitHub / 个人博客……</summary>
    public List<string> Platforms { get; set; } = new();
    public List<BlogContent> Contents { get; set; } = new();
    public List<BlogIdea> Ideas { get; set; } = new();
}

public sealed class BlogContent
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public List<string> Platforms { get; set; } = new();
    /// <summary>idea / draft / ready / published / dropped</summary>
    public string Status { get; set; } = "idea";
    public string PlanDate { get; set; } = "";
    public string PublishDate { get; set; } = "";
    public string Outline { get; set; } = "";
    public string Script { get; set; } = "";
    /// <summary>配套代码仓库，纯文本</summary>
    public string Repo { get; set; } = "";
    public List<AssetItem> Assets { get; set; } = new();
    // 发布后数据：全部手动填写
    public int Reads { get; set; }
    public int Likes { get; set; }
    public int Favorites { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
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

public sealed class BlogIdea
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    /// <summary>源码分析 / 算法 / 踩坑 / 工具</summary>
    public List<string> Tags { get; set; } = new();
    public string CreatedAt { get; set; } = "";
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
    /// <summary>键是日期 yyyy-MM-dd</summary>
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

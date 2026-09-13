using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersonalWorkstation.Services;

/// <summary>统一的 JSON 读写设置，保证数据文件是缩进良好、中文可读的 UTF-8 文本。</summary>
public static class AppJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string text) => JsonSerializer.Deserialize<T>(text, Options);
}

/// <summary>常用的小工具：ID、日期字符串。</summary>
public static class Helper
{
    public static string NewId() => Guid.NewGuid().ToString("n")[..12];

    public static string Now() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    public static string Today() => DateTime.Now.ToString("yyyy-MM-dd");

    public static string Date(DateTime value) => value.ToString("yyyy-MM-dd");

    public static DateTime? ParseDate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (DateTime.TryParse(text, out var value)) return value;
        return null;
    }
}
namespace unoTest.Models;

/// <summary>
/// 文件驅動 UI 定義。開發者透過 JSON/設定檔即可描述畫面結構、綁定與事件。
/// </summary>
public sealed class PluginUiTemplateDocument
{
    public string TemplateId { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0";
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 頁面初始狀態，可作為 Binding 狀態來源。
    /// </summary>
    public Dictionary<string, object?> InitialState { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public List<PluginUiSectionDocument> Sections { get; init; } = [];

    /// <summary>
    /// 可由元件事件引用（PluginUiEventBindingDocument.ActionId）。
    /// </summary>
    public List<PluginUiActionDocument> Actions { get; init; } = [];

    /// <summary>
    /// 頁面層級動作，可綁定於 Loaded/Appearing 等事件。
    /// </summary>
    public List<PluginUiActionDocument> PageActions { get; init; } = [];
}

public sealed class PluginUiSectionDocument
{
    public string Id { get; init; } = string.Empty;
    public string Header { get; init; } = string.Empty;

    /// <summary>
    /// 建議值：Stack、Grid、Wrap。
    /// </summary>
    public string Layout { get; init; } = "Stack";

    public int Columns { get; init; } = 1;

    public List<PluginUiElementDocument> Elements { get; init; } = [];
}

public sealed class PluginUiElementDocument
{
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// 建議值：TextBox、TextArea、NumberBox、ToggleSwitch、ComboBox、Button。
    /// </summary>
    public string ControlType { get; init; } = "TextBox";

    public string Label { get; init; } = string.Empty;
    public string Placeholder { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public bool IsEnabled { get; init; } = true;

    public PluginUiBindingDocument? Binding { get; init; }

    public List<PluginUiOptionDocument> Options { get; init; } = [];
    public List<PluginUiEventBindingDocument> Events { get; init; } = [];

    /// <summary>
    /// 保留擴充欄位（例如 icon/style/visibility-rule）。
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class PluginUiBindingDocument
{
    public string StateKey { get; init; } = string.Empty;

    /// <summary>
    /// 建議值：OneWay、TwoWay。
    /// </summary>
    public string Mode { get; init; } = "TwoWay";

    public string Converter { get; init; } = string.Empty;
}

public sealed class PluginUiEventBindingDocument
{
    /// <summary>
    /// 事件名稱，例如 click、changed、toggled、loaded。
    /// </summary>
    public string Name { get; init; } = "click";

    /// <summary>
    /// 對應 actions 的 Id。
    /// </summary>
    public string ActionId { get; init; } = string.Empty;
}

public sealed class PluginUiActionDocument
{
    public string Id { get; init; } = string.Empty;
    public string Plugin { get; init; } = string.Empty;
    public string Command { get; init; } = string.Empty;

    /// <summary>
    /// 傳給 plugin 的參數，值可使用樣板字串（例如 ${customerName}）。
    /// </summary>
    public Dictionary<string, string> Payload { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class PluginUiOptionDocument
{
    public string Value { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}
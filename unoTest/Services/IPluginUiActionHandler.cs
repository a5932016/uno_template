namespace unoTest.Services;

/// <summary>
/// Plugin 事件處理器。每個 plugin 以 PluginId 區分。
/// </summary>
public interface IPluginUiActionHandler
{
    string PluginId { get; }

    Task<PluginUiActionResult> ExecuteAsync(
        PluginUiActionContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 從 UI 事件派發到 plugin 時的上下文。
/// </summary>
public sealed class PluginUiActionContext
{
    public required string TemplateId { get; init; }
    public required string ActionId { get; init; }
    public required string Command { get; init; }
    public required string EventName { get; init; }
    public required string ElementId { get; init; }

    public required IReadOnlyDictionary<string, object?> State { get; init; }
    public required IReadOnlyDictionary<string, string> Payload { get; init; }
}

/// <summary>
/// Plugin 執行結果。可回傳狀態修補與訊息。
/// </summary>
public sealed class PluginUiActionResult
{
    public bool IsSuccess { get; init; } = true;
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// UI 狀態補丁，Renderer 套用後可觸發畫面更新。
    /// </summary>
    public Dictionary<string, object?> StatePatch { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public interface IPluginUiActionDispatcher
{
    Task<PluginUiActionResult> DispatchAsync(
        PluginUiActionDocument action,
        string templateId,
        string eventName,
        string elementId,
        IReadOnlyDictionary<string, object?> state,
        CancellationToken cancellationToken = default);
}
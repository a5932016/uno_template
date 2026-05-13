namespace unoTest.Services;

/// <summary>
/// 根據 action.Plugin 尋找對應 handler 並執行。
/// </summary>
public sealed class PluginUiActionDispatcher : IPluginUiActionDispatcher
{
    private readonly IReadOnlyDictionary<string, IPluginUiActionHandler> _handlers;

    public PluginUiActionDispatcher(IEnumerable<IPluginUiActionHandler> handlers)
    {
        _handlers = handlers
            .GroupBy(x => x.PluginId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
    }

    public Task<PluginUiActionResult> DispatchAsync(
        PluginUiActionDocument action,
        string templateId,
        string eventName,
        string elementId,
        IReadOnlyDictionary<string, object?> state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (string.IsNullOrWhiteSpace(action.Plugin))
        {
            return Task.FromResult(new PluginUiActionResult
            {
                IsSuccess = false,
                Message = "Action plugin is required."
            });
        }

        if (!_handlers.TryGetValue(action.Plugin, out var handler))
        {
            return Task.FromResult(new PluginUiActionResult
            {
                IsSuccess = false,
                Message = $"Plugin '{action.Plugin}' is not registered."
            });
        }

        var context = new PluginUiActionContext
        {
            TemplateId = templateId,
            ActionId = action.Id,
            Command = action.Command,
            EventName = eventName,
            ElementId = elementId,
            State = state,
            Payload = action.Payload
        };

        return handler.ExecuteAsync(context, cancellationToken);
    }
}
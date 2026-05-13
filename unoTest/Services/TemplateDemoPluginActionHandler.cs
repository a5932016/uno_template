using System.Text.Json;

namespace unoTest.Services;

/// <summary>
/// 動態模板頁的示範 plugin handler。
/// 同一套邏輯可綁定到不同 PluginId，方便演示 dispatcher 分派流程。
/// </summary>
public sealed class TemplateDemoPluginActionHandler : IPluginUiActionHandler
{
    public TemplateDemoPluginActionHandler(string pluginId)
    {
        PluginId = pluginId;
    }

    public string PluginId { get; }

    public Task<PluginUiActionResult> ExecuteAsync(
        PluginUiActionContext context,
        CancellationToken cancellationToken = default)
    {
        var command = context.Command?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(command))
        {
            return Task.FromResult(new PluginUiActionResult
            {
                IsSuccess = false,
                Message = $"Plugin '{PluginId}' command is empty."
            });
        }

        return command switch
        {
            "validateRequired" => Task.FromResult(ValidateRequired(context)),
            "validateEmail" => Task.FromResult(ValidateEmail(context)),
            "refreshCapability" => Task.FromResult(new PluginUiActionResult
            {
                IsSuccess = true,
                Message = "已更新通知管道能力設定。"
            }),
            "syncNotificationPreference" => Task.FromResult(new PluginUiActionResult
            {
                IsSuccess = true,
                Message = "已同步通知偏好設定。"
            }),
            "openPreviewDialog" => Task.FromResult(CreatePreviewResult(context)),
            "createCustomer" => Task.FromResult(new PluginUiActionResult
            {
                IsSuccess = true,
                Message = "客戶建立成功（示範 handler）。",
                StatePatch = new Dictionary<string, object?>
                {
                    ["LastSubmitStatus"] = "Success",
                    ["LastSubmitAt"] = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }
            }),
            "syncDefaults" => Task.FromResult(new PluginUiActionResult
            {
                IsSuccess = true,
                Message = "已同步預設值。",
                StatePatch = new Dictionary<string, object?>
                {
                    ["Channel"] = "email"
                }
            }),
            _ => Task.FromResult(new PluginUiActionResult
            {
                IsSuccess = true,
                Message = $"Plugin '{PluginId}' executed '{command}'."
            })
        };
    }

    private static PluginUiActionResult ValidateRequired(PluginUiActionContext context)
    {
        if (!context.Payload.TryGetValue("StateKey", out var key) || string.IsNullOrWhiteSpace(key))
        {
            return new PluginUiActionResult
            {
                IsSuccess = false,
                Message = "validateRequired 缺少 StateKey。"
            };
        }

        var value = ReadStateAsString(context.State, key);
        var display = context.Payload.TryGetValue("DisplayName", out var displayName)
            ? displayName
            : key;

        if (string.IsNullOrWhiteSpace(value))
        {
            return new PluginUiActionResult
            {
                IsSuccess = false,
                Message = $"{display} 為必填欄位。"
            };
        }

        return new PluginUiActionResult
        {
            IsSuccess = true,
            Message = $"{display} 驗證通過。"
        };
    }

    private static PluginUiActionResult ValidateEmail(PluginUiActionContext context)
    {
        if (!context.Payload.TryGetValue("StateKey", out var key) || string.IsNullOrWhiteSpace(key))
        {
            return new PluginUiActionResult
            {
                IsSuccess = false,
                Message = "validateEmail 缺少 StateKey。"
            };
        }

        var value = ReadStateAsString(context.State, key);

        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
        {
            return new PluginUiActionResult
            {
                IsSuccess = false,
                Message = "Email 格式不正確。"
            };
        }

        return new PluginUiActionResult
        {
            IsSuccess = true,
            Message = "Email 驗證通過。"
        };
    }

    private static PluginUiActionResult CreatePreviewResult(PluginUiActionContext context)
    {
        var name = ReadStateAsString(context.State, "CustomerName");
        var email = ReadStateAsString(context.State, "ContactEmail");
        var channel = ReadStateAsString(context.State, "Channel");
        var notification = ReadStateAsString(context.State, "EnableNotification");

        var preview = $"預覽: Name={name}, Email={email}, Channel={channel}, Notify={notification}";

        return new PluginUiActionResult
        {
            IsSuccess = true,
            Message = preview,
            StatePatch = new Dictionary<string, object?>
            {
                ["LastPreview"] = preview
            }
        };
    }

    private static string ReadStateAsString(IReadOnlyDictionary<string, object?> state, string key)
    {
        if (!state.TryGetValue(key, out var value) || value is null)
        {
            return string.Empty;
        }

        if (value is JsonElement json)
        {
            return json.ValueKind switch
            {
                JsonValueKind.String => json.GetString() ?? string.Empty,
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                JsonValueKind.Null => string.Empty,
                _ => json.ToString() ?? string.Empty
            };
        }

        return Convert.ToString(value) ?? string.Empty;
    }
}
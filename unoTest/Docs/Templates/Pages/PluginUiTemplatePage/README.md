# PluginUiTemplatePage 文件模板 README

## 目的
提供一份「文件驅動 UI + Plugin 事件/動作」模板，讓開發者先定義文件，再由程式動態生成畫面，並透過 plugin 處理事件。

## 對應檔案
- `Models/PluginUiTemplateDocument.cs`
- `Services/IPluginUiActionHandler.cs`
- `Services/PluginUiActionDispatcher.cs`
- `Docs/Templates/Pages/PluginUiTemplatePage/plugin-ui.template.json`

## 核心資料流
1. 載入 JSON 並反序列化為 `PluginUiTemplateDocument`
2. Renderer 依 `ControlType` 建立 Uno 控制項（TextBox/ComboBox/Button...）
3. 以 `Binding.StateKey` 將控制項與狀態字典連接
4. 控制項觸發事件（click/changed/toggled）後，依 `ActionId` 找到對應 `Action`
5. `PluginUiActionDispatcher` 依 `Action.Plugin` 找到對應 `IPluginUiActionHandler`
6. plugin 回傳 `StatePatch`，Renderer 套用後更新 UI

## 文件結構（重點欄位）
- `TemplateId`: 模板唯一識別（建議與功能模組一致）
- `Sections`: 畫面區塊，每個區塊含多個 `Elements`
- `Elements[].ControlType`: 決定要生成的 Uno 控制項類型
- `Elements[].Binding.StateKey`: 綁定狀態鍵值
- `Elements[].Events[]`: UI 事件到 Action 的關聯
- `Actions[]`: plugin 命令定義（`Plugin` + `Command` + `Payload`）
- `PageActions[]`: 頁面層級動作（例如載入時同步預設值）

## ControlType 映射建議
| ControlType | Uno 控制項 | 常見事件 |
| --- | --- | --- |
| TextBox | TextBox | changed |
| TextArea | TextBox (`AcceptsReturn=true`) | changed |
| NumberBox | NumberBox | changed |
| ToggleSwitch | ToggleSwitch | toggled |
| ComboBox | ComboBox | changed |
| Button | Button | click |

## Plugin 契約（最小範例）
```csharp
public sealed class CustomerPluginHandler : IPluginUiActionHandler
{
    public string PluginId => "crm.customer";

    public Task<PluginUiActionResult> ExecuteAsync(
        PluginUiActionContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.Payload.TryGetValue("Operation", out var operation))
        {
            return Task.FromResult(new PluginUiActionResult
            {
                IsSuccess = false,
                Message = "Operation payload is required."
            });
        }

        return Task.FromResult(new PluginUiActionResult
        {
            IsSuccess = true,
            Message = $"Executed: {operation}",
            StatePatch = new Dictionary<string, object?>
            {
                ["LastOperation"] = operation,
                ["LastHandledAt"] = DateTimeOffset.UtcNow
            }
        });
    }
}
```

## App.xaml.cs 註冊範例
```csharp
services.AddSingleton<IPluginUiActionDispatcher, PluginUiActionDispatcher>();
services.AddTransient<IPluginUiActionHandler, CustomerPluginHandler>();
```

## 快速上手
1. 複製 `plugin-ui.template.json`，改成你的業務欄位
2. 實作至少一個 `IPluginUiActionHandler`（一個 plugin 對應一個 `PluginId`）
3. 在 `App.xaml.cs` 註冊 dispatcher 與 plugins
4. 在頁面載入模板文件並建立控制項
5. 事件觸發時呼叫 dispatcher，套用 `StatePatch`

## 給後端工程師的修改建議
- 把 `Actions[].Command` 當成 API use-case（例如 `createCustomer`, `syncDefaults`）
- `Payload` 建議只放必要欄位，避免 UI 文件與 domain model 強耦合
- `TemplateId` 可視為租戶/專案級別設定鍵，方便多版本並存
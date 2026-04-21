# Shell Template

## 1. Template 目的
Shell 是整個應用的容器模板，提供啟動畫面與內容承載入口。

## 2. 檔案組成
- `Presentation/Shell.xaml`
- `Presentation/Shell.xaml.cs`
- `Presentation/ShellViewModel.cs`
- `App.xaml.cs`（`NavigateAsync<Shell>()`）

## 3. 功能清單
- ExtendedSplashScreen 顯示啟動狀態
- 實作 `IContentControlProvider` 供導航系統使用
- 作為路由根節點的承載 UI

## 4. 使用方式
1. 在 `App.OnLaunched` 最後導航到 `Shell`。
2. 在 `RegisterRoutes` 設定 Shell 為 root route。

## 5. 擴充建議
- 在 Shell 統一注入全域服務（session、通知、使用者狀態）。
- 加入全域錯誤顯示層與 loading overlay。

## 6. 注意事項
- ShellViewModel 目前極簡，屬於待擴充骨架。
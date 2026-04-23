# Shell View README

## 目的
`Shell` 是整個 App 的根視圖：上方放全域 `CustomTitleBar`，下方放導航內容區。

## 對應檔案
- `Presentation/Shell.xaml`
- `Presentation/Shell.xaml.cs`
- `Presentation/ShellViewModel.cs`
- `App.xaml.cs`（`NavigateAsync<Shell>()` + RouteMap root）

## 路由與導航
- App 啟動後先進 `Shell`
- 所有子頁都掛在 `Shell` 之下（`DemoIndex` 為預設）

## 畫面結構
- Row 0：`WindowDragStrip` + `CustomTitleBar`
- Row 1：`ExtendedSplashScreen`（Uno Navigation 內容注入點）

## 資料流
- `ShellViewModel` 建立 `TitleBarViewModel`
- `TitleBarStateService`（Singleton）在各頁與 TitleBar 之間同步模式/Tab 狀態

## 主要依賴
- `INavigator`
- `TitleBarStateService`
- `IThemeService`（可選）

## 給 Golang 後端工程師的修改建議
- 把 Shell 當成「API Gateway UI 層」：只放全域框架，不放業務邏輯。
- 業務資料請進 `ViewModel + Service`，不要寫在 Shell code-behind。
- 如果要加全站通知、全站錯誤攔截，優先改 Shell（不是每個頁面重複加）。

## 快速上手
1. 新頁面先註冊 `ViewMap + RouteMap`
2. 從 `DemoIndex` 或 TitleBar Tab 導航到該 route
3. 需要影響標題列時，注入 `TitleBarStateService` 呼叫 `SetTabsMode / SetDetailMode`

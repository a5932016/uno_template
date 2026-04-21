# SettingsPage Template

## 1. Template 目的
SettingsPage 提供應用設定模板，集中管理主題、語言、通知與資料維護操作。

## 2. 檔案組成
- `Presentation/SettingsPage.xaml`
- `Presentation/SettingsPage.xaml.cs`
- `ViewModels/SettingsViewModel.cs`

## 3. 功能清單
- 主題切換（System/Light/Dark）
- 字體大小選項
- 語言選擇
- 通知與音效開關
- 清快取、匯出、刪帳號、檢查更新命令占位

## 4. 使用方式
1. 設定頁 DataContext 綁定 `SettingsViewModel`。
2. 主題索引改變時呼叫 `IThemeService.SetThemeAsync()`。
3. 將命令接到實際服務（快取、匯出、帳號 API）。

## 5. 擴充建議
- 設定值持久化（local settings / sqlite / api profile）。
- 將命令操作結果回饋到 UI（成功、失敗、重試）。
- 增加隱私、授權、版本資訊分區。

## 6. 注意事項
- 目前多數命令為 TODO，占位用途。
# SettingsPage View README

## 目的
設定頁集中管理 Theme/Language/Notification 等偏「系統偏好」資料。

## 對應檔案
- `Presentation/SettingsPage.xaml`
- `ViewModels/SettingsViewModel.cs`

## TitleBar 行為
- 進頁時 `SettingsViewModel` 會呼叫 `TitleBarStateService.SetTabsMode(2)`，同步設定 Tab 高亮。

## 給 Golang 後端工程師的修改建議
- 建議把使用者偏好（語言、通知）儲存在後端 profile API。
- ViewModel 只負責觸發命令，不直接做 IO 細節。

## 快速上手
1. 實作設定儲存 service
2. 在 `LoadSettings()` 載入真實資料
3. 各命令改接實際 API

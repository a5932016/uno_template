# DialogDemoPage View README

## 目的
展示各種對話框與通知模式，供其他頁複用。

## 對應檔案
- `Presentation/DialogDemoPage.xaml`
- `Presentation/DialogDemoPage.xaml.cs`

## 主要模式
- `ContentDialog`（message/confirm/input）
- `InfoBar` 通知
- `Flyout`, `DatePickerFlyout`, `TimePickerFlyout`
- Error dialog（含 exception 細節）

## 給 Golang 後端工程師的修改建議
- 將此頁視為「交互元件菜單」，不是業務頁。
- 正式專案建議抽 `IDialogService`，避免每頁重複建立 dialog。
- 錯誤訊息來源請統一經過後端錯誤碼映射策略。

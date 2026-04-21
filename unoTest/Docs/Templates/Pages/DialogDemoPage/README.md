# DialogDemoPage Template

## 1. Template 目的
DialogDemoPage 是彈窗交互模板，集中示範常見對話框、通知列與進度提示設計。

## 2. 檔案組成
- `Presentation/DialogDemoPage.xaml`
- `Presentation/DialogDemoPage.xaml.cs`

## 3. 功能清單
- ContentDialog：訊息、確認、輸入、選擇、自訂表單
- 錯誤對話框：錯誤碼與例外細節
- Loading/Progress 模式
- Flyout / DatePickerFlyout / TimePickerFlyout
- InfoBar 通知

## 4. 使用方式
1. 由事件觸發建立 `ContentDialog`。
2. 設置 `XamlRoot = this.XamlRoot`。
3. 依 `ContentDialogResult` 決定後續行為。

## 5. 擴充建議
- 抽成 `IDialogService`，統一全站對話框策略。
- 建立標準錯誤模板（錯誤碼、追蹤 ID、重試）。
- 將通知與對話框接入 logging/telemetry。

## 6. 注意事項
- Page code-behind 事件多，若業務增長建議服務化。
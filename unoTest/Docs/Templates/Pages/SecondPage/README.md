# SecondPage Template

## 1. Template 目的
SecondPage 是最簡潔的「導航結果頁」模板，示範接收上一頁傳入的資料模型。

## 2. 檔案組成
- `Presentation/SecondPage.xaml`
- `Presentation/SecondPage.xaml.cs`
- `Presentation/SecondViewModel.cs`

## 3. 功能清單
- 接收 `Entity` 導航資料
- 顯示傳入欄位（Name）
- 作為 DataViewMap 的最小範例

## 4. 使用方式
1. 由前頁呼叫 `NavigateViewModelAsync<SecondViewModel>(..., data)`。
2. 透過 `DataViewMap<SecondPage, SecondViewModel, Entity>` 完成繫結。

## 5. 擴充建議
- 將 `record` 擴展為完整詳情頁 ViewModel。
- 增加返回、編輯與狀態同步命令。

## 6. 注意事項
- 僅示範資料傳遞，不包含業務流程。
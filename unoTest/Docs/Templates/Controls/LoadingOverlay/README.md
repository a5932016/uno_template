# LoadingOverlay Template

## 1. Template 目的
LoadingOverlay 提供全頁或區塊級遮罩模板，統一 loading 體驗。

## 2. 檔案組成
- `Controls/LoadingOverlay.xaml`
- `Controls/LoadingOverlay.xaml.cs`

## 3. 功能清單
- `IsLoading` 顯示/隱藏
- 自訂提示文字
- 可選阻擋互動

## 4. 使用方式
1. 將 `IsLoading` 綁定到 ViewModel 狀態。
2. 在長任務開始前設為 true，結束後設回 false。

## 5. 擴充建議
- 增加進度百分比與取消命令。
- 提供不同 loading 樣式（spinner/skeleton/bar）。

## 6. 注意事項
- 長時間 loading 建議搭配可取消操作與超時提示。
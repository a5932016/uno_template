# LoadingOverlay Control README

## 目的
統一 loading UI（全頁或區塊遮罩）。

## 對應檔案
- `Controls/LoadingOverlay.xaml`
- `Controls/LoadingOverlay.xaml.cs`

## 使用建議
- 與 ViewModel 的 `IsLoading` 綁定
- 長時間任務要有 timeout 與取消

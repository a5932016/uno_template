# DataGridControl Control README

## 目的
表格顯示控制項，封裝欄位呈現與選取互動。

## 對應檔案
- `Controls/DataGridControl.xaml`
- `Controls/DataGridControl.xaml.cs`

## 給 Golang 後端工程師的修改建議
- 大資料量請走伺服器分頁，不要一次把全部資料塞進 `ItemsSource`。

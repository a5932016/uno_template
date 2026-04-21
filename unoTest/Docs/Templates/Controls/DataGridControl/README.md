# DataGridControl Template

## 1. Template 目的
DataGridControl 是資料表格模板，負責清單顯示、選取、排序與欄位模板化。

## 2. 檔案組成
- `Controls/DataGridControl.xaml`
- `Controls/DataGridControl.xaml.cs`

## 3. 功能清單
- `ItemsSource` 綁定
- 多欄位顯示與欄位格式
- 單選/多選行為
- 空資料提示

## 4. 使用方式
1. 綁定 `ItemsSource` 到 ViewModel 集合。
2. 設置欄位模板與格式。
3. 監聽選取變更並同步命令可用狀態。

## 5. 擴充建議
- 加入虛擬化與分頁整合。
- 追加欄位排序/篩選 UI。

## 6. 注意事項
- 與 Uno 平台控制項相容性需實測，特別是全選與複選功能。
# CrudDemoPage Template

## 1. Template 目的
CrudDemoPage 是完整資料管理模板，示範查詢、分頁、編輯、刪除、匯出等常見管理後台場景。

## 2. 檔案組成
- `Presentation/CrudDemoPage.xaml`
- `Presentation/CrudDemoPage.xaml.cs`
- `ViewModels/CrudDemoViewModel.cs`
- `Services/ProductService.cs`
- `Models/Product.cs`

## 3. 功能清單
- 關鍵字搜尋與分類篩選
- 單筆/批次刪除
- 分頁（第一頁/上一頁/下一頁/最後頁）
- 新增與編輯對話框
- CSV 匯出預覽
- 載入中遮罩與空狀態提示

## 4. 使用方式
1. 在 DI 註冊 `IProductService`。
2. 頁面載入時設置 `XamlRoot` 讓 ContentDialog 可顯示。
3. ViewModel 透過 `GetPagedAsync` 驅動畫面資料。

## 5. 擴充建議
- 將 `ContentDialog` 流程抽成 `ICrudDialogService`（讓 ViewModel 純化）。
- 將 `MockProductService` 換成 SQLite 或 API Repository。
- 補齊欄位排序、伺服器端分頁與錯誤重試。

## 6. 注意事項
- `ListView.SelectAll()` 在 Uno 部分平台未完整支援，跨平台需替代方案。
- 目前 ViewModel 仍含部分 UI 邏輯，建議逐步拆分。
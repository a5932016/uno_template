# CrudDemoPage View README

## 目的
後台管理頁模板：搜尋、分頁、新增、編輯、刪除、匯出。

## 對應檔案
- `Presentation/CrudDemoPage.xaml`
- `ViewModels/CrudDemoViewModel.cs`
- `Services/IProductService` + 實作

## 核心資料流
- ViewModel 呼叫 `IProductService.GetPagedAsync`
- 結果進 `ObservableCollection<ProductViewItem>`
- UI 綁定清單、分頁、選取狀態

## 給 Golang 後端工程師的修改建議
- 這頁最適合直接對接你的 REST API。
- 推薦 server-side 分頁（page/pageSize/filter/sort）而不是把全部資料拉到前端。
- 對話框邏輯可抽到 service，讓 ViewModel 更像 handler。

## 快速上手
1. 實作 `IProductService` 串你的 API
2. 接上錯誤處理（429/5xx）與重試
3. 補排序欄位與查詢條件 DTO

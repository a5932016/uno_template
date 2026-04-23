# ProductListPage View README

## 目的
示範「動態 UI 生成 + 點擊項目傳遞資料到下一頁」。

## 對應檔案
- `Presentation/ProductListPage.xaml`
- `Presentation/ProductListViewModel.cs`
- `Models/ProductNavData.cs`

## 核心資料流
1. ViewModel 維護 `ObservableCollection<ProductListItem>`
2. XAML 用 `ItemsRepeater` 逐項生成 UI
3. 點「查看詳情」後，將 `ProductListItem` 包成 `ProductNavData`
4. `NavigateViewModelAsync<ProductDetailViewModel>(data: navData)`

## TitleBar 行為
- 進頁時 `SetTabsMode(1)`，讓「產品」Tab 高亮

## 給 Golang 後端工程師的修改建議
- 這頁可對應你的 `GET /products` 清單 API。
- `ProductNavData` 就是前往詳情頁的輕量 payload（建議只放必要欄位）。
- 若資料量大，改 server-side paging + filter。

## 快速上手
1. 用真實 Product API 取代 `GenerateSampleProducts()`
2. 新增搜尋/篩選參數到 ViewModel
3. 保持 `ProductNavData` 與詳情頁需求一致

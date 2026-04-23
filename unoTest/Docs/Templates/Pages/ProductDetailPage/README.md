# ProductDetailPage View README

## 目的
接收上一頁傳來的產品資料，顯示詳情，並把 TitleBar 切成 Detail 模式。

## 對應檔案
- `Presentation/ProductDetailPage.xaml`
- `Presentation/ProductDetailViewModel.cs`
- Route 註冊：`DataViewMap<ProductDetailPage, ProductDetailViewModel, ProductNavData>`

## 核心資料流
1. 建構子參數 `ProductNavData navData` 由 Uno Navigation 注入
2. ViewModel 將欄位映射到可綁定屬性
3. 呼叫 `TitleBarStateService.SetDetailMode("產品詳情：...")`
4. TitleBar 顯示返回按鈕 + 標題

## 給 Golang 後端工程師的修改建議
- 若清單只傳部分欄位，詳情頁可再用 `Id` 打後端查詢完整資料。
- 請把 `ProductNavData` 當成「跨頁 DTO」，避免直接傳 domain 大物件。

## 快速上手
1. 在建構子加進一步查詢 service（可選）
2. 加入 loading/error 狀態
3. 保持返回時能恢復 tabs mode

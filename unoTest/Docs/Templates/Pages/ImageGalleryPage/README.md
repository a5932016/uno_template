# ImageGalleryPage View README

## 目的
圖片瀏覽與管理示範頁（含右鍵選單、上傳、預覽）。

## 對應檔案
- `Presentation/ImageGalleryPage.xaml`
- `Presentation/ImageGalleryPage.xaml.cs`（同檔含 `ImageGalleryViewModel`）

## 目前狀態
- 已可展示清單與切換選取
- 上傳僅 Windows 路徑較完整
- 下載/分享/編輯屬於占位

## 給 Golang 後端工程師的修改建議
- 把圖片 metadata 當資料表，binary 走物件儲存。
- 建議 API 拆成：`list`, `upload`, `delete`, `signed-url`。
- 前端只拿縮圖 URL + metadata，詳細圖用 signed URL。

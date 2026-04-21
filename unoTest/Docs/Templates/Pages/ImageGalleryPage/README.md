# ImageGalleryPage Template

## 1. Template 目的
ImageGalleryPage 提供圖片管理模板，包含瀏覽模式切換、選取預覽、上傳與快捷操作。

## 2. 檔案組成
- `Presentation/ImageGalleryPage.xaml`
- `Presentation/ImageGalleryPage.xaml.cs`

## 3. 功能清單
- Grid/List/Details 三種檢視模式
- 圖片選取與全螢幕檢視
- 上傳入口（Windows 走 FileOpenPicker）
- 右鍵選單：開啟、下載、複製、刪除、屬性
- 前一張/下一張導覽

## 4. 使用方式
1. 將 `ImageGalleryViewModel` 綁定至頁面 DataContext。
2. 呼叫 `PickAndUploadImagesAsync()` 匯入檔案。
3. 依需求替換 `LoadSampleImages()` 為真實來源。

## 5. 擴充建議
- 接入雲端儲存（S3/Azure Blob）與縮圖服務。
- 補齊編輯、下載、分享實作。
- 增加 lazy loading 與快取策略。

## 6. 注意事項
- 目前部分功能是占位（下載/分享/編輯）。
- `FileOpenPicker` 為平台相關 API，跨平台需分別實作。
# DemoIndexPage Template

## 1. Template 目的
DemoIndexPage 是模板入口頁，透過卡片列表快速導向各示範模板。

## 2. 檔案組成
- `Presentation/DemoIndexPage.xaml`
- `Presentation/DemoIndexPage.xaml.cs`

## 3. 功能清單
- Demo 卡片列表
- 路由導航命令
- NEW 標記
- 圖示與顏色分組

## 4. 使用方式
1. 在 `DemoItems` 新增一個 `DemoItem`。
2. 指定 route 與描述。
3. 確保 `App.xaml.cs` 已註冊對應 ViewMap 與 Route。

## 5. 擴充建議
- 將 `DemoItems` 改為獨立 ViewModel 檔案。
- 用設定檔驅動模板列表，降低硬編碼。

## 6. 注意事項
- 目前 `DemoIndexViewModel` 與 page code-behind 放在同檔，專案變大後建議拆分。
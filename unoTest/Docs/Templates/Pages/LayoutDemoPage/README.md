# LayoutDemoPage Template

## 1. Template 目的
LayoutDemoPage 用於展示 Uno XAML 常見布局手法，適合當新頁面切版的起始模板。

## 2. 檔案組成
- `Presentation/LayoutDemoPage.xaml`
- `Presentation/LayoutDemoPage.xaml.cs`

## 3. 功能清單
- Grid 行列布局
- StackPanel 排版
- 卡片式內容分區
- 響應式排列示範
- SafeArea 與 NavigationBar 基礎結構

## 4. 使用方式
1. 複製頁面骨架（NavigationBar + 內容區）。
2. 依需求替換示範區塊。
3. 將資料來源改為你的 ViewModel。

## 5. 擴充建議
- 將重複 UI 區塊抽成 UserControl。
- 將固定字串加上 `x:Uid` 以支援多語系。
- 以 `ItemsRepeater` 取代硬編排版，降低維護成本。

## 6. 注意事項
- 此頁面重點是布局，不含完整業務流程。
- 若你要上線使用，建議補齊無障礙標籤與自動化屬性。
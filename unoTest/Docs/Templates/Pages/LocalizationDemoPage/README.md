# LocalizationDemoPage Template

## 1. Template 目的
LocalizationDemoPage 展示多語系模板，包含語言切換與格式化輸出（日期、數字、貨幣、百分比）。

## 2. 檔案組成
- `Presentation/LocalizationDemoPage.xaml`
- `Presentation/LocalizationDemoPage.xaml.cs`
- `Strings/*/Resources.resw`
- `appsettings.json`

## 3. 功能清單
- 語言切換（PrimaryLanguageOverride）
- 動態字串更新（IStringLocalizer）
- 日期/數字/貨幣/百分比格式化
- 複數文字示範
- 語言工具類別 `LocalizationHelper`

## 4. 使用方式
1. 在 `Strings/{culture}/Resources.resw` 定義資源。
2. 在 UI 綁定 `x:Uid` 與 localizer 字串。
3. 切換語言後更新動態內容。

## 5. 擴充建議
- 將語系選擇持久化到本地設定。
- 導入 ICU/Plural 規則庫加強複數語法。
- 建立 i18n 測試清單（RTL、長字串、日期格式）。

## 6. 注意事項
- 部分 UI 字串可能需重新建立頁面才會完全套用。
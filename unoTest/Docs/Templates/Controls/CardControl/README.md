# CardControl Template

## 1. Template 目的
CardControl 是可重用資訊卡片模板，統一卡片標題、副標題、內容與操作區布局。

## 2. 檔案組成
- `Controls/CardControl.xaml`
- `Controls/CardControl.xaml.cs`

## 3. 功能清單
- `Title` / `Subtitle` 文字屬性
- `Icon` 顯示
- `CardContent` 內容插槽
- `PrimaryActionContent` / `SecondaryActionContent` 操作插槽

## 4. 使用方式
1. 在 XAML 宣告 `CardControl`。
2. 設定標題相關 DependencyProperty。
3. 把自訂內容放進 `CardContent`。

## 5. 擴充建議
- 增加 `IsLoading` 狀態與 skeleton。
- 增加 `StatusBadge` 區塊。

## 6. 注意事項
- 建議將色彩與字體統一交由 `App.xaml` Style 控制。
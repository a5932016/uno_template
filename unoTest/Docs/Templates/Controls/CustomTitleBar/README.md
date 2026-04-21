# CustomTitleBar Template

## 1. Template 目的
CustomTitleBar 提供桌面視窗標題列模板，整合返回、搜尋與視窗拖曳區域。

## 2. 檔案組成
- `Controls/CustomTitleBar.xaml`
- `Controls/CustomTitleBar.xaml.cs`
- `Styles/TitleBarStyles.xaml`

## 3. 功能清單
- 視窗標題與副標題顯示
- Back 按鈕狀態與事件
- 可選搜尋框
- 額外命令區插槽

## 4. 使用方式
1. 在桌面頁面頂部放入 `CustomTitleBar`。
2. 綁定 `CanGoBack` 與 Back 事件。
3. 透過 `Window` API 設定 draggable region（若有需要）。

## 5. 擴充建議
- 接入全域搜尋。
- 新增通知鈴鐺、使用者選單。

## 6. 注意事項
- 部分標題列行為僅在 Desktop 平台有效。
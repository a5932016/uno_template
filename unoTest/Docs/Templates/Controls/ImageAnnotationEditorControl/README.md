# ImageAnnotationEditorControl View README

## 目的
提供可重用的圖片標註控制項，供任何頁面嵌入使用。

## 對應檔案
- `Controls/ImageAnnotationEditorControl.xaml`
- `Controls/ImageAnnotationEditorControl.xaml.cs`
- `ViewModels/ImageAnnotationEditorViewModel.cs`

## 主要能力
- 載入圖片與另存 PNG
- 工具切換：移動 / 方框 / 多邊形
- 顏色切換：多組描邊色
- 標註編輯：拖曳移動、控制點調整
- 縮放：Zoom In / Zoom Out

## 重用方式
- 在任意 Page XAML 引入 `xmlns:controls="using:unoTest.Controls"`
- 只顯示核心畫布可用：`<controls:ImageAnnotationEditorControl ShowToolbar="False" />`
- 容器頁可透過 control API 快速操作，不必在 XAML 放內建工具列

## 可重用 API（Control）
- `SetTool(AnnotationTool tool)` / `TrySetTool(string rawTool)`：切換模式（移動/方框/多邊形）
- `SetZoom(float)`、`ZoomIn()`、`ZoomOut()`：縮放
- `TrySetColor(string colorKey)`、`SetStrokeColor(Color color)`：切換標註顏色
- `UndoLastAction()`：取消目前草稿或上一筆標註
- `GetCurrentSelection()`：取得目前選取框資訊（種類、邊界、點位）
- `LoadImageAsync(StorageFile)`、`LoadImageAsync(IRandomAccessStream, string?)`：由外部載入圖片

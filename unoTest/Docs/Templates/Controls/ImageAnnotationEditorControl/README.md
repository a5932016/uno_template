# ImageAnnotationEditorControl View README

## 目的
提供可重用的圖片標註控制項，供任何頁面嵌入使用。

## 對應檔案
- `uIP/UI/Uno/Views/UserControls/ImageAnnotationEditorControl.xaml`
- `uIP/UI/Uno/Views/UserControls/ImageAnnotationEditorControl.xaml.cs`
- `uIP/UI/Uno/ViewModels/ImageAnnotationEditorViewModel.cs`

## 主要能力
- 載入圖片與另存 PNG
- 工具切換：移動 / 方框 / 多邊形
- 顏色切換：多組描邊色
- 標註編輯：拖曳移動、控制點調整
- 縮放：Zoom In / Zoom Out
- VM 可持久化：標註區塊清單（Blocks）可序列化存取
- 區塊生命週期紀錄：建立 / 更新 / 刪除 / 選取 / 載入

## 重用方式
- 在任意 Page XAML 引入 `xmlns:controls="using:uIP.UI.Uno.Views.UserControls"`
- 只顯示核心畫布可用：`<controls:ImageAnnotationEditorControl ShowToolbar="False" />`
- 容器頁可透過 control API 快速操作，不必在 XAML 放內建工具列
- 若要由外層 VM 控制狀態，請綁定 `ViewModel`（DependencyProperty）：
  - `<controls:ImageAnnotationEditorControl ViewModel="{Binding AnnotationEditor}" />`
  - 外層修改 `AnnotationEditor.ActiveTool / ActiveColor / ZoomFactor` 會同步套用到 control

## 可重用 API（Control）
- `SetTool(AnnotationTool tool)` / `TrySetTool(string rawTool)`：切換模式（移動/方框/多邊形）
- `SetZoom(float)`、`ZoomIn()`、`ZoomOut()`：縮放
- `TrySetColor(string colorKey)`、`SetStrokeColor(Color color)`：切換標註顏色
- `UndoLastAction()`：取消目前草稿或上一筆標註
- `GetCurrentSelection()`：取得目前選取框資訊（種類、邊界、點位）
- `ExportAnnotations()`：匯出目前全部標註為 `AnnotationBlock` DTO
- `ImportAnnotations(IEnumerable<AnnotationBlock>)`：由 DTO 匯入並重建畫布標註
- `LoadImageAsync(StorageFile)`、`LoadImageAsync(IRandomAccessStream, string?)`：由外部載入圖片

## ViewModel 持久化 API（`ImageAnnotationEditorViewModel`）
- `CreateDocument()`：建立 `AnnotationDocument`
- `SaveDocumentAsJson(string? targetPath)`：序列化成 JSON，必要時直接寫檔
- `TryLoadDocumentFromJson(string, out string?)`：由 JSON 載入文件
- `TryReadDocumentFromPath(string, out string?)`：由檔案讀入並載入文件
- `ReplaceBlocks(...) / UpsertBlock(...) / RemoveBlock(...) / ClearBlocks(...)`：區塊資料操作
- `BlockEventRecords`：區塊事件歷程紀錄集合

# NodeLinkCanvas Template

## 1. Template 目的
NodeLinkCanvas 是流程圖視覺化控制項模板，專注畫布渲染與互動轉發，不負責業務規則。

## 2. 檔案組成
- `Controls/NodeLinkCanvas.xaml`
- `Controls/NodeLinkCanvas.xaml.cs`
- `ViewModels/NodeLinkCanvasViewModel.cs`

## 3. 功能清單
- 節點與連線渲染
- 節點拖曳/選取
- 工具列命令綁定（新增、連線、刪除、自動排版、儲存）
- 網格顯示切換

## 4. 使用方式
1. 將 `Canvas`（`NodeLinkCanvasViewModel`）綁到控制項 `ViewModel`。
2. 由頁面 ViewModel 負責初始化與保存。
3. 控制項只保留渲染與事件轉發。

## 5. 擴充建議
- 節點選框、多選、對齊線。
- 連線路徑優化（貝茲曲線、避障）。

## 6. 注意事項
- 若把業務邏輯寫回 control code-behind，會破壞可測試性與重用性。
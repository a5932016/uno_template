# NodeLinkCanvas Control README

## 目的
流程圖畫布控制項（節點渲染、連線渲染、拖曳互動）。

## 對應檔案
- `Controls/NodeLinkCanvas.xaml`
- `Controls/NodeLinkCanvas.xaml.cs`
- `ViewModels/NodeLinkCanvasViewModel.cs`

## 設計原則
- Control 層只做畫面與互動事件
- 業務流程（初始化、儲存、模板）放在頁面 ViewModel

## 給 Golang 後端工程師的修改建議
- 以 JSON document（nodes/links）為 API 輸入輸出格式最穩定
- schema 變更請做版本欄位

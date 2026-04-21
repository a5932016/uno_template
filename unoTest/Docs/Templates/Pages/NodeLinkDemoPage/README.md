# NodeLinkDemoPage Template

## 1. Template 目的
NodeLinkDemoPage 是高可維護模板範例，展示 View / ViewModel / Repository 分層，並以 SQLite 完成節點流程圖持久化。

## 2. 檔案組成
- `Presentation/NodeLinkDemoPage.xaml`
- `Presentation/NodeLinkDemoPage.xaml.cs`
- `Presentation/NodeLinkDemoViewModel.cs`
- `Presentation/NodeLinkEditorViewModelBase.cs`
- `ViewModels/NodeLinkCanvasViewModel.cs`
- `Controls/NodeLinkCanvas.xaml`
- `Controls/NodeLinkCanvas.xaml.cs`
- `Services/INodeGraphRepository.cs`
- `Services/SqliteNodeGraphRepository.cs`
- `Services/InMemoryNodeGraphRepository.cs`
- `Models/NodeGraphDocument.cs`

## 3. 功能清單
- 節點新增、拖曳、連線、刪除、自動排列
- 初始化模板、重置模板、儲存圖形
- SQLite 持久化（Desktop/Mobile）
- WASM 自動切換 InMemory Repository
- 控制項重用（同一畫布可在多頁使用）

## 4. 使用方式
1. 在 `App.xaml.cs` 註冊 `INodeGraphRepository`。
2. 在頁面綁定 `Canvas`（`NodeLinkCanvasViewModel`）。
3. 頁面 `Loaded` 執行 `InitializeCommand`。
4. 使用 `SaveCommand` 將當前狀態存入 repository。

## 5. 擴充建議
- 追加節點類型（條件、API、延遲、人工審核）。
- 為連線加入 Label 與條件式。
- 儲存格式改為版本化文件，支援未來遷移。

## 6. 注意事項
- 控制項已是「View-only」，請勿把業務規則寫回 code-behind。
- WASM 不使用 SQLite，重整頁面資料不保留（除非改接遠端 API）。
# NodeLinkDemoPage View README

## 目的
流程圖編輯頁，示範 `View + ViewModel + Repository` 的完整分層。

## 對應檔案
- `Presentation/NodeLinkDemoPage.xaml`
- `Presentation/NodeLinkDemoViewModel.cs`
- `Presentation/NodeLinkEditorViewModelBase.cs`
- `Controls/NodeLinkCanvas.*`
- `Services/INodeGraphRepository` 實作

## 核心行為
- `InitializeCommand`：載入或建立預設圖
- `SaveCommand`：保存圖資料
- `ResetTemplateCommand`：重置模板

## 給 Golang 後端工程師的修改建議
- Repository 介面就像你的 data access layer。
- 先固定 document schema（Node/Link），再擴充欄位，避免前後端 schema 漂移。
- 若改用後端儲存，直接新增 `ApiNodeGraphRepository` 實作即可。

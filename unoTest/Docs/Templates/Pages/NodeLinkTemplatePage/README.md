# NodeLinkTemplatePage Template

## 1. Template 目的
NodeLinkTemplatePage 用來展示同一個 NodeLinkCanvas 控件在第二個頁面的重用模式，適合作為流程模板編輯器基底。

## 2. 檔案組成
- `Presentation/NodeLinkTemplatePage.xaml`
- `Presentation/NodeLinkTemplatePage.xaml.cs`
- `Presentation/NodeLinkTemplateViewModel.cs`
- `Presentation/NodeLinkEditorViewModelBase.cs`
- `Controls/NodeLinkCanvas.xaml`

## 3. 功能清單
- 與 NodeLinkDemo 同構的初始化/儲存/重置流程
- 獨立 `GraphKey`，可保存不同圖稿
- 共用 NodeLinkCanvas 呈現與互動能力

## 4. 使用方式
1. 保持 `NodeLinkTemplateViewModel` 繼承 `NodeLinkEditorViewModelBase`。
2. 為此頁配置獨立 `GraphKey`。
3. 在 `CreateTemplateDocument()` 放入預設流程模板。

## 5. 擴充建議
- 增加模板分類（需求、開發、測試、部署）。
- 加入模板複製與版本管理。

## 6. 注意事項
- 請避免與 `NodeLinkDemoPage` 共用同一個 `GraphKey`，避免互相覆蓋。
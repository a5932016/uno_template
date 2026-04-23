# NodeLinkTemplatePage View README

## 目的
重用 NodeLink 編輯器，但用不同模板與不同 `GraphKey` 管理資料。

## 對應檔案
- `Presentation/NodeLinkTemplatePage.xaml`
- `Presentation/NodeLinkTemplateViewModel.cs`

## 與 NodeLinkDemo 的差異
- 主要差在 `GraphKey`
- 預設模板內容不同

## 給 Golang 後端工程師的修改建議
- 把 `GraphKey` 想成資料分區 key（tenant/project/template-id）。
- 切記不要與其他頁共用同 key，避免覆寫。

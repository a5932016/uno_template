# LinkedListDemoPage View README

## 目的
示範清單節點之間的可視化連線（偏流程圖簡化版）。

## 對應檔案
- `Presentation/LinkedListDemoPage.xaml`
- `Presentation/LinkedListDemoPage.xaml.cs`
- `Controls/LinkedListControl.*`

## 互動重點
- 新增節點
- 清除連線
- 編輯/刪除節點（Dialog）
- 事件：`LinkCreated`, `LinkRemoved`, `ItemEditRequested`, `ItemDeleteRequested`

## 給 Golang 後端工程師的修改建議
- 若要持久化，請把節點與連線拆兩張表（nodes, edges）。
- UI 事件只發命令，實際存檔交給 service。

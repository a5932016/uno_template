# LinkedListControl Template

## 1. Template 目的
LinkedListControl 是可視化關聯控制項，用於顯示節點與節點之間的連線。

## 2. 檔案組成
- `Controls/LinkedListControl.xaml`
- `Controls/LinkedListControl.xaml.cs`

## 3. 功能清單
- 以 ItemsSource 顯示節點
- 節點連線繪製
- 新增/移除連線 API
- 連線事件通知

## 4. 使用方式
1. 設置 `ItemsSource`。
2. 呼叫 `AddLink` / `RemoveLink`。
3. 根據事件更新 ViewModel 狀態。

## 5. 擴充建議
- 支援箭頭樣式與連線文字。
- 增加連線合法性檢查（避免循環）。

## 6. 注意事項
- 自動布局與重繪成本會隨節點數上升，需注意效能。
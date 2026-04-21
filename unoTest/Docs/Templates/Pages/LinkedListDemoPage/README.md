# LinkedListDemoPage Template

## 1. Template 目的
LinkedListDemoPage 展示清單項目間連線互動，適合流程步驟、依賴鏈、關聯圖等操作場景。

## 2. 檔案組成
- `Presentation/LinkedListDemoPage.xaml`
- `Presentation/LinkedListDemoPage.xaml.cs`
- `Controls/LinkedListControl.xaml`
- `Controls/LinkedListControl.xaml.cs`

## 3. 功能清單
- 載入範例節點清單
- 建立/移除連線
- 新增節點
- 連線清空
- 編輯/刪除節點（ContentDialog）

## 4. 使用方式
1. 設置 `LinkedList.ItemsSource`。
2. 用 `AddLink(fromId, toId)` 建立關聯。
3. 監聽 `LinkCreated`、`LinkRemoved` 等事件。

## 5. 擴充建議
- 將節點編輯對話框抽離至服務層。
- 連線增加條件與顏色語意。
- 支援節點拖曳排序與自動重新布局。

## 6. 注意事項
- 連線依賴 VisualTree 與座標轉換，複雜模板下要特別驗證。
- 大資料量時，Canvas 重繪需做節流。
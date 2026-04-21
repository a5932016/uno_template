# TabDemoPage Template

## 1. Template 目的
TabDemoPage 提供多種頁籤切換模式示範，方便快速比較不同導航體驗。

## 2. 檔案組成
- `Presentation/TabDemoPage.xaml`
- `Presentation/TabDemoPage.xaml.cs`

## 3. 功能清單
- TabView：新增/關閉/切換頁籤
- NavigationView：側邊切換
- Segmented 風格切換
- Uno Toolkit TabBar 切換
- 事件回呼示範

## 4. 使用方式
1. 依 UX 選一種主導航元件（TabView 或 NavigationView）。
2. 在對應事件中切換內容或路由。
3. 若是動態頁籤，使用 `TabView_AddTabButtonClick` 建構內容。

## 5. 擴充建議
- 動態頁籤內容改為 DataTemplate + ViewModel。
- 加入未儲存變更提醒（關閉 tab 前確認）。
- 對每個 tab 增加路由同步。

## 6. 注意事項
- 不同平台對複合導航元件支援程度不同，請先跨平台驗證。
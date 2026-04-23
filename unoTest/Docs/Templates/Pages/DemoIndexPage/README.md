# DemoIndexPage View README

## 目的
功能入口頁，透過卡片導向所有示範頁。

## 對應檔案
- `Presentation/DemoIndexPage.xaml`
- `Presentation/DemoIndexPage.xaml.cs`（同檔含 `DemoIndexViewModel`）

## 畫面與互動
- `ItemsRepeater + UniformGridLayout` 顯示功能卡
- 每張卡綁定 `NavigateCommand`，導航到 route

## TitleBar 行為
- 建構子呼叫 `titleBarState.SetTabsMode(0)`，讓首頁 Tab 高亮

## 給 Golang 後端工程師的修改建議
- 這頁可視為「前端路由目錄」，相當於 API 文件首頁。
- 新功能先在 `App.xaml.cs` 註冊 route，再回來加一張 `DemoItem` 卡片。
- 若卡片資料改成後端配置，可抽成 `IDemoCatalogService`。

## 快速上手
1. 在 `DemoItems` 新增一筆（標題、描述、route）
2. 確認 route 已註冊
3. 執行後點卡片驗證可導航

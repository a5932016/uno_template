# EmptyStateView Template

## 1. Template 目的
EmptyStateView 提供一致化「無資料」模板，避免各頁重複實作空狀態 UI。

## 2. 檔案組成
- `Controls/EmptyStateView.xaml`
- `Controls/EmptyStateView.xaml.cs`

## 3. 功能清單
- Icon / 標題 / 說明
- 可選主按鈕文案與命令
- 不同空狀態類型（初始、搜尋無結果、權限不足）

## 4. 使用方式
1. 在集合為空時顯示 `EmptyStateView`。
2. 將按鈕命令綁定到重試或新增操作。

## 5. 擴充建議
- 抽出 `EmptyStateKind` enum，統一文案策略。
- 與 telemetry 整合統計空狀態觸發率。

## 6. 注意事項
- 請使用資源字串，避免硬編碼文案。
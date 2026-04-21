# NavigationMenu Template

## 1. Template 目的
NavigationMenu 提供左側導覽模板，集中管理模組入口與選取狀態。

## 2. 檔案組成
- `Controls/NavigationMenu.xaml`
- `Controls/NavigationMenu.xaml.cs`

## 3. 功能清單
- 菜單項目清單顯示
- 當前項目高亮
- 點擊導航事件
- 可選群組與圖示

## 4. 使用方式
1. 綁定 `MenuItems` 到 ViewModel。
2. 在選取變更事件中呼叫 `INavigator`。

## 5. 擴充建議
- 支援權限裁剪（依角色顯示）。
- 支援收藏與最近使用。

## 6. 注意事項
- 菜單定義建議集中於單一來源，避免頁面各自硬編碼。
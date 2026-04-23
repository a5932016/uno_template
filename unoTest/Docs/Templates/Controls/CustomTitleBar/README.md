# CustomTitleBar Control README

## 目的
提供 OS-level TitleBar 客製化（Windows Desktop），並支援 Tab 切頁、返回、主題與使用者操作。

## 對應檔案
- `Controls/CustomTitleBar.xaml`
- `Controls/CustomTitleBar.xaml.cs`
- `ViewModels/TitleBarViewModel.cs`
- `Services/TitleBarStateService.cs`
- `Styles/TitleBarStyles.xaml`
- `Presentation/Shell.xaml`（`WindowDragStrip`）

## 互動原理（重要）
- 只把 `WindowDragStrip`（8px）註冊給 `Window.SetTitleBar(...)` 作為拖曳區
- `CustomTitleBar` 其餘區域保持 client area，Tab/Button 可正常點擊

## 給 Golang 後端工程師的修改建議
- 把這層當全域 navigation/header，不要塞頁面業務。
- 頁面只透過 `TitleBarStateService` 發狀態，不直接操作 TitleBar 控制項。

## 快速上手
1. 新增 Tab：改 `TitleBarViewModel.Tabs`
2. 切 Detail 模式：頁面 ViewModel 呼叫 `SetDetailMode(title)`
3. 回 Tabs 模式：呼叫 `SetTabsMode(tabIndex)`

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;

namespace unoTest.Controls;

/// <summary>
/// 客製化 TitleBar — 真正的 OS 層級標題列替換
///
/// 架構說明：
///   App.xaml.cs 設定 Window.ExtendsContentIntoTitleBar = true（Desktop 平台）
///   → 作業系統移除原生標題列，應用程式內容延伸至視窗最頂端
///   → Shell.xaml 的 Grid Row 0（此 UserControl）佔據原本 OS 標題列的位置
///
///   OnLoaded / Shell.Loaded 呼叫 Window.SetTitleBar(DragStrip)
///   → 只註冊頂部細條為可拖曳區域（對應系統手勢：拖曳移動、雙擊最大化）
///   → Tab / Button 等互動控件保持可點擊
///
///   平台差異：
///   - Desktop (Win32/macOS/Linux)：ExtendsContentIntoTitleBar = true，OS 原生按鈕疊層
///   - Android / iOS：此 UserControl 為普通內容列，不需要 SetTitleBar
///   - WASM：同 Mobile，瀏覽器沒有 OS 標題列
/// </summary>
public sealed partial class CustomTitleBar : UserControl
{
    // 防止初始化時 Checked 事件觸發導航
    private bool _isInitialized = false;

    public CustomTitleBar()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// 將指定元素註冊為 OS TitleBar 拖曳區域。
    /// 預設使用 Shell 層的獨立 drag strip，避免攔截 CustomTitleBar 的互動元素點擊。
    /// </summary>
    internal void BindAsWindowTitleBar(UIElement? dragRegion = null)
    {
#if !__WASM__ && !__ANDROID__ && !__IOS__
        if (!OperatingSystem.IsWindows())
            return;

        var window = global::unoTest.App.CurrentMainWindow;
        if (window is null)
            return;

        window.SetTitleBar(dragRegion ?? TitleBarDragRegion);
        ApplySystemButtonInsets();
#endif
    }

    // ================================================================
    // 初始化：向 OS 登記拖曳區域 + 調整系統按鈕 Inset
    // ================================================================

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isInitialized = true;

        // 初始 VisualState：TabsMode（顯示 Tab 導航）
        VisualStateManager.GoToState(this, "TabsMode", false);

        UpdatePlatformControls();

        // 預設先綁本地 drag strip；Shell Loaded 後會改綁 Shell 層 drag strip（最終生效）。
        BindAsWindowTitleBar(TitleBarDragRegion);
    }

    // ================================================================
    // DataContext 監聽：TitleBarViewModel 屬性改變時更新 VisualState
    // ================================================================

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is TitleBarViewModel vm)
        {
            vm.PropertyChanged -= ViewModel_PropertyChanged;
            vm.PropertyChanged += ViewModel_PropertyChanged;
            ApplyTitleBarMode(vm);
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TitleBarViewModel.Mode) or
                              nameof(TitleBarViewModel.IsTabsMode) or
                              nameof(TitleBarViewModel.IsDetailMode))
        {
            if (DataContext is TitleBarViewModel vm)
                ApplyTitleBarMode(vm);
        }

        if (e.PropertyName == nameof(TitleBarViewModel.SelectedTabIndex))
        {
            if (DataContext is TitleBarViewModel vm)
                SyncTabSelection(vm.SelectedTabIndex);
        }
    }

    private void ApplyTitleBarMode(TitleBarViewModel vm)
    {
        var stateName = vm.Mode == TitleBarMode.Tabs ? "TabsMode" : "DetailMode";
        VisualStateManager.GoToState(this, stateName, true);
    }

    private void SyncTabSelection(int index)
    {
        _isInitialized = false;
        try
        {
            if (index == 0) TabHome.IsChecked = true;
            else if (index == 1) TabProducts.IsChecked = true;
            else if (index == 2) TabSettings.IsChecked = true;
        }
        finally
        {
            _isInitialized = true;
        }
    }

    // ================================================================
    // Tab Checked：使用者點選 Tab → 呼叫 ViewModel Command 導航
    // ================================================================

    private async void NavTab_Checked(object sender, RoutedEventArgs e)
    {
        if (!_isInitialized) return;
        if (DataContext is not TitleBarViewModel vm) return;

        var tabIndex = sender switch
        {
            _ when sender == TabHome     => 0,
            _ when sender == TabProducts => 1,
            _ when sender == TabSettings => 2,
            _ => -1
        };

        if (tabIndex < 0 || tabIndex >= vm.Tabs.Count) return;

        var tab = vm.Tabs[tabIndex];
        if (vm.TabSelectedCommand.CanExecute(tab))
            await vm.TabSelectedCommand.ExecuteAsync(tab);
    }

    // ================================================================
    // 平台特定設定
    // ================================================================

    private void UpdatePlatformControls()
    {
        // Desktop：ExtendsContentIntoTitleBar = true 後，OS 會在視窗右上角疊層繪製
        //          原生的最小化/最大化/關閉按鈕。隱藏自訂按鈕避免重複。
        // Mobile / WASM：沒有視窗控制按鈕概念。
        WindowControlButtons.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// 讀取 OS 原生視窗按鈕所佔的寬度（RightInset），
    /// 設定 SystemButtonSpacer 讓我們的內容不被系統按鈕遮擋。
    ///
    /// Uno Platform 架構說明：
    /// - Uno 使用 WindowChrome（XAML ContentControl）包裹 Shell 並疊層繪製 Min/Max/Close 按鈕
    /// - WindowChrome 的按鈕疊在 Shell 右上角，會遮住我們的功能按鈕（通知/設定/使用者）
    /// - AppWindowTitleBar.RightInset 在 WinUI/WinAppSDK 上會回傳按鈕區域寬度
    /// - 但 Uno Platform 目前 RightInset 尚未實作（回傳 0），需要手動 fallback
    ///
    /// Windows Caption Buttons 寬度（Windows 11 標準）：
    ///   最小化(46) + 最大化(46) + 關閉(46) = 138px（標準 DPI）
    /// </summary>
    private void ApplySystemButtonInsets()
    {
        try
        {
            var appWindow = global::unoTest.App.CurrentMainWindow?.AppWindow;
            if (appWindow?.TitleBar is { } tb)
            {
                if (tb.RightInset > 0)
                {
                    // WinUI/WinAppSDK 路徑：直接使用 OS 回報的寬度
                    SystemButtonSpacer.Width = tb.RightInset;
                }
                else if (OperatingSystem.IsWindows())
                {
                    // Uno Platform Skia Win32 路徑：
                    // WindowChrome 的 XAML 按鈕寬度固定為 138px（3 × 46px）
                    // 使用 fallback 值確保我們的功能按鈕不被遮擋
                    SystemButtonSpacer.Width = 138;
                }
                // macOS / Linux：WindowChrome 不顯示 XAML 按鈕（IsCustomizationSupported=false）
                // 保留原始寬度 0 即可
            }
        }
        catch
        {
            // 部分平台可能不支援 AppWindow.TitleBar，使用預設值 0
        }
    }

    // ================================================================
    // 視窗控制按鈕（雖然被隱藏，但保留 handlers 以備後用）
    // ================================================================

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
#if !__WASM__ && !__ANDROID__ && !__IOS__
        try
        {
            // OverlappedPresenter 在 Uno Platform Skia Desktop（Win32/macOS/Linux）均可用
            if (global::unoTest.App.CurrentMainWindow?.AppWindow?.Presenter is OverlappedPresenter p)
                p.Minimize();
        }
        catch { /* 部分平台不支援 OverlappedPresenter */ }
#endif
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
#if !__WASM__ && !__ANDROID__ && !__IOS__
        try
        {
            var appWindow = global::unoTest.App.CurrentMainWindow?.AppWindow;
            if (appWindow?.Presenter is OverlappedPresenter p)
            {
                if (p.State == OverlappedPresenterState.Maximized)
                {
                    p.Restore();
                    MaximizeIcon.Glyph = "\uE922"; // 還原圖示
                }
                else
                {
                    p.Maximize();
                    MaximizeIcon.Glyph = "\uE923"; // 最大化圖示
                }
            }
        }
        catch { }
#endif
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        global::unoTest.App.CurrentMainWindow?.Close();
    }
}

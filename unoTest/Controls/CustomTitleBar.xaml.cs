using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace unoTest.Controls;

/// <summary>
/// 客製化 TitleBar 控件
/// 提供完整的標題列功能，包括導航、使用者資訊、主題切換、視窗控制等
/// </summary>
public sealed partial class CustomTitleBar : UserControl
{
    #region Dependency Properties

    /// <summary>
    /// 標題文字
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(CustomTitleBar),
            new PropertyMetadata("Uno Platform App"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// 是否顯示返回按鈕
    /// </summary>
    public static readonly DependencyProperty ShowBackButtonProperty =
        DependencyProperty.Register(
            nameof(ShowBackButton),
            typeof(bool),
            typeof(CustomTitleBar),
            new PropertyMetadata(true));

    public bool ShowBackButton
    {
        get => (bool)GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    /// <summary>
    /// 是否顯示視窗控制按鈕
    /// </summary>
    public static readonly DependencyProperty ShowWindowControlsProperty =
        DependencyProperty.Register(
            nameof(ShowWindowControls),
            typeof(bool),
            typeof(CustomTitleBar),
            new PropertyMetadata(true, OnShowWindowControlsChanged));

    public bool ShowWindowControls
    {
        get => (bool)GetValue(ShowWindowControlsProperty);
        set => SetValue(ShowWindowControlsProperty, value);
    }

    /// <summary>
    /// 是否顯示使用者名稱
    /// </summary>
    public static readonly DependencyProperty ShowUserNameProperty =
        DependencyProperty.Register(
            nameof(ShowUserName),
            typeof(bool),
            typeof(CustomTitleBar),
            new PropertyMetadata(true));

    public bool ShowUserName
    {
        get => (bool)GetValue(ShowUserNameProperty);
        set => SetValue(ShowUserNameProperty, value);
    }

    /// <summary>
    /// TitleBar 高度
    /// </summary>
    public static readonly DependencyProperty TitleBarHeightProperty =
        DependencyProperty.Register(
            nameof(TitleBarHeight),
            typeof(double),
            typeof(CustomTitleBar),
            new PropertyMetadata(48.0, OnTitleBarHeightChanged));

    public double TitleBarHeight
    {
        get => (double)GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler? BackRequested;
    public event EventHandler? HomeRequested;
    public event EventHandler? RefreshRequested;
    public event EventHandler? SearchRequested;
    public event EventHandler? SettingsRequested;
    public event EventHandler? ThemeToggleRequested;
    public event EventHandler? LogoutRequested;

    #endregion

    private Window? _parentWindow;

    public CustomTitleBar()
    {
        this.InitializeComponent();
        this.Loaded += CustomTitleBar_Loaded;
    }

    private void CustomTitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化視窗控制按鈕可見性
        UpdateWindowControlsVisibility();
        
        // 設定拖曳區域（僅限支援的平台）
        SetupDraggableArea();
    }

    #region Private Methods

    private void UpdateWindowControlsVisibility()
    {
        // 在 WebAssembly 和 Mobile 平台上隱藏視窗控制按鈕
#if __WASM__ || __ANDROID__ || __IOS__
        WindowControlButtons.Visibility = Visibility.Collapsed;
#else
        WindowControlButtons.Visibility = ShowWindowControls ? Visibility.Visible : Visibility.Collapsed;
#endif
    }

    private void SetupDraggableArea()
    {
        // 在 Desktop 平台上設定拖曳區域
#if !__WASM__ && !__ANDROID__ && !__IOS__
        // 可以在這裡設定視窗拖曳功能
        // 需要獲取 Window 實例
#endif
    }

    /// <summary>
    /// 設定父視窗引用（用於視窗控制）
    /// </summary>
    public void SetParentWindow(Window window)
    {
        _parentWindow = window;
    }

    private static void OnShowWindowControlsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomTitleBar titleBar)
        {
            titleBar.UpdateWindowControlsVisibility();
        }
    }

    private static void OnTitleBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomTitleBar titleBar && e.NewValue is double height)
        {
            titleBar.TitleBarRoot.Height = height;
        }
    }

    #endregion

    #region Event Handlers

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        // 最小化視窗
#if !__WASM__ && !__ANDROID__ && !__IOS__
        // 實作視窗最小化
        // 注意：需要平台特定實作
#endif
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        // 最大化/還原視窗
#if !__WASM__ && !__ANDROID__ && !__IOS__
        // 實作視窗最大化/還原
        // 切換圖標
#endif
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // 關閉視窗
#if !__WASM__ && !__ANDROID__ && !__IOS__
        _parentWindow?.Close();
#endif
    }

    #endregion
}

namespace unoTest.ViewModels;

// ============================================================
// TitleBar 導航 Tab 定義
// ============================================================
public class NavTab
{
    public int Index { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;    // Segoe MDL2 Glyph
    public string Route { get; init; } = string.Empty;
}

/// <summary>
/// 客製化 TitleBar 的 ViewModel
/// 
/// 架構說明：
/// - TitleBarStateService（Singleton）是頁面與 TitleBar 的溝通橋樑
/// - 頁面 ViewModel 呼叫 StateService 切換模式（Tabs/Detail）
/// - TitleBarViewModel 監聽 StateService 變更並更新 UI 屬性
/// - CustomTitleBar XAML 透過 VisualStateManager 對應 Mode 切換外觀
/// </summary>
public partial class TitleBarViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IThemeService? _themeService;
    private readonly TitleBarStateService _stateService;

    // ============================================================
    // Tab 定義（應用程式的主要頁面）
    // ============================================================
    public List<NavTab> Tabs { get; } =
    [
        new NavTab { Index = 0, Title = "首頁",   Icon = "\uE80F", Route = "DemoIndex"   },
        new NavTab { Index = 1, Title = "產品",   Icon = "\uE8A5", Route = "ProductList" },
        new NavTab { Index = 2, Title = "設定",   Icon = "\uE713", Route = "Settings"    },
    ];

    // ============================================================
    // TitleBar 模式與狀態
    // ============================================================

    /// <summary>目前顯示模式（Tabs=主頁面，Detail=詳情頁）</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTabsMode))]
    [NotifyPropertyChangedFor(nameof(IsDetailMode))]
    private TitleBarMode _mode = TitleBarMode.Tabs;

    /// <summary>Detail 模式時顯示的頁面標題</summary>
    [ObservableProperty]
    private string _detailTitle = string.Empty;

    /// <summary>目前選中的 Tab 索引（對應 Tabs list）</summary>
    [ObservableProperty]
    private int _selectedTabIndex;

    public bool IsTabsMode  => Mode == TitleBarMode.Tabs;
    public bool IsDetailMode => Mode == TitleBarMode.Detail;

    // ============================================================
    // 使用者資訊
    // ============================================================

    [ObservableProperty]
    private string _userName = "訪客";

    [ObservableProperty]
    private string? _userAvatar;

    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private bool _hasNotifications;

    [ObservableProperty]
    private int _notificationCount;

    // ============================================================
    // Commands
    // ============================================================

    /// <summary>Tab 被點選時執行（傳入 NavTab 物件）</summary>
    public IAsyncRelayCommand<NavTab?> TabSelectedCommand { get; }

    /// <summary>Detail 模式的返回按鈕</summary>
    public IAsyncRelayCommand BackCommand { get; }

    public IRelayCommand ToggleThemeCommand { get; }
    public IAsyncRelayCommand NotificationCommand { get; }
    public IAsyncRelayCommand SettingsCommand { get; }

    // ============================================================
    // Constructor
    // ============================================================

    public TitleBarViewModel(
        INavigator navigator,
        TitleBarStateService stateService,
        IThemeService? themeService = null)
    {
        _navigator = navigator;
        _stateService = stateService;
        _themeService = themeService;

        TabSelectedCommand    = new AsyncRelayCommand<NavTab?>(OnTabSelectedAsync);
        BackCommand           = new AsyncRelayCommand(GoBackAsync);
        ToggleThemeCommand    = new RelayCommand(ToggleTheme);
        NotificationCommand   = new AsyncRelayCommand(OpenNotificationsAsync);
        SettingsCommand       = new AsyncRelayCommand(OpenSettingsAsync);

        // 監聽 StateService 變更（頁面 ViewModel 會呼叫它切換模式）
        _stateService.PropertyChanged += OnStateChanged;

        // 從系統判斷預設主題
        IsDarkTheme = Application.Current?.RequestedTheme == ApplicationTheme.Dark;

        // 同步初始狀態
        SyncFromStateService();
    }

    // ============================================================
    // Private Handlers
    // ============================================================

    private void OnStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        => SyncFromStateService();

    private void SyncFromStateService()
    {
        Mode             = _stateService.Mode;
        DetailTitle      = _stateService.DetailPageTitle;
        SelectedTabIndex = _stateService.SelectedTabIndex;
    }

    /// <summary>
    /// 使用者點選 Tab 時：更新狀態 + 導航到對應頁面
    /// </summary>
    private async Task OnTabSelectedAsync(NavTab? tab)
    {
        if (tab is null) return;
        _stateService.SetTabsMode(tab.Index);
        await _navigator.NavigateRouteAsync(this, tab.Route);
    }

    private async Task GoBackAsync()
    {
        try
        {
            await _navigator.GoBack(this);
        }
        catch { /* 已在根頁面時沒有上一頁 */ }
        // 返回後無論成功與否，恢復 Tabs 模式
        _stateService.SetTabsMode();
    }

    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        _themeService?.SetThemeAsync(IsDarkTheme ? AppTheme.Dark : AppTheme.Light);
    }

    private async Task OpenNotificationsAsync()
    {
        HasNotifications = false;
        await Task.CompletedTask;
    }

    private async Task OpenSettingsAsync()
    {
        _stateService.SetTabsMode(tabIndex: 2);
        await _navigator.NavigateRouteAsync(this, "Settings");
    }

    // ============================================================
    // Public Helpers（供外部呼叫更新使用者資訊）
    // ============================================================

    public void UpdateUserInfo(string userName, string? avatarUrl = null)
    {
        UserName   = userName;
        UserAvatar = avatarUrl;
    }

    public void SetNotifications(int count)
    {
        NotificationCount = count;
        HasNotifications  = count > 0;
    }
}

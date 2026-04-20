namespace unoTest.ViewModels;

/// <summary>
/// 客製化 TitleBar 的 ViewModel
/// 處理標題列的所有互動邏輯
/// </summary>
public partial class TitleBarViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IThemeService? _themeService;
    private readonly IStringLocalizer _localizer;

    #region Observable Properties

    [ObservableProperty]
    private string _title = "Uno Platform App";

    [ObservableProperty]
    private string _userName = "訪客";

    [ObservableProperty]
    private string? _userAvatar;

    [ObservableProperty]
    private bool _showUserName = true;

    [ObservableProperty]
    private bool _hasNotifications;

    [ObservableProperty]
    private int _notificationCount;

    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private bool _isLoggedIn;

    #endregion

    #region Commands

    public IAsyncRelayCommand BackCommand { get; }
    public IAsyncRelayCommand HomeCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand NotificationCommand { get; }
    public IRelayCommand ToggleThemeCommand { get; }
    public IAsyncRelayCommand SettingsCommand { get; }
    public IAsyncRelayCommand ProfileCommand { get; }
    public IAsyncRelayCommand AccountSettingsCommand { get; }
    public IAsyncRelayCommand LogoutCommand { get; }

    #endregion

    public TitleBarViewModel(
        INavigator navigator,
        IThemeService? themeService = null,
        IStringLocalizer<TitleBarViewModel>? localizer = null)
    {
        _navigator = navigator;
        _themeService = themeService;
        _localizer = localizer!;

        // 初始化 Commands
        BackCommand = new AsyncRelayCommand(GoBackAsync, () => CanGoBack);
        HomeCommand = new AsyncRelayCommand(GoHomeAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        SearchCommand = new AsyncRelayCommand(OpenSearchAsync);
        NotificationCommand = new AsyncRelayCommand(OpenNotificationsAsync);
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        SettingsCommand = new AsyncRelayCommand(OpenSettingsAsync);
        ProfileCommand = new AsyncRelayCommand(OpenProfileAsync);
        AccountSettingsCommand = new AsyncRelayCommand(OpenAccountSettingsAsync);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);

        // 初始化主題狀態
        InitializeTheme();
    }

    #region Command Implementations

    private async Task GoBackAsync()
    {
        await _navigator.GoBack(this);
        UpdateCanGoBack();
    }

    private async Task GoHomeAsync()
    {
        await _navigator.NavigateViewModelAsync<MainViewModel>(this);
        UpdateCanGoBack();
    }

    private async Task RefreshAsync()
    {
        // 觸發頁面重新整理事件
        // 可以透過 Messenger 發送重新整理請求
        await Task.CompletedTask;
    }

    private async Task OpenSearchAsync()
    {
        // 開啟搜尋對話框或導航到搜尋頁面
        // await _navigator.NavigateViewModelAsync<SearchViewModel>(this);
        await Task.CompletedTask;
    }

    private async Task OpenNotificationsAsync()
    {
        // 開啟通知面板或導航到通知頁面
        // await _navigator.NavigateViewModelAsync<NotificationsViewModel>(this);
        HasNotifications = false; // 標記已讀
        await Task.CompletedTask;
    }

    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        _themeService?.SetThemeAsync(IsDarkTheme ? AppTheme.Dark : AppTheme.Light);
    }

    private async Task OpenSettingsAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Settings");
    }

    private async Task OpenProfileAsync()
    {
        // Profile 功能待實作
        await Task.CompletedTask;
    }

    private async Task OpenAccountSettingsAsync()
    {
        // Account Settings 功能待實作，可導向設定頁面
        await _navigator.NavigateRouteAsync(this, "Settings");
    }

    private async Task LogoutAsync()
    {
        // 執行登出邏輯
        IsLoggedIn = false;
        UserName = "訪客";
        UserAvatar = null;

        // 導航到登入頁面
        await _navigator.NavigateRouteAsync(this, "Login");
    }

    #endregion

    #region Helper Methods

    private void InitializeTheme()
    {
        // 從系統取得目前主題
        if (Application.Current?.RequestedTheme == ApplicationTheme.Dark)
        {
            IsDarkTheme = true;
        }
    }

    public void UpdateCanGoBack()
    {
        // 更新返回按鈕狀態
        // CanGoBack = _navigator.CanGoBack;
    }

    public void UpdateUserInfo(string userName, string? avatarUrl = null)
    {
        UserName = userName;
        UserAvatar = avatarUrl;
        IsLoggedIn = !string.IsNullOrEmpty(userName) && userName != "訪客";
    }

    public void SetNotifications(int count)
    {
        NotificationCount = count;
        HasNotifications = count > 0;
    }

    #endregion
}

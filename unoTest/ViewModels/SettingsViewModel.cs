namespace unoTest.ViewModels;

/// <summary>
/// 設定頁面 ViewModel
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService? _themeService;
    private readonly TitleBarStateService? _titleBarState;

    #region Observable Properties

    [ObservableProperty]
    private int _selectedThemeIndex = 0; // 0=系統, 1=淺色, 2=深色

    [ObservableProperty]
    private int _selectedFontSizeIndex = 1; // 1=標準

    [ObservableProperty]
    private string _selectedLanguage = "繁體中文";

    [ObservableProperty]
    private bool _isNotificationsEnabled = true;

    [ObservableProperty]
    private bool _isSoundEnabled = true;

    #endregion

    #region Properties

    public string[] AvailableLanguages => new[]
    {
        "繁體中文",
        "English",
        "Español",
        "Français",
        "Português"
    };

    public string AppName => "Uno Platform App";
    public string AppVersion => "版本 1.0.0";

    #endregion

    #region Commands

    public IAsyncRelayCommand ClearCacheCommand { get; }
    public IAsyncRelayCommand ExportDataCommand { get; }
    public IAsyncRelayCommand DeleteAccountCommand { get; }
    public IRelayCommand ShowLicensesCommand { get; }
    public IAsyncRelayCommand CheckUpdateCommand { get; }

    #endregion

    public SettingsViewModel(
        IThemeService? themeService = null,
        TitleBarStateService? titleBarState = null)
    {
        _themeService = themeService;
        _titleBarState = titleBarState;

        ClearCacheCommand = new AsyncRelayCommand(ClearCacheAsync);
        ExportDataCommand = new AsyncRelayCommand(ExportDataAsync);
        DeleteAccountCommand = new AsyncRelayCommand(DeleteAccountAsync);
        ShowLicensesCommand = new RelayCommand(ShowLicenses);
        CheckUpdateCommand = new AsyncRelayCommand(CheckUpdateAsync);

        // 載入目前設定
        LoadSettings();
        _titleBarState?.SetTabsMode(tabIndex: 2);
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        var theme = value switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.System
        };
        _themeService?.SetThemeAsync(theme);
    }

    private void LoadSettings()
    {
        // 從設定載入目前值
        // TODO: 實作從 IOptions 或本地儲存讀取設定
    }

    private async Task ClearCacheAsync()
    {
        // TODO: 實作清除快取
        await Task.Delay(500);
    }

    private async Task ExportDataAsync()
    {
        // TODO: 實作匯出資料
        await Task.Delay(500);
    }

    private async Task DeleteAccountAsync()
    {
        // TODO: 顯示確認對話框並刪除帳號
        await Task.Delay(500);
    }

    private void ShowLicenses()
    {
        // TODO: 顯示開源授權資訊
    }

    private async Task CheckUpdateAsync()
    {
        // TODO: 檢查更新
        await Task.Delay(1000);
    }
}

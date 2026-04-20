using System.ComponentModel.DataAnnotations;

namespace unoTest.Presentation;

/// <summary>
/// 登入頁面 ViewModel
/// 處理登入邏輯、表單驗證、第三方登入等
/// </summary>
public partial class LoginViewModel : ObservableValidator
{
    private readonly INavigator _navigator;
    private readonly IAuthService _authService;
    private readonly IStringLocalizer _localizer;

    #region Observable Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLogin))]
    [Required(ErrorMessage = "請輸入使用者名稱或 Email")]
    [EmailAddress(ErrorMessage = "Email 格式不正確")]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLogin))]
    [Required(ErrorMessage = "請輸入密碼")]
    [MinLength(6, ErrorMessage = "密碼至少需要 6 個字元")]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLogin))]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    #endregion

    #region Computed Properties

    public bool CanLogin => !IsLoading && 
                            !string.IsNullOrWhiteSpace(Username) && 
                            !string.IsNullOrWhiteSpace(Password);

    #endregion

    #region Commands

    public IAsyncRelayCommand LoginCommand { get; }
    public IAsyncRelayCommand GoogleLoginCommand { get; }
    public IAsyncRelayCommand MicrosoftLoginCommand { get; }
    public IAsyncRelayCommand AppleLoginCommand { get; }
    public IAsyncRelayCommand ForgotPasswordCommand { get; }
    public IAsyncRelayCommand RegisterCommand { get; }

    #endregion

    public LoginViewModel(
        INavigator navigator,
        IAuthService? authService = null,
        IStringLocalizer<LoginViewModel>? localizer = null)
    {
        _navigator = navigator;
        _authService = authService ?? new MockAuthService();
        _localizer = localizer!;

        // 初始化 Commands
        LoginCommand = new AsyncRelayCommand(LoginAsync, () => CanLogin);
        GoogleLoginCommand = new AsyncRelayCommand(LoginWithGoogleAsync);
        MicrosoftLoginCommand = new AsyncRelayCommand(LoginWithMicrosoftAsync);
        AppleLoginCommand = new AsyncRelayCommand(LoginWithAppleAsync);
        ForgotPasswordCommand = new AsyncRelayCommand(NavigateToForgotPasswordAsync);
        RegisterCommand = new AsyncRelayCommand(NavigateToRegisterAsync);
    }

    #region Login Methods

    private async Task LoginAsync()
    {
        try
        {
            ClearError();
            IsLoading = true;

            // 驗證表單
            ValidateAllProperties();
            if (HasErrors)
            {
                ShowError("請檢查輸入的資料");
                return;
            }

            // 執行登入
            var result = await _authService.LoginAsync(Username, Password, RememberMe);

            if (result.IsSuccess)
            {
                // 登入成功，導航到主頁
                await _navigator.NavigateViewModelAsync<MainViewModel>(this);
            }
            else
            {
                ShowError(result.ErrorMessage ?? "登入失敗，請檢查帳號密碼");
            }
        }
        catch (Exception ex)
        {
            ShowError($"發生錯誤：{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoginWithGoogleAsync()
    {
        try
        {
            ClearError();
            IsLoading = true;

            var result = await _authService.LoginWithGoogleAsync();

            if (result.IsSuccess)
            {
                await _navigator.NavigateViewModelAsync<MainViewModel>(this);
            }
            else
            {
                ShowError(result.ErrorMessage ?? "Google 登入失敗");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Google 登入發生錯誤：{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoginWithMicrosoftAsync()
    {
        try
        {
            ClearError();
            IsLoading = true;

            var result = await _authService.LoginWithMicrosoftAsync();

            if (result.IsSuccess)
            {
                await _navigator.NavigateViewModelAsync<MainViewModel>(this);
            }
            else
            {
                ShowError(result.ErrorMessage ?? "Microsoft 登入失敗");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Microsoft 登入發生錯誤：{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoginWithAppleAsync()
    {
        try
        {
            ClearError();
            IsLoading = true;

            var result = await _authService.LoginWithAppleAsync();

            if (result.IsSuccess)
            {
                await _navigator.NavigateViewModelAsync<MainViewModel>(this);
            }
            else
            {
                ShowError(result.ErrorMessage ?? "Apple 登入失敗");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Apple 登入發生錯誤：{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task NavigateToForgotPasswordAsync()
    {
        await _navigator.NavigateViewModelAsync<ForgotPasswordViewModel>(this);
    }

    private async Task NavigateToRegisterAsync()
    {
        await _navigator.NavigateViewModelAsync<RegisterViewModel>(this);
    }

    #endregion

    #region Helper Methods

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    #endregion
}

/// <summary>
/// 忘記密碼頁面 ViewModel（佔位）
/// </summary>
public partial class ForgotPasswordViewModel : ObservableObject
{
    public string Title => "忘記密碼";
}

/// <summary>
/// 註冊頁面 ViewModel（佔位）
/// </summary>
public partial class RegisterViewModel : ObservableObject
{
    public string Title => "註冊";
}

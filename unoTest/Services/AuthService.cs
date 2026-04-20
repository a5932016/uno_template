namespace unoTest.Services;

/// <summary>
/// 認證服務介面
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 使用帳號密碼登入
    /// </summary>
    Task<AuthResult> LoginAsync(string username, string password, bool rememberMe = false);

    /// <summary>
    /// 使用 Google 帳號登入
    /// </summary>
    Task<AuthResult> LoginWithGoogleAsync();

    /// <summary>
    /// 使用 Microsoft 帳號登入
    /// </summary>
    Task<AuthResult> LoginWithMicrosoftAsync();

    /// <summary>
    /// 使用 Apple 帳號登入
    /// </summary>
    Task<AuthResult> LoginWithAppleAsync();

    /// <summary>
    /// 登出
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// 取得目前使用者
    /// </summary>
    Task<UserInfo?> GetCurrentUserAsync();

    /// <summary>
    /// 檢查是否已登入
    /// </summary>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// 刷新 Token
    /// </summary>
    Task<AuthResult> RefreshTokenAsync();
}

/// <summary>
/// 認證結果
/// </summary>
public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfo? User { get; set; }

    public static AuthResult Success(UserInfo user, string accessToken, string? refreshToken = null)
    {
        return new AuthResult
        {
            IsSuccess = true,
            User = user,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public static AuthResult Failure(string message)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = message
        };
    }
}

/// <summary>
/// 使用者資訊
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Mock 認證服務（開發測試用）
/// </summary>
public class MockAuthService : IAuthService
{
    private UserInfo? _currentUser;

    public async Task<AuthResult> LoginAsync(string username, string password, bool rememberMe = false)
    {
        // 模擬網路延遲
        await Task.Delay(1500);

        // 簡單的驗證邏輯（實際應用中應該呼叫後端 API）
        if (username == "admin" && password == "123456")
        {
            _currentUser = new UserInfo
            {
                Id = "1",
                Username = "admin",
                Email = "admin@example.com",
                DisplayName = "系統管理員",
                Roles = new List<string> { "Admin", "User" }
            };
            return AuthResult.Success(_currentUser, "mock-access-token");
        }

        if (username == "user" && password == "123456")
        {
            _currentUser = new UserInfo
            {
                Id = "2",
                Username = "user",
                Email = "user@example.com",
                DisplayName = "一般使用者",
                Roles = new List<string> { "User" }
            };
            return AuthResult.Success(_currentUser, "mock-access-token");
        }

        return AuthResult.Failure("帳號或密碼錯誤");
    }

    public async Task<AuthResult> LoginWithGoogleAsync()
    {
        await Task.Delay(1000);
        _currentUser = new UserInfo
        {
            Id = "google-123",
            Username = "google_user",
            Email = "user@gmail.com",
            DisplayName = "Google 使用者"
        };
        return AuthResult.Success(_currentUser, "google-access-token");
    }

    public async Task<AuthResult> LoginWithMicrosoftAsync()
    {
        await Task.Delay(1000);
        _currentUser = new UserInfo
        {
            Id = "ms-123",
            Username = "microsoft_user",
            Email = "user@outlook.com",
            DisplayName = "Microsoft 使用者"
        };
        return AuthResult.Success(_currentUser, "ms-access-token");
    }

    public async Task<AuthResult> LoginWithAppleAsync()
    {
        await Task.Delay(1000);
        _currentUser = new UserInfo
        {
            Id = "apple-123",
            Username = "apple_user",
            Email = "user@icloud.com",
            DisplayName = "Apple 使用者"
        };
        return AuthResult.Success(_currentUser, "apple-access-token");
    }

    public Task LogoutAsync()
    {
        _currentUser = null;
        return Task.CompletedTask;
    }

    public Task<UserInfo?> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(_currentUser != null);
    }

    public async Task<AuthResult> RefreshTokenAsync()
    {
        await Task.Delay(500);
        if (_currentUser != null)
        {
            return AuthResult.Success(_currentUser, "refreshed-token");
        }
        return AuthResult.Failure("未登入");
    }
}

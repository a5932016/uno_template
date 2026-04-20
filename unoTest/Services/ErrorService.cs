namespace unoTest.Services;

/// <summary>
/// 錯誤代碼定義
/// 格式: [模組]_[類型]_[編號]
/// </summary>
public static class ErrorCodes
{
    #region 通用錯誤 (GEN_xxx)

    public const string GEN_UNKNOWN = "GEN_ERR_000";
    public const string GEN_VALIDATION_FAILED = "GEN_ERR_001";
    public const string GEN_NOT_FOUND = "GEN_ERR_002";
    public const string GEN_UNAUTHORIZED = "GEN_ERR_003";
    public const string GEN_FORBIDDEN = "GEN_ERR_004";
    public const string GEN_TIMEOUT = "GEN_ERR_005";
    public const string GEN_CANCELLED = "GEN_ERR_006";

    #endregion

    #region 網路錯誤 (NET_xxx)

    public const string NET_CONNECTION_FAILED = "NET_ERR_001";
    public const string NET_TIMEOUT = "NET_ERR_002";
    public const string NET_DNS_FAILED = "NET_ERR_003";
    public const string NET_SSL_ERROR = "NET_ERR_004";
    public const string NET_SERVER_ERROR = "NET_ERR_005";
    public const string NET_SERVICE_UNAVAILABLE = "NET_ERR_006";

    #endregion

    #region 認證錯誤 (AUTH_xxx)

    public const string AUTH_INVALID_CREDENTIALS = "AUTH_ERR_001";
    public const string AUTH_ACCOUNT_LOCKED = "AUTH_ERR_002";
    public const string AUTH_TOKEN_EXPIRED = "AUTH_ERR_003";
    public const string AUTH_TOKEN_INVALID = "AUTH_ERR_004";
    public const string AUTH_SESSION_EXPIRED = "AUTH_ERR_005";
    public const string AUTH_MFA_REQUIRED = "AUTH_ERR_006";
    public const string AUTH_PASSWORD_EXPIRED = "AUTH_ERR_007";

    #endregion

    #region 資料錯誤 (DATA_xxx)

    public const string DATA_INVALID_FORMAT = "DATA_ERR_001";
    public const string DATA_DUPLICATE_KEY = "DATA_ERR_002";
    public const string DATA_INTEGRITY_VIOLATION = "DATA_ERR_003";
    public const string DATA_CONCURRENCY_CONFLICT = "DATA_ERR_004";
    public const string DATA_NOT_FOUND = "DATA_ERR_005";
    public const string DATA_SAVE_FAILED = "DATA_ERR_006";
    public const string DATA_DELETE_FAILED = "DATA_ERR_007";

    #endregion

    #region 檔案錯誤 (FILE_xxx)

    public const string FILE_NOT_FOUND = "FILE_ERR_001";
    public const string FILE_ACCESS_DENIED = "FILE_ERR_002";
    public const string FILE_TOO_LARGE = "FILE_ERR_003";
    public const string FILE_INVALID_TYPE = "FILE_ERR_004";
    public const string FILE_UPLOAD_FAILED = "FILE_ERR_005";
    public const string FILE_DOWNLOAD_FAILED = "FILE_ERR_006";
    public const string FILE_CORRUPTED = "FILE_ERR_007";

    #endregion

    #region 業務錯誤 (BIZ_xxx)

    public const string BIZ_INVALID_OPERATION = "BIZ_ERR_001";
    public const string BIZ_INSUFFICIENT_BALANCE = "BIZ_ERR_002";
    public const string BIZ_QUOTA_EXCEEDED = "BIZ_ERR_003";
    public const string BIZ_NOT_AVAILABLE = "BIZ_ERR_004";
    public const string BIZ_ALREADY_EXISTS = "BIZ_ERR_005";
    public const string BIZ_DEPENDENCY_ERROR = "BIZ_ERR_006";

    #endregion
}

/// <summary>
/// 錯誤資訊
/// </summary>
public class ErrorInfo
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? UserMessage { get; set; }
    public string? Suggestion { get; set; }
    public Exception? Exception { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }

    public ErrorInfo() { }

    public ErrorInfo(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public ErrorInfo(string code, string message, Exception exception) : this(code, message)
    {
        Exception = exception;
    }

    public override string ToString()
    {
        return $"[{Code}] {Message}";
    }
}

/// <summary>
/// 錯誤處理服務介面
/// </summary>
public interface IErrorService
{
    /// <summary>
    /// 取得錯誤資訊
    /// </summary>
    ErrorInfo GetErrorInfo(string code);

    /// <summary>
    /// 建立錯誤
    /// </summary>
    ErrorInfo CreateError(string code, string? customMessage = null, Exception? exception = null);

    /// <summary>
    /// 從例外建立錯誤
    /// </summary>
    ErrorInfo FromException(Exception exception);

    /// <summary>
    /// 記錄錯誤
    /// </summary>
    void LogError(ErrorInfo error);

    /// <summary>
    /// 取得使用者友善訊息
    /// </summary>
    string GetUserMessage(string code);

    /// <summary>
    /// 取得建議解決方案
    /// </summary>
    string? GetSuggestion(string code);
}

/// <summary>
/// 錯誤處理服務實作
/// </summary>
public class ErrorService : IErrorService
{
    private readonly ILogger<ErrorService>? _logger;
    private readonly IStringLocalizer? _localizer;
    private readonly Dictionary<string, ErrorDefinition> _errorDefinitions;

    public ErrorService(ILogger<ErrorService>? logger = null, IStringLocalizer? localizer = null)
    {
        _logger = logger;
        _localizer = localizer;
        _errorDefinitions = InitializeErrorDefinitions();
    }

    public ErrorInfo GetErrorInfo(string code)
    {
        if (_errorDefinitions.TryGetValue(code, out var definition))
        {
            return new ErrorInfo
            {
                Code = code,
                Message = definition.Message,
                UserMessage = GetLocalizedMessage(code, definition.UserMessage),
                Suggestion = GetLocalizedSuggestion(code, definition.Suggestion)
            };
        }

        return new ErrorInfo
        {
            Code = code,
            Message = "未知錯誤",
            UserMessage = "發生未預期的錯誤"
        };
    }

    public ErrorInfo CreateError(string code, string? customMessage = null, Exception? exception = null)
    {
        var error = GetErrorInfo(code);
        if (!string.IsNullOrEmpty(customMessage))
        {
            error.Message = customMessage;
        }
        error.Exception = exception;
        error.RequestId = Guid.NewGuid().ToString("N")[..8];
        return error;
    }

    public ErrorInfo FromException(Exception exception)
    {
        var code = MapExceptionToCode(exception);
        return CreateError(code, exception.Message, exception);
    }

    public void LogError(ErrorInfo error)
    {
        var logMessage = $"[{error.Code}] {error.Message}";
        if (!string.IsNullOrEmpty(error.RequestId))
        {
            logMessage = $"RequestId: {error.RequestId} - {logMessage}";
        }

        if (error.Exception != null)
        {
            _logger?.LogError(error.Exception, logMessage);
        }
        else
        {
            _logger?.LogError(logMessage);
        }
    }

    public string GetUserMessage(string code)
    {
        if (_errorDefinitions.TryGetValue(code, out var definition))
        {
            return GetLocalizedMessage(code, definition.UserMessage);
        }
        return "發生錯誤，請稍後再試";
    }

    public string? GetSuggestion(string code)
    {
        if (_errorDefinitions.TryGetValue(code, out var definition))
        {
            return GetLocalizedSuggestion(code, definition.Suggestion);
        }
        return null;
    }

    private string GetLocalizedMessage(string code, string defaultMessage)
    {
        // 嘗試從多語系資源取得
        var key = $"Error_{code.Replace("_", "")}";
        var localized = _localizer?[key];
        return localized?.ResourceNotFound == false ? localized.Value : defaultMessage;
    }

    private string? GetLocalizedSuggestion(string code, string? defaultSuggestion)
    {
        if (string.IsNullOrEmpty(defaultSuggestion)) return null;

        var key = $"ErrorSuggestion_{code.Replace("_", "")}";
        var localized = _localizer?[key];
        return localized?.ResourceNotFound == false ? localized.Value : defaultSuggestion;
    }

    private static string MapExceptionToCode(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => ErrorCodes.NET_CONNECTION_FAILED,
            TimeoutException => ErrorCodes.GEN_TIMEOUT,
            OperationCanceledException => ErrorCodes.GEN_CANCELLED,
            UnauthorizedAccessException => ErrorCodes.GEN_UNAUTHORIZED,
            FileNotFoundException => ErrorCodes.FILE_NOT_FOUND,
            ArgumentException => ErrorCodes.GEN_VALIDATION_FAILED,
            InvalidOperationException => ErrorCodes.BIZ_INVALID_OPERATION,
            KeyNotFoundException => ErrorCodes.DATA_NOT_FOUND,
            _ => ErrorCodes.GEN_UNKNOWN
        };
    }

    private static Dictionary<string, ErrorDefinition> InitializeErrorDefinitions()
    {
        return new Dictionary<string, ErrorDefinition>
        {
            // 通用錯誤
            [ErrorCodes.GEN_UNKNOWN] = new("未知錯誤", "發生未預期的錯誤，請稍後再試", "請重新整理頁面或聯繫客服"),
            [ErrorCodes.GEN_VALIDATION_FAILED] = new("驗證失敗", "輸入的資料格式不正確", "請檢查輸入的資料是否正確"),
            [ErrorCodes.GEN_NOT_FOUND] = new("找不到資源", "您要找的內容不存在", "請確認網址是否正確"),
            [ErrorCodes.GEN_UNAUTHORIZED] = new("未授權", "您需要登入才能進行此操作", "請先登入您的帳號"),
            [ErrorCodes.GEN_FORBIDDEN] = new("禁止存取", "您沒有權限執行此操作", "請聯繫管理員取得權限"),
            [ErrorCodes.GEN_TIMEOUT] = new("操作逾時", "操作時間過長已逾時", "請稍後再試"),
            [ErrorCodes.GEN_CANCELLED] = new("操作已取消", "操作已被取消", null),

            // 網路錯誤
            [ErrorCodes.NET_CONNECTION_FAILED] = new("連線失敗", "無法連線到伺服器", "請檢查您的網路連線"),
            [ErrorCodes.NET_TIMEOUT] = new("連線逾時", "連線逾時", "請檢查網路狀況或稍後再試"),
            [ErrorCodes.NET_DNS_FAILED] = new("DNS 解析失敗", "無法解析伺服器位址", "請檢查網路設定"),
            [ErrorCodes.NET_SSL_ERROR] = new("安全連線錯誤", "無法建立安全連線", "請確認時間設定是否正確"),
            [ErrorCodes.NET_SERVER_ERROR] = new("伺服器錯誤", "伺服器發生錯誤", "請稍後再試"),
            [ErrorCodes.NET_SERVICE_UNAVAILABLE] = new("服務暫停", "服務暫時無法使用", "系統維護中，請稍後再試"),

            // 認證錯誤
            [ErrorCodes.AUTH_INVALID_CREDENTIALS] = new("登入失敗", "帳號或密碼錯誤", "請確認您的帳號密碼是否正確"),
            [ErrorCodes.AUTH_ACCOUNT_LOCKED] = new("帳號已鎖定", "您的帳號已被鎖定", "請聯繫客服解鎖帳號"),
            [ErrorCodes.AUTH_TOKEN_EXPIRED] = new("Token 已過期", "登入狀態已過期", "請重新登入"),
            [ErrorCodes.AUTH_TOKEN_INVALID] = new("Token 無效", "登入狀態無效", "請重新登入"),
            [ErrorCodes.AUTH_SESSION_EXPIRED] = new("工作階段已過期", "您已登出", "請重新登入"),
            [ErrorCodes.AUTH_MFA_REQUIRED] = new("需要雙重驗證", "請完成雙重驗證", "請輸入驗證碼"),
            [ErrorCodes.AUTH_PASSWORD_EXPIRED] = new("密碼已過期", "您的密碼已過期", "請更新您的密碼"),

            // 資料錯誤
            [ErrorCodes.DATA_INVALID_FORMAT] = new("資料格式錯誤", "資料格式不正確", "請檢查輸入的資料"),
            [ErrorCodes.DATA_DUPLICATE_KEY] = new("重複的資料", "此資料已存在", "請使用其他名稱或編號"),
            [ErrorCodes.DATA_INTEGRITY_VIOLATION] = new("資料完整性錯誤", "資料關聯錯誤", "請檢查相關資料是否存在"),
            [ErrorCodes.DATA_CONCURRENCY_CONFLICT] = new("資料已被修改", "此資料已被其他人修改", "請重新載入資料後再試"),
            [ErrorCodes.DATA_NOT_FOUND] = new("找不到資料", "查無此資料", "請確認資料是否存在"),
            [ErrorCodes.DATA_SAVE_FAILED] = new("儲存失敗", "無法儲存資料", "請稍後再試"),
            [ErrorCodes.DATA_DELETE_FAILED] = new("刪除失敗", "無法刪除資料", "可能有關聯資料，請先解除關聯"),

            // 檔案錯誤
            [ErrorCodes.FILE_NOT_FOUND] = new("找不到檔案", "指定的檔案不存在", "請確認檔案路徑是否正確"),
            [ErrorCodes.FILE_ACCESS_DENIED] = new("存取被拒", "沒有權限存取此檔案", "請確認您有足夠的權限"),
            [ErrorCodes.FILE_TOO_LARGE] = new("檔案過大", "檔案大小超過限制", "請選擇較小的檔案"),
            [ErrorCodes.FILE_INVALID_TYPE] = new("不支援的檔案類型", "此檔案類型不被支援", "請使用支援的檔案格式"),
            [ErrorCodes.FILE_UPLOAD_FAILED] = new("上傳失敗", "檔案上傳失敗", "請檢查網路連線後重試"),
            [ErrorCodes.FILE_DOWNLOAD_FAILED] = new("下載失敗", "檔案下載失敗", "請檢查網路連線後重試"),
            [ErrorCodes.FILE_CORRUPTED] = new("檔案損壞", "檔案可能已損壞", "請重新上傳檔案"),

            // 業務錯誤
            [ErrorCodes.BIZ_INVALID_OPERATION] = new("無效的操作", "此操作目前無法執行", "請確認操作條件是否滿足"),
            [ErrorCodes.BIZ_INSUFFICIENT_BALANCE] = new("餘額不足", "餘額不足以完成此操作", "請先儲值"),
            [ErrorCodes.BIZ_QUOTA_EXCEEDED] = new("超過配額", "已達使用上限", "請升級方案或等待配額重置"),
            [ErrorCodes.BIZ_NOT_AVAILABLE] = new("服務暫不可用", "此功能目前無法使用", "請稍後再試"),
            [ErrorCodes.BIZ_ALREADY_EXISTS] = new("資料已存在", "此資料已經存在", "請使用其他資料"),
            [ErrorCodes.BIZ_DEPENDENCY_ERROR] = new("相依性錯誤", "相關資料處理失敗", "請確認相關資料是否正確"),
        };
    }

    private class ErrorDefinition
    {
        public string Message { get; }
        public string UserMessage { get; }
        public string? Suggestion { get; }

        public ErrorDefinition(string message, string userMessage, string? suggestion = null)
        {
            Message = message;
            UserMessage = userMessage;
            Suggestion = suggestion;
        }
    }
}

/// <summary>
/// 應用程式例外基底類別
/// </summary>
public class AppException : Exception
{
    public string ErrorCode { get; }
    public string? UserMessage { get; }
    public new Dictionary<string, object>? Data { get; init; }

    public AppException(string errorCode, string message, string? userMessage = null)
        : base(message)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
    }

    public AppException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// 驗證例外
/// </summary>
public class ValidationException : AppException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base(ErrorCodes.GEN_VALIDATION_FAILED, "驗證失敗", "輸入的資料有誤，請檢查後重試")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = new[] { message } })
    {
    }
}

/// <summary>
/// 業務例外
/// </summary>
public class BusinessException : AppException
{
    public BusinessException(string errorCode, string message, string? userMessage = null)
        : base(errorCode, message, userMessage ?? message)
    {
    }
}

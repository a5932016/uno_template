# LoginPage View README

## 目的
完整登入流程模板：輸入驗證、登入命令、錯誤回饋、第三方登入入口。

## 對應檔案
- `Presentation/LoginPage.xaml`
- `ViewModels/LoginViewModel.cs`
- `Services/AuthService.cs`

## 核心行為
- `ObservableValidator` 驗證帳號密碼
- `LoginCommand` 呼叫 `IAuthService.LoginAsync`
- 成功後導航 `MainViewModel`
- 失敗時顯示 `HasError/ErrorMessage`

## 依賴
- `INavigator`
- `IAuthService`
- `IStringLocalizer<LoginViewModel>`

## 給 Golang 後端工程師的修改建議
- 把 `IAuthService` 想成你的 Go API adapter。
- ViewModel 不應知道 token 儲存細節，交給 service。
- 錯誤碼（例如 401/429）請在 service 轉成可讀訊息再回 UI。

## 快速上手
1. 用真實 Auth API 取代 `MockAuthService`
2. 補齊 `ForgotPassword` / `Register` 路由
3. 加入 refresh token + logout flow

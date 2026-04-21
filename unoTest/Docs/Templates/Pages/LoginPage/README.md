# LoginPage Template

## 1. Template 目的
LoginPage 提供完整的登入模板，示範表單驗證、載入狀態、錯誤訊息與第三方登入命令。

## 2. 檔案組成
- `Presentation/LoginPage.xaml`
- `Presentation/LoginPage.xaml.cs`
- `ViewModels/LoginViewModel.cs`
- `Services/AuthService.cs`

## 3. 功能清單
- 帳號/Email 與密碼欄位
- `ObservableValidator` + DataAnnotations 驗證
- 登入中狀態 (`IsLoading`) 與錯誤顯示 (`HasError` / `ErrorMessage`)
- 第三方登入命令（Google/Microsoft/Apple）
- 忘記密碼、註冊命令占位
- Enter 快捷鍵提交

## 4. 使用方式
1. 在 View 綁定 `LoginCommand` 與其他第三方命令。
2. 在 DI 註冊 `IAuthService`（目前預設可用 `MockAuthService`）。
3. 成功登入後由 ViewModel 導航到 `MainViewModel`。

## 5. 擴充建議
- 將 `MockAuthService` 替換成真實 API/OIDC 流程。
- 增加 token refresh 與 remember me 的安全儲存。
- 將錯誤碼映射到本地化資源，不直接顯示原始字串。

## 6. 注意事項
- `ForgotPasswordViewModel`、`RegisterViewModel` 目前是占位模板。
- WASM/Trimming 情境會出現 `ObservableValidator` 相關警告，需依部署策略評估。
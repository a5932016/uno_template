# LocalizationDemoPage View README

## 目的
多語系示範頁：切語言、格式化日期/數字/貨幣、複數字串。

## 對應檔案
- `Presentation/LocalizationDemoPage.xaml`
- `Presentation/LocalizationDemoPage.xaml.cs`
- `Strings/*/Resources.resw`

## 核心行為
- 透過 `ApplicationLanguages.PrimaryLanguageOverride` 切語言
- 透過 `IStringLocalizer` 載入多語系字串
- 用 `CultureInfo.CurrentUICulture` 做格式化輸出

## 給 Golang 後端工程師的修改建議
- 後端若回傳可本地化字串，建議回傳 key + args，不要回傳硬字串。
- 前後端要共用錯誤碼與文案 key 命名規則。

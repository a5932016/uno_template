# MainPage View README

## 目的
最小 MVVM 導航範例：輸入文字 → 傳資料到下一頁。

## 對應檔案
- `Presentation/MainPage.xaml`
- `Presentation/MainViewModel.cs`
- 搭配 `SecondPage / SecondViewModel`

## 核心流程
1. 使用者輸入 `Name`
2. 觸發 `GoToSecond`
3. `NavigateViewModelAsync<SecondViewModel>(data: new Entity(Name))`
4. `SecondPage` 顯示傳入資料

## 依賴
- `INavigator`
- `IStringLocalizer`
- `IOptions<AppConfig>`

## 給 Golang 後端工程師的修改建議
- 這頁是「request/response」心智模型：`Name` 是 request，`SecondPage` 是 response render。
- 若要接後端 API，建議在 ViewModel 增加 `IMainService` 呼叫，不改 XAML。

## 快速上手
1. 新增欄位到 `Entity`
2. MainViewModel 改傳新欄位
3. SecondPage 綁定顯示新欄位

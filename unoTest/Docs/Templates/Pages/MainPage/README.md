# MainPage Template

## 1. Template 目的
MainPage 是最小可運作的頁面模板，用於示範 Uno Navigation + MVVM 的基本流程：
- 輸入資料
- 透過 Command 觸發導航
- 帶資料導向下一頁

## 2. 檔案組成
- `Presentation/MainPage.xaml`
- `Presentation/MainPage.xaml.cs`
- `Presentation/MainViewModel.cs`
- `Presentation/SecondPage.xaml`
- `Presentation/SecondViewModel.cs`

## 3. 功能清單
- TextBox 雙向綁定 `Name`
- Command 綁定 `GoToSecond`
- 導航時傳遞 `Entity` 資料
- 展示 `IStringLocalizer` 與 `IOptions<AppConfig>` 的注入

## 4. 使用方式
1. 在 `MainPage` 輸入名稱。
2. 點擊 `Go to Second Page`。
3. `MainViewModel.GoToSecondView()` 呼叫 `NavigateViewModelAsync<SecondViewModel>`。
4. `SecondPage` 顯示傳入的 `Entity.Name`。

## 5. 擴充建議
- 將輸入驗證抽到 `ObservableValidator`。
- 將導航參數改成專用 DTO，避免後續欄位擴充造成破壞。
- 將頁面標題改為由資源檔提供。

## 6. 注意事項
- `Name!` 目前使用 null-forgiving，若要上線建議先做非空驗證。
- 這個模板偏教學用途，不包含錯誤處理與 loading 狀態。
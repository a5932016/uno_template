# TabDemoPage View README

## 目的
集中示範多種 Tab/導航元件（TabView, NavigationView, TabBar）。

## 對應檔案
- `Presentation/TabDemoPage.xaml`
- `Presentation/TabDemoPage.xaml.cs`

## 目前實作特性
- 以 code-behind 事件為主
- 適合 demo，不適合直接當正式業務頁

## 給 Golang 後端工程師的修改建議
- 正式功能請改成 ViewModel 驅動（items + selected state），不要長期依賴 code-behind。
- 如果每個 tab 對應後端查詢，建議做 caching + lazy loading。

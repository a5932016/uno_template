# SecondPage View README

## 目的
接收上一頁傳入資料並呈現。

## 對應檔案
- `Presentation/SecondPage.xaml`
- `Presentation/SecondViewModel.cs`（`record SecondViewModel(Entity Entity)`）

## 主要資料流
- 入口：`MainViewModel` 導航時傳入 `Entity`
- 顯示：XAML 綁定 `Entity.Name`

## 給 Golang 後端工程師的修改建議
- 這頁相當於 DTO 展示頁。
- 若要查更多資料，建議用 `Entity.Id` 再呼叫 API，不要把完整大物件全塞在導航參數。

## 快速上手
1. 在 `Entity` 加欄位
2. MainPage 傳新欄位
3. SecondPage 綁定新欄位

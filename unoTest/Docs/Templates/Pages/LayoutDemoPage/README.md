# LayoutDemoPage View README

## 目的
純版面教學頁，示範 Grid/StackPanel/響應式切換。

## 對應檔案
- `Presentation/LayoutDemoPage.xaml`
- `Presentation/LayoutDemoPage.xaml.cs`

## 適用情境
- 你要快速切一個新頁面版型
- 你要驗證不同裝置寬度下 UI 變化

## 給 Golang 後端工程師的修改建議
- 把這頁當「UI scaffold」，不要塞業務。
- 先定好 ViewModel 資料結構，再回填 Binding，避免 XAML 先寫死。

## 快速上手
1. 先複製這頁骨架
2. 把示範區塊替換成真實區塊
3. 補齊 ViewModel 與命令

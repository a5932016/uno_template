# ImageAnnotationEditorControl View README

## 目的
提供可重用的圖片標註控制項，供任何頁面嵌入使用。

## 對應檔案
- `Controls/ImageAnnotationEditorControl.xaml`
- `Controls/ImageAnnotationEditorControl.xaml.cs`
- `ViewModels/ImageAnnotationEditorViewModel.cs`

## 主要能力
- 載入圖片與另存 PNG
- 工具切換：移動 / 方框 / 多邊形
- 顏色切換：多組描邊色
- 標註編輯：拖曳移動、控制點調整
- 縮放：Zoom In / Zoom Out

## 重用方式
- 在任意 Page XAML 引入 `xmlns:controls="using:unoTest.Controls"`
- 直接放置 `<controls:ImageAnnotationEditorControl />`
- 容器頁不需要再實作標註邏輯

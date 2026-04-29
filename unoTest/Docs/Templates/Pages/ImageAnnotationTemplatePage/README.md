# ImageAnnotationTemplatePage View README

## 目的
提供一個可直接重用的圖片標註模板頁，支援載入圖片、拖曳移動畫面、方框描邊、多邊形描邊、選色與另存新檔。

## 對應檔案
- `Presentation/ImageAnnotationTemplatePage.xaml`
- `Presentation/ImageAnnotationTemplatePage.xaml.cs`
- `Controls/ImageAnnotationEditorControl.xaml`
- `Controls/ImageAnnotationEditorControl.xaml.cs`
- `ViewModels/ImageAnnotationEditorViewModel.cs`

## 架構調整
- `ImageAnnotationTemplatePage` 現在是容器頁，只負責導航與承載控制項
- 核心互動改為 `ImageAnnotationEditorControl`（可重用於其他頁面）
- 編輯器狀態改為 `ImageAnnotationEditorViewModel`（工具、顏色、縮放與提示文字）

## 功能清單
- 載入圖片：透過檔案挑選器匯入常見圖片格式
- 工具切換：移動圖片 / 方框線 / 多邊形線
- 顏色切換：預設七種常用描邊顏色
- 標註可編輯：已完成的方框可拖曳移動與角點調整；多邊形可拖曳移動與頂點調整
- 縮放控制：提供 Zoom In / Zoom Out，支援細節檢視
- 另存新檔：將圖片與標註合成後輸出 PNG

## 互動說明
- 移動圖片：在畫布中按住滑鼠拖曳即可平移檢視
- 方框線：按住滑鼠拖曳建立矩形線框
- 多邊形線：連續點擊建立節點，回到起點或點擊「完成多邊形」收合
- 編輯方框：點選方框後可直接拖曳移動，拉四角控制點可調整尺寸
- 編輯多邊形：點選多邊形後可拖曳移動，拉頂點控制點可調整線段
- 縮放：使用「放大 / 縮小」按鈕調整檢視比例

## 可延伸方向
- 加入縮放工具（比例控制）
- 加入文字標籤工具
- 將標註資料序列化保存（JSON）

namespace unoTest.Presentation;

/// <summary>
/// 產品詳情 ViewModel
///
/// ★ 重點：如何接收從上一頁傳來的資料
///
/// Uno Extensions Navigation 資料傳遞機制：
///   1. 上一頁呼叫：await _navigator.NavigateViewModelAsync&lt;ProductDetailViewModel&gt;(this, data: navData)
///   2. Route 需要 DataViewMap: new DataViewMap&lt;ProductDetailPage, ProductDetailViewModel, ProductNavData&gt;()
///   3. Uno Extensions 建立此 ViewModel 時，自動把 data 注入到建構子中型別匹配的參數
///   4. 本 ViewModel 的 ProductNavData navData 參數就是上一頁傳來的資料
/// </summary>
public partial class ProductDetailViewModel : ObservableObject
{
    private readonly TitleBarStateService _titleBarState;

    // ============================================================
    // 從 navData 取出的資料（對外暴露給 XAML 綁定）
    // ============================================================

    /// <summary>產品 ID（從列表頁傳來）</summary>
    public int     Id       { get; }
    public string  Name     { get; }
    public string  Category { get; }
    public decimal Price    { get; }
    public bool    IsActive { get; }

    // 顯示用格式化屬性
    public string PriceText    => $"NT$ {Price:N0}";
    public string StatusText   => IsActive ? "啟用中" : "已停用";
    public string StatusColor  => IsActive ? "#4CAF50" : "#9E9E9E";
    public string IdBadgeText  => $"#{Id:D4}";

    // ============================================================
    // Constructor：Uno Extensions 自動注入 navData + DI 服務
    // ============================================================

    /// <summary>
    /// 建構子接收兩種參數：
    /// - ProductNavData navData：從導航傳來（Uno Extensions 自動注入）
    /// - TitleBarStateService titleBarState：從 DI 容器注入
    /// </summary>
    public ProductDetailViewModel(ProductNavData navData, TitleBarStateService titleBarState)
    {
        _titleBarState = titleBarState;

        // 從 navData 取出 ID 及所有產品資料
        Id       = navData.Id;
        Name     = navData.Name;
        Category = navData.Category;
        Price    = navData.Price;
        IsActive = navData.IsActive;

        // ✅ 切換 TitleBar 為 Detail 模式
        // TitleBar 中間欄會從「Tab 列」切換為「頁面標題 + 返回按鈕」
        _titleBarState.SetDetailMode($"產品詳情：{Name}");
    }
}

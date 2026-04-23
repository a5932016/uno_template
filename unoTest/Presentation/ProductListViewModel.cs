using System.Collections.ObjectModel;

namespace unoTest.Presentation;

// ============================================================
// ProductListViewModel
// 示範：動態生成 UI 項目 + 傳遞 ID 到另一個頁面
// ============================================================

/// <summary>
/// 產品列表 ViewModel
///
/// 核心概念：
/// 1. ObservableCollection — UI 自動追蹤集合變化（新增/刪除）
/// 2. 每個 ProductListItem 擁有自己的 Command（綁定在 DataTemplate 內）
/// 3. GoToDetailAsync — 把產品 ID + 資料包成 ProductNavData 傳給詳情頁
/// 4. TitleBarStateService.SetTabsMode(1) — 告知 TitleBar 目前在「產品」Tab
/// </summary>
public partial class ProductListViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly TitleBarStateService _titleBarState;

    /// <summary>
    /// 動態產品清單
    /// ObservableCollection 讓 ListView/ItemsRepeater 自動更新 UI
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ProductListItem> _items = [];

    /// <summary>新增隨機產品的命令</summary>
    public IRelayCommand AddItemCommand { get; }

    public ProductListViewModel(INavigator navigator, TitleBarStateService titleBarState)
    {
        _navigator = navigator;
        _titleBarState = titleBarState;

        // ✅ 告知 TitleBar：目前在 Tab[1]=產品，切換為 Tabs 模式
        _titleBarState.SetTabsMode(tabIndex: 1);

        // 初始化動態資料（模擬從 Service/API 取得）
        foreach (var item in GenerateSampleProducts())
        {
            // 將 GoToDetail 動作注入每個 Item
            item.GoToDetailAction = () => _ = GoToDetailAsync(item);
            item.RemoveAction     = () => Items.Remove(item);
            Items.Add(item);
        }

        AddItemCommand = new RelayCommand(AddRandomProduct);
    }

    // ============================================================
    // ★ 核心：導航到詳情頁並傳遞 ID
    // ============================================================

    /// <summary>
    /// 點擊「查看詳情」時：把 ProductNavData（包含 ID）傳給 ProductDetailViewModel
    ///
    /// Uno Extensions Navigation 機制：
    ///   NavigateViewModelAsync&lt;ProductDetailViewModel&gt;(sender, data: navData)
    ///   → Uno Extensions 建立 ProductDetailViewModel(ProductNavData navData, ...)
    ///   → ProductDetailViewModel 的建構子接收 navData 即可取得 Id
    /// </summary>
    private async Task GoToDetailAsync(ProductListItem item)
    {
        var navData = new ProductNavData(
            Id:       item.Id,
            Name:     item.Name,
            Category: item.Category,
            Price:    item.Price,
            IsActive: item.IsActive
        );

        await _navigator.NavigateViewModelAsync<ProductDetailViewModel>(this, data: navData);
    }

    // ============================================================
    // 動態新增 / 移除（示範 ObservableCollection 即時更新 UI）
    // ============================================================

    private void AddRandomProduct()
    {
        var newId = Items.Count == 0 ? 1 : Items.Max(x => x.Id) + 1;
        var categories = new[] { "電子產品", "服飾", "食品", "家具", "書籍" };

        var item = new ProductListItem
        {
            Id       = newId,
            Name     = $"新產品 #{newId:D3}",
            Category = categories[newId % categories.Length],
            Price    = Random.Shared.Next(100, 9999),
            IsActive = true,
        };
        item.GoToDetailAction = () => _ = GoToDetailAsync(item);
        item.RemoveAction     = () => Items.Remove(item);
        Items.Add(item);
    }

    private static IEnumerable<ProductListItem> GenerateSampleProducts()
    {
        var data = new[]
        {
            (1,  "MacBook Pro 16\"",   "電子產品", 89900m, true),
            (2,  "iPhone 15 Pro",      "電子產品", 35900m, true),
            (3,  "AirPods Pro",        "電子產品",  7990m, true),
            (4,  "登山外套",           "服飾",      3990m, true),
            (5,  "運動跑鞋",           "服飾",      2490m, false),
            (6,  "有機咖啡豆 1kg",    "食品",       890m, true),
            (7,  "人體工學椅",         "家具",      12000m, true),
            (8,  "Clean Code（書）",  "書籍",        680m, true),
        };

        foreach (var (id, name, cat, price, active) in data)
        {
            yield return new ProductListItem
            {
                Id       = id,
                Name     = name,
                Category = cat,
                Price    = price,
                IsActive = active,
            };
        }
    }
}

// ============================================================
// ProductListItem — 列表項目的 ViewModel
// ============================================================

/// <summary>
/// 單一產品列表項目
///
/// ✅ 設計重點：
/// - 每個 Item 持有自己的 GoToDetailCommand 和 RemoveCommand
/// - 這樣 DataTemplate 內的按鈕可以直接 {Binding GoToDetailCommand} 而不需要 ElementName
/// - GoToDetailAction 由 ProductListViewModel 注入，保持 Item 不依賴 INavigator
/// </summary>
public partial class ProductListItem : ObservableObject
{
    public int     Id       { get; init; }
    public string  Name     { get; init; } = string.Empty;
    public string  Category { get; init; } = string.Empty;
    public decimal Price    { get; init; }

    [ObservableProperty]
    private bool _isActive;

    // 顯示用屬性
    public string IdText     => $"#{Id:D3}";
    public string PriceText  => $"NT$ {Price:N0}";
    public string StatusText => IsActive ? "✓ 啟用" : "✗ 停用";

    // ViewModel 注入的動作（不直接持有 INavigator）
    internal Action? GoToDetailAction { get; set; }
    internal Action? RemoveAction     { get; set; }

    // UI 綁定的 Commands
    public IRelayCommand GoToDetailCommand => new RelayCommand(() => GoToDetailAction?.Invoke());
    public IRelayCommand RemoveCommand     => new RelayCommand(() => RemoveAction?.Invoke());
}

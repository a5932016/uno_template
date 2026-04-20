namespace unoTest.Services;

/// <summary>
/// 產品服務介面（CRUD 操作）
/// </summary>
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> SearchAsync(string? keyword, string? category = null);
    Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, string? keyword = null, string? category = null);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteManyAsync(IEnumerable<int> ids);
}

/// <summary>
/// 分頁結果
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Mock 產品服務（開發測試用）
/// </summary>
public class MockProductService : IProductService
{
    private readonly List<Product> _products;
    private int _nextId;

    public MockProductService()
    {
        // 初始化假資料
        _products = GenerateMockData();
        _nextId = _products.Max(p => p.Id) + 1;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        await Task.Delay(300); // 模擬網路延遲
        return _products.OrderByDescending(p => p.CreatedAt).ToList();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        await Task.Delay(100);
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string? keyword, string? category = null)
    {
        await Task.Delay(200);
        
        var query = _products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(keyword) ||
                (p.Description?.ToLower().Contains(keyword) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        return query.OrderByDescending(p => p.CreatedAt).ToList();
    }

    public async Task<PagedResult<Product>> GetPagedAsync(
        int page, int pageSize, string? keyword = null, string? category = null)
    {
        await Task.Delay(300);

        var query = _products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(keyword) ||
                (p.Description?.ToLower().Contains(keyword) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var totalCount = query.Count();
        var items = query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await Task.Delay(500);

        product.Id = _nextId++;
        product.CreatedAt = DateTime.Now;
        _products.Add(product);

        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        await Task.Delay(500);

        var existing = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Product with ID {product.Id} not found");
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Category = product.Category;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.IsActive = product.IsActive;
        existing.UpdatedAt = DateTime.Now;

        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await Task.Delay(300);

        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null) return false;

        _products.Remove(product);
        return true;
    }

    public async Task<bool> DeleteManyAsync(IEnumerable<int> ids)
    {
        await Task.Delay(500);

        var idSet = ids.ToHashSet();
        var count = _products.RemoveAll(p => idSet.Contains(p.Id));
        return count > 0;
    }

    private static List<Product> GenerateMockData()
    {
        var products = new List<Product>();
        var random = new Random(42);
        var categories = ProductCategories.All;

        var names = new[]
        {
            "智慧手機", "藍牙耳機", "平板電腦", "筆記型電腦", "智慧手錶",
            "運動外套", "休閒T恤", "牛仔褲", "運動鞋", "帽子",
            "咖啡豆", "綠茶", "巧克力", "餅乾", "堅果",
            "檯燈", "收納盒", "抱枕", "地毯", "花瓶",
            "瑜伽墊", "啞鈴", "跑步機", "自行車", "露營帳篷",
            "小說", "筆記本", "鋼筆", "便條紙", "檔案夾"
        };

        for (int i = 1; i <= 50; i++)
        {
            var categoryIndex = (i - 1) / 5 % categories.Length;
            products.Add(new Product
            {
                Id = i,
                Name = $"{names[(i - 1) % names.Length]} {i:000}",
                Description = $"這是產品 {i} 的詳細描述，包含各種特性和規格說明。",
                Category = categories[categoryIndex],
                Price = random.Next(100, 10000) + random.Next(0, 99) / 100m,
                Stock = random.Next(0, 500),
                IsActive = random.Next(10) > 1, // 90% 機率啟用
                CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365))
            });
        }

        return products;
    }
}

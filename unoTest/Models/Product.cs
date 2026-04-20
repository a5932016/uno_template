namespace unoTest.Models;

/// <summary>
/// 產品實體（CRUD 示範用）
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 產品分類
/// </summary>
public static class ProductCategories
{
    public static readonly string[] All = new[]
    {
        "電子產品",
        "服飾配件",
        "食品飲料",
        "家居生活",
        "運動戶外",
        "圖書文具",
        "美妝保養",
        "其他"
    };
}

namespace unoTest.Models;

/// <summary>
/// 產品導航資料
/// 從「列表頁」傳遞到「詳情頁」的資料模型
/// 使用 record 型別以確保不可變性
/// </summary>
/// <param name="Id">產品 ID（頁面識別鍵）</param>
/// <param name="Name">產品名稱</param>
/// <param name="Category">分類</param>
/// <param name="Price">價格</param>
/// <param name="IsActive">是否啟用</param>
// ★ partial 是必要的：Uno Extensions 的 IKeyEquatable 需要 partial record
public partial record ProductNavData(int Id, string Name, string Category, decimal Price, bool IsActive);

namespace unoTest.Services;

/// <summary>
/// 節點圖資料來源抽象，便於替換為 SQLite、API 或記憶體實作。
/// </summary>
public interface INodeGraphRepository
{
    Task EnsureCreatedAsync(CancellationToken cancellationToken = default);
    Task<NodeGraphDocument?> LoadAsync(string graphKey, CancellationToken cancellationToken = default);
    Task SaveAsync(NodeGraphDocument graph, CancellationToken cancellationToken = default);
}
namespace unoTest.Models;

/// <summary>
/// 可序列化與持久化的節點圖資料。
/// </summary>
public sealed class NodeGraphDocument
{
    public string GraphKey { get; init; } = string.Empty;
    public List<NodeGraphNodeDocument> Nodes { get; init; } = new();
    public List<NodeGraphLinkDocument> Links { get; init; } = new();
}

public sealed class NodeGraphNodeDocument
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public double X { get; init; }
    public double Y { get; init; }
}

public sealed class NodeGraphLinkDocument
{
    public int Id { get; init; }
    public int FromNodeId { get; init; }
    public int ToNodeId { get; init; }
}
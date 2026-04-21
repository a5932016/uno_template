namespace unoTest.Services;

/// <summary>
/// WASM 或測試情境可用的記憶體儲存實作。
/// </summary>
public sealed class InMemoryNodeGraphRepository : INodeGraphRepository
{
    private readonly Dictionary<string, NodeGraphDocument> _storage = new(StringComparer.OrdinalIgnoreCase);

    public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<NodeGraphDocument?> LoadAsync(string graphKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphKey))
        {
            throw new ArgumentException("Graph key is required.", nameof(graphKey));
        }

        return Task.FromResult(_storage.TryGetValue(graphKey, out var document)
            ? Clone(document)
            : null);
    }

    public Task SaveAsync(NodeGraphDocument graph, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (string.IsNullOrWhiteSpace(graph.GraphKey))
        {
            throw new ArgumentException("Graph key is required.", nameof(graph));
        }

        _storage[graph.GraphKey] = Clone(graph);
        return Task.CompletedTask;
    }

    private static NodeGraphDocument Clone(NodeGraphDocument source)
    {
        return new NodeGraphDocument
        {
            GraphKey = source.GraphKey,
            Nodes = source.Nodes
                .Select(node => new NodeGraphNodeDocument
                {
                    Id = node.Id,
                    Title = node.Title,
                    X = node.X,
                    Y = node.Y
                })
                .ToList(),
            Links = source.Links
                .Select(link => new NodeGraphLinkDocument
                {
                    Id = link.Id,
                    FromNodeId = link.FromNodeId,
                    ToNodeId = link.ToNodeId
                })
                .ToList()
        };
    }
}

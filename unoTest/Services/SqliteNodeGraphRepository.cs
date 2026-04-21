namespace unoTest.Services;

/// <summary>
/// SQLite 節點圖儲存實作。
/// </summary>
public sealed class SqliteNodeGraphRepository : INodeGraphRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;

    public SqliteNodeGraphRepository(ISqliteDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS node_graph_nodes (
                graph_key TEXT NOT NULL,
                id INTEGER NOT NULL,
                title TEXT NOT NULL,
                x REAL NOT NULL,
                y REAL NOT NULL,
                PRIMARY KEY (graph_key, id)
            );

            CREATE TABLE IF NOT EXISTS node_graph_links (
                graph_key TEXT NOT NULL,
                id INTEGER NOT NULL,
                from_node_id INTEGER NOT NULL,
                to_node_id INTEGER NOT NULL,
                PRIMARY KEY (graph_key, id)
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<NodeGraphDocument?> LoadAsync(string graphKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphKey))
        {
            throw new ArgumentException("Graph key is required.", nameof(graphKey));
        }

        await EnsureCreatedAsync(cancellationToken);
        await using var connection = _connectionFactory.CreateOpenConnection();

        var nodes = new List<NodeGraphNodeDocument>();
        var nodeCommand = connection.CreateCommand();
        nodeCommand.CommandText =
            """
            SELECT id, title, x, y
            FROM node_graph_nodes
            WHERE graph_key = $graphKey
            ORDER BY id;
            """;
        nodeCommand.Parameters.AddWithValue("$graphKey", graphKey);

        await using (var reader = await nodeCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                nodes.Add(new NodeGraphNodeDocument
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    X = reader.GetDouble(2),
                    Y = reader.GetDouble(3)
                });
            }
        }

        if (nodes.Count == 0)
        {
            return null;
        }

        var links = new List<NodeGraphLinkDocument>();
        var linkCommand = connection.CreateCommand();
        linkCommand.CommandText =
            """
            SELECT id, from_node_id, to_node_id
            FROM node_graph_links
            WHERE graph_key = $graphKey
            ORDER BY id;
            """;
        linkCommand.Parameters.AddWithValue("$graphKey", graphKey);

        await using (var reader = await linkCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                links.Add(new NodeGraphLinkDocument
                {
                    Id = reader.GetInt32(0),
                    FromNodeId = reader.GetInt32(1),
                    ToNodeId = reader.GetInt32(2)
                });
            }
        }

        return new NodeGraphDocument
        {
            GraphKey = graphKey,
            Nodes = nodes,
            Links = links
        };
    }

    public async Task SaveAsync(NodeGraphDocument graph, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (string.IsNullOrWhiteSpace(graph.GraphKey))
        {
            throw new ArgumentException("Graph key is required.", nameof(graph));
        }

        await EnsureCreatedAsync(cancellationToken);
        await using var connection = _connectionFactory.CreateOpenConnection();
        using var transaction = connection.BeginTransaction();

        var deleteLinks = connection.CreateCommand();
        deleteLinks.Transaction = transaction;
        deleteLinks.CommandText =
            """
            DELETE FROM node_graph_links
            WHERE graph_key = $graphKey;
            """;
        deleteLinks.Parameters.AddWithValue("$graphKey", graph.GraphKey);
        await deleteLinks.ExecuteNonQueryAsync(cancellationToken);

        var deleteNodes = connection.CreateCommand();
        deleteNodes.Transaction = transaction;
        deleteNodes.CommandText =
            """
            DELETE FROM node_graph_nodes
            WHERE graph_key = $graphKey;
            """;
        deleteNodes.Parameters.AddWithValue("$graphKey", graph.GraphKey);
        await deleteNodes.ExecuteNonQueryAsync(cancellationToken);

        foreach (var node in graph.Nodes)
        {
            var insertNode = connection.CreateCommand();
            insertNode.Transaction = transaction;
            insertNode.CommandText =
                """
                INSERT INTO node_graph_nodes (graph_key, id, title, x, y)
                VALUES ($graphKey, $id, $title, $x, $y);
                """;
            insertNode.Parameters.AddWithValue("$graphKey", graph.GraphKey);
            insertNode.Parameters.AddWithValue("$id", node.Id);
            insertNode.Parameters.AddWithValue("$title", node.Title);
            insertNode.Parameters.AddWithValue("$x", node.X);
            insertNode.Parameters.AddWithValue("$y", node.Y);
            await insertNode.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var link in graph.Links)
        {
            var insertLink = connection.CreateCommand();
            insertLink.Transaction = transaction;
            insertLink.CommandText =
                """
                INSERT INTO node_graph_links (graph_key, id, from_node_id, to_node_id)
                VALUES ($graphKey, $id, $fromNodeId, $toNodeId);
                """;
            insertLink.Parameters.AddWithValue("$graphKey", graph.GraphKey);
            insertLink.Parameters.AddWithValue("$id", link.Id);
            insertLink.Parameters.AddWithValue("$fromNodeId", link.FromNodeId);
            insertLink.Parameters.AddWithValue("$toNodeId", link.ToNodeId);
            await insertLink.ExecuteNonQueryAsync(cancellationToken);
        }

        transaction.Commit();
    }
}

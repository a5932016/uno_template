namespace unoTest.ViewModels;

/// <summary>
/// 節點圖狀態管理，使用 NodeInfo/LinkInfo 資料模型。
/// 排序規則：Y 差距 ≤ 40px 視為同列（同列依 X 排序），否則依 Y 排序。
/// 連線規則：每次排序後重建前後鏈，不支援手動連線。
/// </summary>
public partial class NodeLinkCanvasViewModel : ObservableObject
{
    private const double YTolerance = 40;

    private readonly List<NodeInfo> _nodes = new();
    private readonly List<LinkInfo> _links = new();
    private int _nodeCounter;

    [ObservableProperty]
    private string _statusText = "拖曳節點移動位置，點擊 ＋ 可新增節點";

    [ObservableProperty]
    private NodeInfo? _selectedNode;

    [ObservableProperty]
    private LinkInfo? _selectedLink;

    /// <summary>任何節點圖狀態改變（新增、移動、排序、選取）時觸發，控制項訂閱後重繪。</summary>
    public event EventHandler? GraphChanged;

    public event EventHandler<NodeInfo>? NodeAdded;
    public event EventHandler<NodeInfo>? NodeRemoved;
    public event EventHandler<NodeInfo?>? NodeSelected;
    public event EventHandler<LinkInfo>? LinkRemoved;

    public IRelayCommand AddNodeCommand { get; }
    public IRelayCommand DeleteSelectionCommand { get; }
    public IRelayCommand ClearCommand { get; }
    public IRelayCommand AutoLayoutCommand { get; }

    public NodeLinkCanvasViewModel()
    {
        AddNodeCommand = new RelayCommand(AddNodeFromToolbar);
        DeleteSelectionCommand = new RelayCommand(DeleteSelection, CanDeleteSelection);
        ClearCommand = new RelayCommand(Clear);
        AutoLayoutCommand = new RelayCommand(() => AutoLayout());
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>新增節點，自動排序並重建前後鏈。</summary>
    public NodeInfo AddNode(string title, double x, double y)
    {
        var node = new NodeInfo
        {
            Id = ++_nodeCounter,
            Title = title,
            X = x,
            Y = y,
            ButtonInfo = new ButtonInfo(),
            TextInfo = new TextInfo { Text = title },
            ImageInfo = new ImageInfo()
        };

        _nodes.Add(node);
        SortNodes();
        RebuildLinks();

        NodeAdded?.Invoke(this, node);
        GraphChanged?.Invoke(this, EventArgs.Empty);

        return node;
    }

    /// <summary>拖曳中即時更新座標（由控制項直接呼叫，不觸發 GraphChanged）。</summary>
    public void MoveNode(NodeInfo node, double x, double y)
    {
        node.X = x;
        node.Y = y;
    }

    /// <summary>拖曳結束後呼叫，觸發排序、重建連線並通知控制項重繪。</summary>
    public void EndDrag()
    {
        SortNodes();
        RebuildLinks();
        GraphChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveNode(NodeInfo node)
    {
        var relatedLinks = _links.Where(l => l.FromNode == node || l.ToNode == node).ToList();
        foreach (var link in relatedLinks)
        {
            _links.Remove(link);
            LinkRemoved?.Invoke(this, link);
        }

        _nodes.Remove(node);
        if (_selectedNode == node) SelectedNode = null;

        SortNodes();
        RebuildLinks();

        NodeRemoved?.Invoke(this, node);
        GraphChanged?.Invoke(this, EventArgs.Empty);
        StatusText = "已刪除節點";
    }

    public void RemoveLink(LinkInfo link)
    {
        _links.Remove(link);
        if (_selectedLink == link) SelectedLink = null;

        // 鏈結模式下刪除連線等同移除一個節點間的連結；重建即可
        RebuildLinks();

        LinkRemoved?.Invoke(this, link);
        GraphChanged?.Invoke(this, EventArgs.Empty);
        StatusText = "已刪除連線";
    }

    public void Clear()
    {
        _nodes.Clear();
        _links.Clear();
        _nodeCounter = 0;
        SelectedNode = null;
        SelectedLink = null;
        GraphChanged?.Invoke(this, EventArgs.Empty);
        StatusText = "已清除所有節點和連線";
    }

    public void AutoLayout(int columns = 4, double spacing = 160, double startX = 100, double startY = 100)
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            _nodes[i].X = startX + (i % columns) * spacing;
            _nodes[i].Y = startY + (i / columns) * spacing;
        }

        SortNodes();
        RebuildLinks();
        GraphChanged?.Invoke(this, EventArgs.Empty);
        StatusText = "已自動排列節點";
    }

    public void SelectNode(NodeInfo? node)
    {
        SelectedNode = node;
        SelectedLink = null;
        NodeSelected?.Invoke(this, node);
    }

    public void SelectLink(LinkInfo? link)
    {
        SelectedLink = link;
        SelectedNode = null;
    }

    public void DeleteSelection()
    {
        if (SelectedLink is not null)
            RemoveLink(SelectedLink);
        else if (SelectedNode is not null)
            RemoveNode(SelectedNode);
    }

    public IReadOnlyList<NodeInfo> GetNodes() => _nodes.AsReadOnly();
    public IReadOnlyList<LinkInfo> GetLinks() => _links.AsReadOnly();

    public NodeInfo? FindNodeById(int id) => _nodes.FirstOrDefault(n => n.Id == id);

    public NodeGraphDocument ToDocument(string graphKey) => new()
    {
        GraphKey = graphKey,
        Nodes = _nodes.Select(n => new NodeGraphNodeDocument
        {
            Id = n.Id,
            Title = n.Title,
            X = n.X,
            Y = n.Y
        }).ToList(),
        Links = _links.Select((l, i) => new NodeGraphLinkDocument
        {
            Id = i + 1,
            FromNodeId = l.FromNode.Id,
            ToNodeId = l.ToNode.Id
        }).ToList()
    };

    public void LoadDocument(NodeGraphDocument? graph)
    {
        Clear();

        if (graph is null)
        {
            StatusText = "目前沒有已儲存的節點圖";
            return;
        }

        foreach (var n in graph.Nodes.OrderBy(n => n.Id))
        {
            _nodeCounter = Math.Max(_nodeCounter, n.Id);
            _nodes.Add(new NodeInfo
            {
                Id = n.Id,
                Title = n.Title,
                X = n.X,
                Y = n.Y,
                ButtonInfo = new ButtonInfo(),
                TextInfo = new TextInfo { Text = n.Title },
                ImageInfo = new ImageInfo()
            });
        }

        // 連線一律由排序後重建（忽略文件中的 Links 記錄）
        SortNodes();
        RebuildLinks();
        GraphChanged?.Invoke(this, EventArgs.Empty);

        StatusText = _nodes.Count == 0
            ? "目前沒有節點，請先新增節點"
            : $"已載入 {_nodes.Count} 個節點與 {_links.Count} 條連線";
    }

    public void SetStatus(string message) => StatusText = message;

    // ── Private helpers ────────────────────────────────────────────────────

    private void SortNodes()
    {
        _nodes.Sort((a, b) =>
        {
            bool sameYBand = Math.Abs(a.Y - b.Y) <= YTolerance;
            return sameYBand ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y);
        });
    }

    private void RebuildLinks()
    {
        _links.Clear();
        for (int i = 0; i < _nodes.Count - 1; i++)
            _links.Add(new LinkInfo { FromNode = _nodes[i], ToNode = _nodes[i + 1] });
    }

    private void AddNodeFromToolbar()
    {
        double x = 100 + (_nodes.Count % 5) * 150;
        double y = 100 + (_nodes.Count / 5) * 100;

        if (_nodes.Count > 0)
        {
            x = _nodes[^1].X + 100;
            y = _nodes[^1].Y;
        }

        AddNode($"節點 {_nodeCounter + 1}", x, y);
    }

    private bool CanDeleteSelection() => SelectedNode is not null || SelectedLink is not null;

    partial void OnSelectedNodeChanged(NodeInfo? value) => DeleteSelectionCommand.NotifyCanExecuteChanged();
    partial void OnSelectedLinkChanged(LinkInfo? value) => DeleteSelectionCommand.NotifyCanExecuteChanged();
}

// ── Data Models ────────────────────────────────────────────────────────────

/// <summary>節點資料，不持有 UI 元素參考。</summary>
public class NodeInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public object? Data { get; set; }
    public double Width { get; set; } = 80;
    public double Height { get; set; } = 80;
    public ButtonInfo ButtonInfo { get; set; } = new();
    public TextInfo TextInfo { get; set; } = new();
    public ImageInfo ImageInfo { get; set; } = new();
}

/// <summary>連線資料，以 NodeInfo 物件參考代替 ID。</summary>
public class LinkInfo
{
    public NodeInfo FromNode { get; set; } = null!;
    public NodeInfo ToNode { get; set; } = null!;
    public object? Data { get; set; }
}

/// <summary>節點按鈕外觀設定。</summary>
public class ButtonInfo
{
    public double Width { get; set; } = 50;
    public double Height { get; set; } = 50;
}

/// <summary>節點標題文字設定。</summary>
public class TextInfo
{
    public string Text { get; set; } = string.Empty;
}

/// <summary>節點圖示設定（支援 ms-appx:/// 路徑）。</summary>
public class ImageInfo
{
    public string Source { get; set; } = "ms-appx:///Assets/Icons/icon_foreground.png";
    public double Width { get; set; } = 32;
    public double Height { get; set; } = 32;
}

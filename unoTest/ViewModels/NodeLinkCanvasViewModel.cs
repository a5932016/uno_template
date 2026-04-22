using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace unoTest.ViewModels;

/// <summary>
/// 負責節點圖的狀態與業務規則，不包含任何 UI 控制項邏輯。
/// 節點依 Y（容差 40px）再依 X 自動排序，並在排序後重建前後鏈連線。
/// </summary>
public partial class NodeLinkCanvasViewModel : ObservableObject
{
    private const double YTolerance = 40;

    private readonly ObservableCollection<NodeLinkNodeViewModel> _nodes = new();
    private readonly ObservableCollection<NodeLinkLinkViewModel> _links = new();

    private int _nextNodeId = 1;
    private int _nextLinkId = 1;

    public ReadOnlyObservableCollection<NodeLinkNodeViewModel> Nodes { get; }
    public ReadOnlyObservableCollection<NodeLinkLinkViewModel> Links { get; }

    public event EventHandler? GraphChanged;

    [ObservableProperty]
    private string _statusText = "拖曳節點移動位置，點擊 ＋ 可新增節點";

    [ObservableProperty]
    private int? _selectedNodeId;

    [ObservableProperty]
    private int? _selectedLinkId;

    public IRelayCommand AddNodeCommand { get; }
    public IRelayCommand DeleteSelectionCommand { get; }
    public IRelayCommand ClearCommand { get; }
    public IRelayCommand AutoLayoutCommand { get; }

    public NodeLinkCanvasViewModel()
    {
        Nodes = new ReadOnlyObservableCollection<NodeLinkNodeViewModel>(_nodes);
        Links = new ReadOnlyObservableCollection<NodeLinkLinkViewModel>(_links);

        AddNodeCommand = new RelayCommand(AddNodeFromToolbar);
        DeleteSelectionCommand = new RelayCommand(DeleteSelection, CanDeleteSelection);
        ClearCommand = new RelayCommand(Clear);
        AutoLayoutCommand = new RelayCommand(() => AutoLayout());

        _nodes.CollectionChanged += NodesOnCollectionChanged;
        _links.CollectionChanged += LinksOnCollectionChanged;
    }

    public NodeLinkNodeViewModel? FindNode(int nodeId)
        => _nodes.FirstOrDefault(node => node.Id == nodeId);

    /// <summary>新增節點，完成後自動排序並重建前後鏈。</summary>
    public NodeLinkNodeViewModel AddNode(string title, double x, double y, int? nodeId = null,
        ButtonInfo? buttonInfo = null, TextInfo? textInfo = null, ImageInfo? imageInfo = null)
    {
        var node = CreateNode(title, x, y, nodeId, buttonInfo, textInfo, imageInfo);
        _nodes.Add(node);
        SelectNode(node.Id);
        SortAndRebuildLinks();
        return node;
    }

    /// <summary>更新節點座標（拖曳中即時呼叫）。</summary>
    public void MoveNode(int nodeId, double x, double y)
    {
        var node = FindNode(nodeId);
        if (node is null) return;

        node.X = x;
        node.Y = y;
    }

    /// <summary>拖曳結束後呼叫，觸發排序與重建連線。</summary>
    public void EndDrag()
    {
        SortAndRebuildLinks();
    }

    public void SelectNode(int? nodeId)
    {
        SelectedNodeId = nodeId;
        SelectedLinkId = null;
    }

    public void SelectLink(int? linkId)
    {
        SelectedLinkId = linkId;
        SelectedNodeId = null;
    }

    public void DeleteSelection()
    {
        if (SelectedLinkId is int selectedLinkId)
        {
            var link = _links.FirstOrDefault(item => item.Id == selectedLinkId);
            if (link is not null)
                _links.Remove(link);

            StatusText = "已刪除連線";
            SelectedLinkId = null;
            SortAndRebuildLinks();
            return;
        }

        if (SelectedNodeId is not int selectedNodeId) return;

        var node = FindNode(selectedNodeId);
        if (node is null) return;

        var relatedLinks = _links
            .Where(l => l.FromNodeId == selectedNodeId || l.ToNodeId == selectedNodeId)
            .ToList();
        foreach (var l in relatedLinks)
            _links.Remove(l);

        _nodes.Remove(node);
        SelectedNodeId = null;
        StatusText = "已刪除節點";
        SortAndRebuildLinks();
    }

    public void Clear()
    {
        _nodes.Clear();
        _links.Clear();
        _nextNodeId = 1;
        _nextLinkId = 1;
        SelectedNodeId = null;
        SelectedLinkId = null;
        StatusText = "已清除所有節點和連線";
    }

    public void AutoLayout(int columns = 4, double spacing = 160, double startX = 100, double startY = 100)
    {
        for (var i = 0; i < _nodes.Count; i++)
        {
            var node = _nodes[i];
            node.X = startX + (i % columns) * spacing;
            node.Y = startY + (i / columns) * spacing;
        }

        SortAndRebuildLinks();
        StatusText = "已自動排列節點";
    }

    public NodeGraphDocument ToDocument(string graphKey)
    {
        return new NodeGraphDocument
        {
            GraphKey = graphKey,
            Nodes = _nodes.Select(n => new NodeGraphNodeDocument
            {
                Id = n.Id,
                Title = n.Title,
                X = n.X,
                Y = n.Y
            }).ToList(),
            Links = _links.Select(l => new NodeGraphLinkDocument
            {
                Id = l.Id,
                FromNodeId = l.FromNodeId,
                ToNodeId = l.ToNodeId
            }).ToList()
        };
    }

    public void LoadDocument(NodeGraphDocument? graph)
    {
        Clear();

        if (graph is null)
        {
            StatusText = "目前沒有已儲存的節點圖";
            return;
        }

        // 批量載入：暫停中間的排序重建，最後做一次
        foreach (var n in graph.Nodes.OrderBy(n => n.Id))
            AddNodeSilent(n.Title, n.X, n.Y, n.Id);

        SortAndRebuildLinks();

        SelectedNodeId = null;
        SelectedLinkId = null;

        StatusText = _nodes.Count == 0
            ? "目前沒有節點，請先新增節點"
            : $"已載入 {_nodes.Count} 個節點與 {_links.Count} 條連線";
    }

    public void SetStatus(string message) => StatusText = message;

    // ── Private helpers ────────────────────────────────────────────────────

    private NodeLinkNodeViewModel CreateNode(string title, double x, double y, int? nodeId,
        ButtonInfo? buttonInfo, TextInfo? textInfo, ImageInfo? imageInfo)
    {
        var resolvedId = nodeId ?? _nextNodeId++;
        _nextNodeId = Math.Max(_nextNodeId, resolvedId + 1);

        return new NodeLinkNodeViewModel(
            resolvedId, title, x, y,
            buttonInfo ?? new ButtonInfo(),
            textInfo ?? new TextInfo { Text = title },
            imageInfo ?? new ImageInfo());
    }

    /// <summary>不觸發 SortAndRebuildLinks 的靜默新增，批量載入時使用。</summary>
    private void AddNodeSilent(string title, double x, double y, int? nodeId = null,
        ButtonInfo? buttonInfo = null, TextInfo? textInfo = null, ImageInfo? imageInfo = null)
    {
        var node = CreateNode(title, x, y, nodeId, buttonInfo, textInfo, imageInfo);
        _nodes.Add(node);
    }

    /// <summary>依 Y（含容差）再依 X 排序節點，然後重建前後鏈連線。</summary>
    private void SortAndRebuildLinks()
    {
        SortNodes();
        RebuildLinks();
    }

    private void SortNodes()
    {
        var sorted = _nodes.ToList();
        sorted.Sort((a, b) =>
        {
            bool sameRow = Math.Abs(a.Y - b.Y) <= YTolerance;
            return sameRow ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y);
        });

        for (int i = 0; i < sorted.Count; i++)
        {
            int current = _nodes.IndexOf(sorted[i]);
            if (current != i)
                _nodes.Move(current, i);
        }
    }

    private void RebuildLinks()
    {
        // 清除舊連線
        for (int i = _links.Count - 1; i >= 0; i--)
            _links.RemoveAt(i);

        _nextLinkId = 1;

        // 建立前後鏈
        for (int i = 0; i < _nodes.Count - 1; i++)
            _links.Add(new NodeLinkLinkViewModel(_nextLinkId++, _nodes[i].Id, _nodes[i + 1].Id));
    }

    private void AddNodeFromToolbar()
    {
        double x = 100 + (_nodes.Count % 5) * 150;
        double y = 100 + (_nodes.Count / 5) * 100;

        if (_nodes.Count > 0)
        {
            x = _nodes[_nodes.Count - 1].X + 100;
            y = _nodes[_nodes.Count - 1].Y;
        }

        AddNode($"節點 {_nextNodeId}", x, y);
    }

    private bool CanDeleteSelection()
        => SelectedNodeId.HasValue || SelectedLinkId.HasValue;

    private void NodesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (var item in e.OldItems.OfType<NodeLinkNodeViewModel>())
                item.PropertyChanged -= NodeOnPropertyChanged;

        if (e.NewItems is not null)
            foreach (var item in e.NewItems.OfType<NodeLinkNodeViewModel>())
                item.PropertyChanged += NodeOnPropertyChanged;

        GraphChanged?.Invoke(this, EventArgs.Empty);
    }

    private void LinksOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        GraphChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NodeLinkNodeViewModel.X)
            or nameof(NodeLinkNodeViewModel.Y)
            or nameof(NodeLinkNodeViewModel.Title))
        {
            GraphChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    partial void OnSelectedNodeIdChanged(int? value)
    {
        DeleteSelectionCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedLinkIdChanged(int? value)
    {
        DeleteSelectionCommand.NotifyCanExecuteChanged();
    }
}

// ── Sub-ViewModels ────────────────────────────────────────────────────────

public partial class NodeLinkNodeViewModel : ObservableObject
{
    public int Id { get; }
    public ButtonInfo ButtonInfo { get; }
    public TextInfo TextInfo { get; }
    public ImageInfo ImageInfo { get; }

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    public NodeLinkNodeViewModel(int id, string title, double x, double y,
        ButtonInfo buttonInfo, TextInfo textInfo, ImageInfo imageInfo)
    {
        Id = id;
        _title = title;
        _x = x;
        _y = y;
        ButtonInfo = buttonInfo;
        TextInfo = textInfo;
        ImageInfo = imageInfo;
    }
}

public sealed class NodeLinkLinkViewModel
{
    public int Id { get; }
    public int FromNodeId { get; }
    public int ToNodeId { get; }

    public NodeLinkLinkViewModel(int id, int fromNodeId, int toNodeId)
    {
        Id = id;
        FromNodeId = fromNodeId;
        ToNodeId = toNodeId;
    }
}

// ── Node info classes ─────────────────────────────────────────────────────

/// <summary>節點圓形按鈕的外觀設定。</summary>
public sealed class ButtonInfo
{
    public double Width { get; set; } = 50;
    public double Height { get; set; } = 50;
}

/// <summary>節點標題文字設定。</summary>
public sealed class TextInfo
{
    public string Text { get; set; } = string.Empty;
}

/// <summary>節點圖示來源設定（支援 ms-appx:/// 路徑）。</summary>
public sealed class ImageInfo
{
    public string Source { get; set; } = "ms-appx:///Assets/Icons/icon_foreground.svg";
    public double Width { get; set; } = 32;
    public double Height { get; set; } = 32;
}

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace unoTest.ViewModels;

/// <summary>
/// 負責節點圖的狀態與業務規則，不包含任何 UI 控制項邏輯。
/// </summary>
public partial class NodeLinkCanvasViewModel : ObservableObject
{
    private readonly ObservableCollection<NodeLinkNodeViewModel> _nodes = new();
    private readonly ObservableCollection<NodeLinkLinkViewModel> _links = new();

    private int _nextNodeId = 1;
    private int _nextLinkId = 1;
    private int? _linkStartNodeId;

    public ReadOnlyObservableCollection<NodeLinkNodeViewModel> Nodes { get; }
    public ReadOnlyObservableCollection<NodeLinkLinkViewModel> Links { get; }

    public event EventHandler? GraphChanged;

    [ObservableProperty]
    private string _statusText = "拖曳節點移動位置，點擊「新增連線」後依序選擇起點和終點";

    [ObservableProperty]
    private int? _selectedNodeId;

    [ObservableProperty]
    private int? _selectedLinkId;

    [ObservableProperty]
    private bool _isLinkMode;

    public IRelayCommand AddNodeCommand { get; }
    public IRelayCommand StartAddLinkCommand { get; }
    public IRelayCommand DeleteSelectionCommand { get; }
    public IRelayCommand ClearCommand { get; }
    public IRelayCommand AutoLayoutCommand { get; }

    public NodeLinkCanvasViewModel()
    {
        Nodes = new ReadOnlyObservableCollection<NodeLinkNodeViewModel>(_nodes);
        Links = new ReadOnlyObservableCollection<NodeLinkLinkViewModel>(_links);

        AddNodeCommand = new RelayCommand(AddNodeFromToolbar);
        StartAddLinkCommand = new RelayCommand(StartLinkMode);
        DeleteSelectionCommand = new RelayCommand(DeleteSelection, CanDeleteSelection);
        ClearCommand = new RelayCommand(Clear);
        AutoLayoutCommand = new RelayCommand(() => AutoLayout());

        _nodes.CollectionChanged += NodesOnCollectionChanged;
        _links.CollectionChanged += LinksOnCollectionChanged;
    }

    public NodeLinkNodeViewModel? FindNode(int nodeId)
        => _nodes.FirstOrDefault(node => node.Id == nodeId);

    public NodeLinkNodeViewModel AddNode(string title, double x, double y, int? nodeId = null)
    {
        var resolvedId = nodeId ?? _nextNodeId++;
        _nextNodeId = Math.Max(_nextNodeId, resolvedId + 1);

        var node = new NodeLinkNodeViewModel(resolvedId, title, x, y);
        _nodes.Add(node);
        SelectNode(resolvedId);

        return node;
    }

    public bool TryAddLink(int fromNodeId, int toNodeId, int? linkId = null)
    {
        if (fromNodeId == toNodeId)
        {
            return false;
        }

        if (FindNode(fromNodeId) is null || FindNode(toNodeId) is null)
        {
            return false;
        }

        if (_links.Any(link => link.FromNodeId == fromNodeId && link.ToNodeId == toNodeId))
        {
            return false;
        }

        var resolvedId = linkId ?? _nextLinkId++;
        _nextLinkId = Math.Max(_nextLinkId, resolvedId + 1);

        _links.Add(new NodeLinkLinkViewModel(resolvedId, fromNodeId, toNodeId));
        SelectLink(resolvedId);

        return true;
    }

    public void HandleNodePressed(int nodeId)
    {
        if (!IsLinkMode)
        {
            SelectNode(nodeId);
            return;
        }

        if (_linkStartNodeId is null)
        {
            _linkStartNodeId = nodeId;
            SelectNode(nodeId);
            StatusText = $"已選擇起點「{FindNode(nodeId)?.Title}」，請點擊終點節點";
            return;
        }

        if (_linkStartNodeId == nodeId)
        {
            StatusText = "起點與終點不能是同一個節點";
            _linkStartNodeId = null;
            IsLinkMode = false;
            return;
        }

        var isCreated = TryAddLink(_linkStartNodeId.Value, nodeId);
        StatusText = isCreated ? "連線完成" : "連線已存在或節點不存在";

        _linkStartNodeId = null;
        IsLinkMode = false;
    }

    public void MoveNode(int nodeId, double x, double y)
    {
        var node = FindNode(nodeId);
        if (node is null)
        {
            return;
        }

        node.X = x;
        node.Y = y;
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
            {
                _links.Remove(link);
            }

            StatusText = "已刪除連線";
            SelectedLinkId = null;
            return;
        }

        if (SelectedNodeId is not int selectedNodeId)
        {
            return;
        }

        var node = FindNode(selectedNodeId);
        if (node is null)
        {
            return;
        }

        var relatedLinks = _links.Where(link => link.FromNodeId == selectedNodeId || link.ToNodeId == selectedNodeId).ToList();
        foreach (var link in relatedLinks)
        {
            _links.Remove(link);
        }

        _nodes.Remove(node);
        SelectedNodeId = null;
        StatusText = "已刪除節點";
    }

    public void Clear()
    {
        _nodes.Clear();
        _links.Clear();
        _nextNodeId = 1;
        _nextLinkId = 1;
        _linkStartNodeId = null;
        SelectedNodeId = null;
        SelectedLinkId = null;
        IsLinkMode = false;
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

        StatusText = "已自動排列節點";
    }

    public NodeGraphDocument ToDocument(string graphKey)
    {
        return new NodeGraphDocument
        {
            GraphKey = graphKey,
            Nodes = _nodes
                .Select(node => new NodeGraphNodeDocument
                {
                    Id = node.Id,
                    Title = node.Title,
                    X = node.X,
                    Y = node.Y
                })
                .ToList(),
            Links = _links
                .Select(link => new NodeGraphLinkDocument
                {
                    Id = link.Id,
                    FromNodeId = link.FromNodeId,
                    ToNodeId = link.ToNodeId
                })
                .ToList()
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

        foreach (var node in graph.Nodes.OrderBy(node => node.Id))
        {
            AddNode(node.Title, node.X, node.Y, node.Id);
        }

        foreach (var link in graph.Links.OrderBy(link => link.Id))
        {
            TryAddLink(link.FromNodeId, link.ToNodeId, link.Id);
        }

        SelectedNodeId = null;
        SelectedLinkId = null;

        StatusText = _nodes.Count == 0
            ? "目前沒有節點，請先新增節點"
            : $"已載入 {_nodes.Count} 個節點與 {_links.Count} 條連線";
    }

    public void SetStatus(string message)
    {
        StatusText = message;
    }

    private void AddNodeFromToolbar()
    {
        var x = 100 + (_nodes.Count % 5) * 150;
        var y = 100 + (_nodes.Count / 5) * 100;
        AddNode($"節點 {_nextNodeId}", x, y);
    }

    private void StartLinkMode()
    {
        IsLinkMode = true;
        _linkStartNodeId = null;
        StatusText = "連線模式：請點擊起點節點";
    }

    private bool CanDeleteSelection()
        => SelectedNodeId.HasValue || SelectedLinkId.HasValue;

    private void NodesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var oldItem in e.OldItems.OfType<NodeLinkNodeViewModel>())
            {
                oldItem.PropertyChanged -= NodeOnPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var newItem in e.NewItems.OfType<NodeLinkNodeViewModel>())
            {
                newItem.PropertyChanged += NodeOnPropertyChanged;
            }
        }

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

    partial void OnIsLinkModeChanged(bool value)
    {
        if (!value)
        {
            _linkStartNodeId = null;
        }
    }
}

public partial class NodeLinkNodeViewModel : ObservableObject
{
    public int Id { get; }

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    public NodeLinkNodeViewModel(int id, string title, double x, double y)
    {
        Id = id;
        _title = title;
        _x = x;
        _y = y;
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

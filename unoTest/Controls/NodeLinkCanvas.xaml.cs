using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;

namespace unoTest.Controls;

/// <summary>
/// 節點連線畫布控件
/// 支援拖曳節點、連接節點的互動式畫布
/// </summary>
public sealed partial class NodeLinkCanvas : UserControl
{
    #region Private Fields

    private readonly List<NodeInfo> _nodes = new();
    private readonly List<LinkInfo> _links = new();
    private NodeInfo? _selectedNode;
    private LinkInfo? _selectedLink;
    private bool _isLinkMode;
    private NodeInfo? _linkStartNode;
    private int _nodeCounter;

    // 拖曳相關
    private bool _isDragging;
    private Point _dragStartPoint;
    private Point _nodeStartPosition;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(NodeLinkCanvas),
            new PropertyMetadata(true, OnShowGridChanged));

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public static readonly DependencyProperty NodeMinWidthProperty =
        DependencyProperty.Register(nameof(NodeMinWidth), typeof(double), typeof(NodeLinkCanvas),
            new PropertyMetadata(100.0));

    public double NodeMinWidth
    {
        get => (double)GetValue(NodeMinWidthProperty);
        set => SetValue(NodeMinWidthProperty, value);
    }

    public static readonly DependencyProperty LinkColorProperty =
        DependencyProperty.Register(nameof(LinkColor), typeof(Color), typeof(NodeLinkCanvas),
            new PropertyMetadata(Microsoft.UI.Colors.Gray));

    public Color LinkColor
    {
        get => (Color)GetValue(LinkColorProperty);
        set => SetValue(LinkColorProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<NodeInfo>? NodeAdded;
    public event EventHandler<NodeInfo>? NodeRemoved;
    public event EventHandler<NodeInfo>? NodeSelected;
    public event EventHandler<LinkInfo>? LinkAdded;
    public event EventHandler<LinkInfo>? LinkRemoved;

    #endregion

    public NodeLinkCanvas()
    {
        this.InitializeComponent();
        this.Loaded += NodeLinkCanvas_Loaded;
    }

    private void NodeLinkCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        DrawGrid();
    }

    #region Public Methods

    /// <summary>
    /// 新增節點
    /// </summary>
    public NodeInfo AddNode(string title, double x, double y)
    {
        var node = new NodeInfo
        {
            Id = ++_nodeCounter,
            Title = title,
            X = x,
            Y = y
        };

        CreateNodeVisual(node);
        _nodes.Add(node);
        NodeAdded?.Invoke(this, node);

        return node;
    }

    /// <summary>
    /// 新增連線
    /// </summary>
    public LinkInfo? AddLink(NodeInfo fromNode, NodeInfo toNode)
    {
        // 檢查是否已存在相同連線
        if (_links.Any(l => l.FromNode == fromNode && l.ToNode == toNode))
            return null;

        var link = new LinkInfo
        {
            FromNode = fromNode,
            ToNode = toNode
        };

        CreateLinkVisual(link);
        _links.Add(link);
        LinkAdded?.Invoke(this, link);

        return link;
    }

    /// <summary>
    /// 刪除節點
    /// </summary>
    public void RemoveNode(NodeInfo node)
    {
        // 先刪除相關連線
        var relatedLinks = _links.Where(l => l.FromNode == node || l.ToNode == node).ToList();
        foreach (var link in relatedLinks)
        {
            RemoveLink(link);
        }

        // 刪除節點視覺元素
        if (node.Visual != null)
        {
            NodeCanvas.Children.Remove(node.Visual);
        }

        _nodes.Remove(node);
        NodeRemoved?.Invoke(this, node);
    }

    /// <summary>
    /// 刪除連線
    /// </summary>
    public void RemoveLink(LinkInfo link)
    {
        if (link.Visual != null)
        {
            LinkCanvas.Children.Remove(link.Visual);
        }

        _links.Remove(link);
        LinkRemoved?.Invoke(this, link);
    }

    /// <summary>
    /// 清除所有節點和連線
    /// </summary>
    public void Clear()
    {
        NodeCanvas.Children.Clear();
        LinkCanvas.Children.Clear();
        _nodes.Clear();
        _links.Clear();
        _nodeCounter = 0;
    }

    /// <summary>
    /// 取得所有節點
    /// </summary>
    public IReadOnlyList<NodeInfo> GetNodes() => _nodes.AsReadOnly();

    /// <summary>
    /// 取得所有連線
    /// </summary>
    public IReadOnlyList<LinkInfo> GetLinks() => _links.AsReadOnly();

    #endregion

    #region Private Methods

    private void CreateNodeVisual(NodeInfo node)
    {
        var button = new Button
        {
            Content = node.Title,
            MinWidth = NodeMinWidth,
            Padding = new Thickness(16, 12, 16, 12),
            Tag = node
        };

        // 設定位置
        Canvas.SetLeft(button, node.X);
        Canvas.SetTop(button, node.Y);

        // 添加事件
        button.PointerPressed += Node_PointerPressed;
        button.PointerMoved += Node_PointerMoved;
        button.PointerReleased += Node_PointerReleased;
        button.Click += Node_Click;

        node.Visual = button;
        NodeCanvas.Children.Add(button);
    }

    private void CreateLinkVisual(LinkInfo link)
    {
        var line = new Line
        {
            Stroke = new SolidColorBrush(LinkColor),
            StrokeThickness = 2,
            Tag = link
        };

        UpdateLinkPosition(link, line);
        line.PointerPressed += Link_PointerPressed;

        link.Visual = line;
        LinkCanvas.Children.Add(line);

        // 添加箭頭
        AddArrowHead(link);
    }

    private void AddArrowHead(LinkInfo link)
    {
        // 計算箭頭位置和角度
        var fromCenter = GetNodeCenter(link.FromNode);
        var toCenter = GetNodeCenter(link.ToNode);

        var angle = Math.Atan2(toCenter.Y - fromCenter.Y, toCenter.X - fromCenter.X);
        var arrowSize = 10;

        var arrowPath = new Polygon
        {
            Fill = new SolidColorBrush(LinkColor),
            Points = new PointCollection
            {
                new Point(0, 0),
                new Point(-arrowSize, -arrowSize / 2),
                new Point(-arrowSize, arrowSize / 2)
            },
            Tag = link
        };

        // 設定箭頭位置（連線終點）
        var arrowX = toCenter.X - 30 * Math.Cos(angle);
        var arrowY = toCenter.Y - 30 * Math.Sin(angle);
        Canvas.SetLeft(arrowPath, arrowX);
        Canvas.SetTop(arrowPath, arrowY);

        // 旋轉箭頭
        arrowPath.RenderTransform = new RotateTransform { Angle = angle * 180 / Math.PI };
        arrowPath.RenderTransformOrigin = new Point(0, 0.5);

        link.ArrowVisual = arrowPath;
        LinkCanvas.Children.Add(arrowPath);
    }

    private void UpdateLinkPosition(LinkInfo link, Line? line = null)
    {
        line ??= link.Visual as Line;
        if (line == null) return;

        var fromCenter = GetNodeCenter(link.FromNode);
        var toCenter = GetNodeCenter(link.ToNode);

        line.X1 = fromCenter.X;
        line.Y1 = fromCenter.Y;
        line.X2 = toCenter.X;
        line.Y2 = toCenter.Y;

        // 更新箭頭位置
        UpdateArrowPosition(link);
    }

    private void UpdateArrowPosition(LinkInfo link)
    {
        if (link.ArrowVisual is not Polygon arrow) return;

        var fromCenter = GetNodeCenter(link.FromNode);
        var toCenter = GetNodeCenter(link.ToNode);

        var angle = Math.Atan2(toCenter.Y - fromCenter.Y, toCenter.X - fromCenter.X);

        var arrowX = toCenter.X - 30 * Math.Cos(angle);
        var arrowY = toCenter.Y - 30 * Math.Sin(angle);
        Canvas.SetLeft(arrow, arrowX);
        Canvas.SetTop(arrow, arrowY);

        arrow.RenderTransform = new RotateTransform { Angle = angle * 180 / Math.PI };
    }

    private Point GetNodeCenter(NodeInfo node)
    {
        if (node.Visual is not FrameworkElement element)
            return new Point(node.X, node.Y);

        return new Point(
            node.X + element.ActualWidth / 2,
            node.Y + element.ActualHeight / 2
        );
    }

    private void UpdateAllLinks()
    {
        foreach (var link in _links)
        {
            UpdateLinkPosition(link);
        }
    }

    private void DrawGrid()
    {
        GridCanvas.Children.Clear();
        if (!ShowGrid) return;

        var gridSize = 20;
        var brush = new SolidColorBrush(Microsoft.UI.Colors.LightGray) { Opacity = 0.3 };

        // 垂直線
        for (double x = 0; x < ActualWidth; x += gridSize)
        {
            var line = new Line
            {
                X1 = x, Y1 = 0,
                X2 = x, Y2 = ActualHeight,
                Stroke = brush,
                StrokeThickness = 1
            };
            GridCanvas.Children.Add(line);
        }

        // 水平線
        for (double y = 0; y < ActualHeight; y += gridSize)
        {
            var line = new Line
            {
                X1 = 0, Y1 = y,
                X2 = ActualWidth, Y2 = y,
                Stroke = brush,
                StrokeThickness = 1
            };
            GridCanvas.Children.Add(line);
        }
    }

    private void SelectNode(NodeInfo? node)
    {
        // 取消之前的選擇
        if (_selectedNode?.Visual is Button prevButton)
        {
            prevButton.BorderBrush = null;
            prevButton.BorderThickness = new Thickness(0);
        }

        _selectedNode = node;

        // 高亮新選擇
        if (_selectedNode?.Visual is Button newButton)
        {
            newButton.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Blue);
            newButton.BorderThickness = new Thickness(2);
        }

        NodeSelected?.Invoke(this, node!);
    }

    private static void OnShowGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NodeLinkCanvas canvas)
        {
            canvas.DrawGrid();
        }
    }

    #endregion

    #region Event Handlers

    private void Node_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not NodeInfo node) return;

        if (_isLinkMode)
        {
            // 連線模式
            if (_linkStartNode == null)
            {
                _linkStartNode = node;
                StatusText.Text = $"已選擇起點「{node.Title}」，請點擊終點節點";
            }
            else
            {
                if (_linkStartNode != node)
                {
                    AddLink(_linkStartNode, node);
                }
                _linkStartNode = null;
                _isLinkMode = false;
                StatusText.Text = "連線完成";
            }
        }
        else
        {
            // 拖曳模式
            _isDragging = true;
            _dragStartPoint = e.GetCurrentPoint(NodeCanvas).Position;
            _nodeStartPosition = new Point(node.X, node.Y);
            button.CapturePointer(e.Pointer);
            SelectNode(node);
        }
    }

    private void Node_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging || sender is not Button button || button.Tag is not NodeInfo node) return;

        var currentPoint = e.GetCurrentPoint(NodeCanvas).Position;
        var deltaX = currentPoint.X - _dragStartPoint.X;
        var deltaY = currentPoint.Y - _dragStartPoint.Y;

        node.X = _nodeStartPosition.X + deltaX;
        node.Y = _nodeStartPosition.Y + deltaY;

        Canvas.SetLeft(button, node.X);
        Canvas.SetTop(button, node.Y);

        UpdateAllLinks();
    }

    private void Node_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Button button)
        {
            _isDragging = false;
            button.ReleasePointerCapture(e.Pointer);
        }
    }

    private void Node_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is NodeInfo node)
        {
            SelectNode(node);
        }
    }

    private void Link_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Line line && line.Tag is LinkInfo link)
        {
            _selectedLink = link;
            line.Stroke = new SolidColorBrush(Microsoft.UI.Colors.Blue);
        }
    }

    private void AddNodeButton_Click(object sender, RoutedEventArgs e)
    {
        var x = 100 + (_nodes.Count % 5) * 150;
        var y = 100 + (_nodes.Count / 5) * 100;
        AddNode($"節點 {_nodeCounter + 1}", x, y);
    }

    private void AddLinkButton_Click(object sender, RoutedEventArgs e)
    {
        _isLinkMode = true;
        _linkStartNode = null;
        StatusText.Text = "連線模式：請點擊起點節點";
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedLink != null)
        {
            RemoveLink(_selectedLink);
            _selectedLink = null;
        }
        else if (_selectedNode != null)
        {
            RemoveNode(_selectedNode);
            _selectedNode = null;
        }
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        Clear();
        StatusText.Text = "已清除所有節點和連線";
    }

    private void AutoLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        // 簡單的自動排列：網格布局
        int columns = 4;
        int spacing = 160;
        int startX = 100;
        int startY = 100;

        for (int i = 0; i < _nodes.Count; i++)
        {
            var node = _nodes[i];
            node.X = startX + (i % columns) * spacing;
            node.Y = startY + (i / columns) * spacing;

            if (node.Visual is FrameworkElement element)
            {
                Canvas.SetLeft(element, node.X);
                Canvas.SetTop(element, node.Y);
            }
        }

        UpdateAllLinks();
        StatusText.Text = "已自動排列節點";
    }

    #endregion
}

/// <summary>
/// 節點資訊
/// </summary>
public class NodeInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public object? Data { get; set; }
    public FrameworkElement? Visual { get; set; }
}

/// <summary>
/// 連線資訊
/// </summary>
public class LinkInfo
{
    public NodeInfo FromNode { get; set; } = null!;
    public NodeInfo ToNode { get; set; } = null!;
    public object? Data { get; set; }
    public FrameworkElement? Visual { get; set; }
    public FrameworkElement? ArrowVisual { get; set; }
}

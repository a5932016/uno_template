using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using unoTest.ViewModels;
using Windows.Foundation;
using Windows.UI;

namespace unoTest.Controls;

public sealed partial class NodeLinkCanvas : UserControl
{
    // 每個節點的根視覺：Border > StackPanel > [Button(image) + TextBlock]
    private readonly Dictionary<int, Border> _nodeBorders = new();

    private NodeLinkNodeViewModel? _draggingNode;
    private Point _dragStartPoint;
    private Point _nodeStartPosition;

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
            new PropertyMetadata(100.0, OnNodeMinWidthChanged));

    public double NodeMinWidth
    {
        get => (double)GetValue(NodeMinWidthProperty);
        set => SetValue(NodeMinWidthProperty, value);
    }

    public static readonly DependencyProperty LinkColorProperty =
        DependencyProperty.Register(nameof(LinkColor), typeof(Color), typeof(NodeLinkCanvas),
            new PropertyMetadata(Microsoft.UI.Colors.Gray, OnLinkColorChanged));

    public Color LinkColor
    {
        get => (Color)GetValue(LinkColorProperty);
        set => SetValue(LinkColorProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(NodeLinkCanvasViewModel), typeof(NodeLinkCanvas),
            new PropertyMetadata(null, OnViewModelChanged));

    public NodeLinkCanvasViewModel ViewModel
    {
        get => (NodeLinkCanvasViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public NodeLinkCanvas()
    {
        this.InitializeComponent();
        ViewModel = new NodeLinkCanvasViewModel();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DrawGrid();
        RenderGraph();
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NodeLinkCanvas canvas) return;

        if (e.OldValue is NodeLinkCanvasViewModel oldVm)
            canvas.DetachViewModel(oldVm);

        if (e.NewValue is not NodeLinkCanvasViewModel newVm)
        {
            canvas.ViewModel = new NodeLinkCanvasViewModel();
            return;
        }

        canvas.AttachViewModel(newVm);
        canvas.Bindings.Update();
        canvas.RenderGraph();
    }

    private static void OnShowGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NodeLinkCanvas canvas) canvas.DrawGrid();
    }

    private static void OnNodeMinWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NodeLinkCanvas canvas) { canvas.RenderNodes(); canvas.RenderLinks(); }
    }

    private static void OnLinkColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NodeLinkCanvas canvas) canvas.RenderLinks();
    }

    private void AttachViewModel(NodeLinkCanvasViewModel viewModel)
    {
        viewModel.GraphChanged += ViewModelOnGraphChanged;
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        foreach (var node in viewModel.Nodes)
            node.PropertyChanged += NodeOnPropertyChanged;

        DrawGrid();
        RenderGraph();
    }

    private void DetachViewModel(NodeLinkCanvasViewModel viewModel)
    {
        viewModel.GraphChanged -= ViewModelOnGraphChanged;
        viewModel.PropertyChanged -= ViewModelOnPropertyChanged;

        foreach (var node in viewModel.Nodes)
            node.PropertyChanged -= NodeOnPropertyChanged;
    }

    private void ViewModelOnGraphChanged(object? sender, EventArgs e)
    {
        foreach (var node in ViewModel.Nodes)
        {
            node.PropertyChanged -= NodeOnPropertyChanged;
            node.PropertyChanged += NodeOnPropertyChanged;
        }

        RenderNodes();
        RenderLinks();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NodeLinkCanvasViewModel.SelectedNodeId)
            or nameof(NodeLinkCanvasViewModel.SelectedLinkId))
        {
            UpdateSelectionVisuals();
            RenderLinks();
        }
    }

    private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NodeLinkNodeViewModel.X)
            or nameof(NodeLinkNodeViewModel.Y)
            or nameof(NodeLinkNodeViewModel.Title))
        {
            RenderNodes();
            RenderLinks();
        }
    }

    private void RenderGraph()
    {
        RenderNodes();
        RenderLinks();
    }

    private void RenderNodes()
    {
        var nodeIds = ViewModel.Nodes.Select(n => n.Id).ToHashSet();

        // 移除已不存在的節點視覺
        foreach (var entry in _nodeBorders.Where(e => !nodeIds.Contains(e.Key)).ToList())
        {
            UnwireNodeBorder(entry.Value);
            NodeCanvas.Children.Remove(entry.Value);
            _nodeBorders.Remove(entry.Key);
        }

        // 新增或更新節點
        foreach (var node in ViewModel.Nodes)
        {
            if (!_nodeBorders.TryGetValue(node.Id, out var border))
            {
                border = CreateNodeVisual(node);
                _nodeBorders[node.Id] = border;
                NodeCanvas.Children.Add(border);
            }
            else
            {
                // 更新文字（標題可能改變）
                if (border.Child is StackPanel stack)
                {
                    var tb = stack.Children.OfType<TextBlock>().FirstOrDefault();
                    if (tb is not null) tb.Text = node.TextInfo.Text;
                }
            }

            Canvas.SetLeft(border, node.X);
            Canvas.SetTop(border, node.Y);
        }

        UpdateSelectionVisuals();
    }

    private void RenderLinks()
    {
        LinkCanvas.Children.Clear();

        foreach (var link in ViewModel.Links)
        {
            var fromNode = ViewModel.FindNode(link.FromNodeId);
            var toNode = ViewModel.FindNode(link.ToNodeId);
            if (fromNode is null || toNode is null) continue;

            var fromCenter = GetNodeCenter(fromNode);
            var toCenter = GetNodeCenter(toNode);

            var isSelected = ViewModel.SelectedLinkId == link.Id;
            var strokeColor = isSelected ? Microsoft.UI.Colors.Blue : LinkColor;

            var line = new Line
            {
                X1 = fromCenter.X,
                Y1 = fromCenter.Y,
                X2 = toCenter.X,
                Y2 = toCenter.Y,
                StrokeThickness = isSelected ? 3 : 2,
                Stroke = new SolidColorBrush(strokeColor),
                Tag = link.Id
            };
            line.PointerPressed += LinkOnPointerPressed;
            LinkCanvas.Children.Add(line);

            AddArrowHead(fromCenter, toCenter, strokeColor);
        }
    }

    /// <summary>
    /// 建立 Border > StackPanel > [Button(image) + TextBlock] 節點視覺。
    /// Button 和 TextBlock 的 IsHitTestVisible=false，所有指標事件由 Border 處理。
    /// </summary>
    private Border CreateNodeVisual(NodeLinkNodeViewModel node)
    {
        var imageInfo = node.ImageInfo;
        var btnInfo = node.ButtonInfo;

        var img = new Image
        {
            IsHitTestVisible = false,
            Width = imageInfo.Width,
            Height = imageInfo.Height,
            Stretch = Stretch.Uniform,
        };
        if (!string.IsNullOrEmpty(imageInfo.Source))
        {
            try { img.Source = new BitmapImage(new Uri(imageInfo.Source)); }
            catch { /* 忽略無效路徑 */ }
        }

        var btn = new Button
        {
            IsHitTestVisible = false,
            Width = btnInfo.Width,
            Height = btnInfo.Height,
            CornerRadius = new CornerRadius(7),
            Content = img,
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51))
        };

        var text = new TextBlock
        {
            IsHitTestVisible = false,
            Text = node.TextInfo.Text,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 11, 0, 0)
        };

        var stack = new StackPanel
        {
            IsHitTestVisible = false,
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        stack.Children.Add(btn);
        stack.Children.Add(text);

        var border = new Border
        {
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            Child = stack,
            Tag = node.Id,
            MinWidth = NodeMinWidth
        };

        border.PointerPressed += NodeOnPointerPressed;
        border.PointerMoved += NodeOnPointerMoved;
        border.PointerReleased += NodeOnPointerReleased;
        border.PointerCanceled += NodeOnPointerCanceled;

        return border;
    }

    private void UnwireNodeBorder(Border border)
    {
        border.PointerPressed -= NodeOnPointerPressed;
        border.PointerMoved -= NodeOnPointerMoved;
        border.PointerReleased -= NodeOnPointerReleased;
        border.PointerCanceled -= NodeOnPointerCanceled;
    }

    private void UpdateSelectionVisuals()
    {
        foreach (var (nodeId, border) in _nodeBorders)
        {
            var isSelected = ViewModel.SelectedNodeId == nodeId;
            border.BorderBrush = isSelected ? new SolidColorBrush(Microsoft.UI.Colors.Blue) : null;
            border.BorderThickness = isSelected ? new Thickness(2) : new Thickness(0);
        }
    }

    private void AddArrowHead(Point fromCenter, Point toCenter, Color strokeColor)
    {
        var angle = Math.Atan2(toCenter.Y - fromCenter.Y, toCenter.X - fromCenter.X);
        const double arrowSize = 10;

        var arrow = new Polygon
        {
            Fill = new SolidColorBrush(strokeColor),
            Points = new PointCollection
            {
                new Point(0, 0),
                new Point(-arrowSize, -arrowSize / 2),
                new Point(-arrowSize, arrowSize / 2)
            }
        };

        var arrowX = toCenter.X - 30 * Math.Cos(angle);
        var arrowY = toCenter.Y - 30 * Math.Sin(angle);
        Canvas.SetLeft(arrow, arrowX);
        Canvas.SetTop(arrow, arrowY);
        arrow.RenderTransform = new RotateTransform { Angle = angle * 180 / Math.PI };
        arrow.RenderTransformOrigin = new Point(0, 0.5);
        LinkCanvas.Children.Add(arrow);
    }

    private Point GetNodeCenter(NodeLinkNodeViewModel node)
    {
        if (_nodeBorders.TryGetValue(node.Id, out var border))
        {
            var w = border.ActualWidth > 0 ? border.ActualWidth : Math.Max(NodeMinWidth, 70);
            var h = border.ActualHeight > 0 ? border.ActualHeight : 80;
            return new Point(node.X + w / 2, node.Y + h / 2);
        }

        return new Point(node.X + Math.Max(NodeMinWidth, 70) / 2, node.Y + 40);
    }

    private void DrawGrid()
    {
        GridCanvas.Children.Clear();
        if (!ShowGrid) return;

        const int gridSize = 20;
        var brush = new SolidColorBrush(Microsoft.UI.Colors.LightGray) { Opacity = 0.3 };
        var width = Math.Max(ActualWidth, GridCanvas.ActualWidth);
        var height = Math.Max(ActualHeight, GridCanvas.ActualHeight);

        for (double x = 0; x < width; x += gridSize)
        {
            GridCanvas.Children.Add(new Line
            {
                X1 = x, Y1 = 0, X2 = x, Y2 = height,
                Stroke = brush, StrokeThickness = 1
            });
        }

        for (double y = 0; y < height; y += gridSize)
        {
            GridCanvas.Children.Add(new Line
            {
                X1 = 0, Y1 = y, X2 = width, Y2 = y,
                Stroke = brush, StrokeThickness = 1
            });
        }
    }

    // ── Pointer events ────────────────────────────────────────────────────

    private void NodeOnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border border || border.Tag is not int nodeId) return;

        var node = ViewModel.FindNode(nodeId);
        if (node is null) return;

        _draggingNode = node;
        _dragStartPoint = e.GetCurrentPoint(NodeCanvas).Position;
        _nodeStartPosition = new Point(node.X, node.Y);

        border.CapturePointer(e.Pointer);
        ViewModel.SelectNode(node.Id);
        UpdateSelectionVisuals();
        RenderLinks();

        e.Handled = true;
    }

    private void NodeOnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_draggingNode is null) return;

        var current = e.GetCurrentPoint(NodeCanvas).Position;
        ViewModel.MoveNode(
            _draggingNode.Id,
            _nodeStartPosition.X + (current.X - _dragStartPoint.X),
            _nodeStartPosition.Y + (current.Y - _dragStartPoint.Y));
    }

    private void NodeOnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
            border.ReleasePointerCapture(e.Pointer);

        _draggingNode = null;

        // 拖曳結束，觸發排序並重建前後鏈連線
        ViewModel.EndDrag();

        e.Handled = true;
    }

    private void NodeOnPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        _draggingNode = null;
    }

    private void LinkOnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Line line && line.Tag is int linkId)
        {
            ViewModel.SelectLink(linkId);
            UpdateSelectionVisuals();
            RenderLinks();
            e.Handled = true;
        }
    }

    private void CanvasHost_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawGrid();
        RenderLinks();
    }
}

using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using unoTest.ViewModels;
using Windows.Foundation;
using Windows.UI;

namespace unoTest.Controls;

public sealed partial class NodeLinkCanvas : UserControl
{
    private readonly Dictionary<int, Button> _nodeButtons = new();
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
        if (d is not NodeLinkCanvas canvas)
        {
            return;
        }

        if (e.OldValue is NodeLinkCanvasViewModel oldViewModel)
        {
            canvas.DetachViewModel(oldViewModel);
        }

        if (e.NewValue is not NodeLinkCanvasViewModel newViewModel)
        {
            canvas.ViewModel = new NodeLinkCanvasViewModel();
            return;
        }

        canvas.AttachViewModel(newViewModel);
        canvas.Bindings.Update();
        canvas.RenderGraph();
    }

    private static void OnShowGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NodeLinkCanvas canvas)
        {
            canvas.DrawGrid();
        }
    }

    private static void OnNodeMinWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NodeLinkCanvas canvas)
        {
            canvas.RenderNodes();
            canvas.RenderLinks();
        }
    }

    private static void OnLinkColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NodeLinkCanvas canvas)
        {
            canvas.RenderLinks();
        }
    }

    private void AttachViewModel(NodeLinkCanvasViewModel viewModel)
    {
        if (viewModel.Nodes is INotifyCollectionChanged nodes)
        {
            nodes.CollectionChanged += NodesOnCollectionChanged;
        }

        if (viewModel.Links is INotifyCollectionChanged links)
        {
            links.CollectionChanged += LinksOnCollectionChanged;
        }

        viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        foreach (var node in viewModel.Nodes)
        {
            node.PropertyChanged += NodeOnPropertyChanged;
        }

        DrawGrid();
        RenderGraph();
    }

    private void DetachViewModel(NodeLinkCanvasViewModel viewModel)
    {
        if (viewModel.Nodes is INotifyCollectionChanged nodes)
        {
            nodes.CollectionChanged -= NodesOnCollectionChanged;
        }

        if (viewModel.Links is INotifyCollectionChanged links)
        {
            links.CollectionChanged -= LinksOnCollectionChanged;
        }

        viewModel.PropertyChanged -= ViewModelOnPropertyChanged;

        foreach (var node in viewModel.Nodes)
        {
            node.PropertyChanged -= NodeOnPropertyChanged;
        }
    }

    private void NodesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var oldNode in e.OldItems.OfType<NodeLinkNodeViewModel>())
            {
                oldNode.PropertyChanged -= NodeOnPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var newNode in e.NewItems.OfType<NodeLinkNodeViewModel>())
            {
                newNode.PropertyChanged += NodeOnPropertyChanged;
            }
        }

        RenderNodes();
        RenderLinks();
    }

    private void LinksOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
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
        var nodeIds = ViewModel.Nodes.Select(node => node.Id).ToHashSet();

        foreach (var entry in _nodeButtons.Where(entry => !nodeIds.Contains(entry.Key)).ToList())
        {
            var button = entry.Value;
            button.PointerPressed -= NodeOnPointerPressed;
            button.PointerMoved -= NodeOnPointerMoved;
            button.PointerReleased -= NodeOnPointerReleased;
            button.PointerCanceled -= NodeOnPointerCanceled;
            button.Click -= NodeOnClick;
            NodeCanvas.Children.Remove(button);
            _nodeButtons.Remove(entry.Key);
        }

        foreach (var node in ViewModel.Nodes)
        {
            if (!_nodeButtons.TryGetValue(node.Id, out var button))
            {
                button = CreateNodeButton(node.Id);
                _nodeButtons[node.Id] = button;
                NodeCanvas.Children.Add(button);
            }

            button.Content = node.Title;
            button.MinWidth = NodeMinWidth;

            Canvas.SetLeft(button, node.X);
            Canvas.SetTop(button, node.Y);
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
            if (fromNode is null || toNode is null)
            {
                continue;
            }

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

    private Button CreateNodeButton(int nodeId)
    {
        var button = new Button
        {
            Tag = nodeId,
            Padding = new Thickness(16, 12, 16, 12)
        };

        button.PointerPressed += NodeOnPointerPressed;
        button.PointerMoved += NodeOnPointerMoved;
        button.PointerReleased += NodeOnPointerReleased;
        button.PointerCanceled += NodeOnPointerCanceled;
        button.Click += NodeOnClick;

        return button;
    }

    private void UpdateSelectionVisuals()
    {
        foreach (var (nodeId, button) in _nodeButtons)
        {
            var isSelected = ViewModel.SelectedNodeId == nodeId;
            button.BorderBrush = isSelected ? new SolidColorBrush(Microsoft.UI.Colors.Blue) : null;
            button.BorderThickness = isSelected ? new Thickness(2) : new Thickness(0);
        }
    }

    private void AddArrowHead(Point fromCenter, Point toCenter, Color strokeColor)
    {
        var angle = Math.Atan2(toCenter.Y - fromCenter.Y, toCenter.X - fromCenter.X);
        const double arrowSize = 10;

        var arrowPath = new Polygon
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
        Canvas.SetLeft(arrowPath, arrowX);
        Canvas.SetTop(arrowPath, arrowY);

        arrowPath.RenderTransform = new RotateTransform { Angle = angle * 180 / Math.PI };
        arrowPath.RenderTransformOrigin = new Point(0, 0.5);
        LinkCanvas.Children.Add(arrowPath);
    }

    private Point GetNodeCenter(NodeLinkNodeViewModel node)
    {
        if (_nodeButtons.TryGetValue(node.Id, out var button))
        {
            var width = button.ActualWidth > 0 ? button.ActualWidth : NodeMinWidth;
            var height = button.ActualHeight > 0 ? button.ActualHeight : 44;

            return new Point(node.X + width / 2, node.Y + height / 2);
        }

        return new Point(node.X + NodeMinWidth / 2, node.Y + 22);
    }

    private void DrawGrid()
    {
        GridCanvas.Children.Clear();
        if (!ShowGrid)
        {
            return;
        }

        const int gridSize = 20;
        var brush = new SolidColorBrush(Microsoft.UI.Colors.LightGray) { Opacity = 0.3 };

        var width = Math.Max(ActualWidth, GridCanvas.ActualWidth);
        var height = Math.Max(ActualHeight, GridCanvas.ActualHeight);

        for (double x = 0; x < width; x += gridSize)
        {
            GridCanvas.Children.Add(new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = height,
                Stroke = brush,
                StrokeThickness = 1
            });
        }

        for (double y = 0; y < height; y += gridSize)
        {
            GridCanvas.Children.Add(new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = width,
                Y2 = y,
                Stroke = brush,
                StrokeThickness = 1
            });
        }
    }

    private void NodeOnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int nodeId)
        {
            return;
        }

        if (ViewModel.IsLinkMode)
        {
            ViewModel.HandleNodePressed(nodeId);
            UpdateSelectionVisuals();
            RenderLinks();
            return;
        }

        var node = ViewModel.FindNode(nodeId);
        if (node is null)
        {
            return;
        }

        _draggingNode = node;
        _dragStartPoint = e.GetCurrentPoint(NodeCanvas).Position;
        _nodeStartPosition = new Point(node.X, node.Y);

        button.CapturePointer(e.Pointer);
        ViewModel.SelectNode(node.Id);
        UpdateSelectionVisuals();
        RenderLinks();
    }

    private void NodeOnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_draggingNode is null)
        {
            return;
        }

        var currentPoint = e.GetCurrentPoint(NodeCanvas).Position;
        var deltaX = currentPoint.X - _dragStartPoint.X;
        var deltaY = currentPoint.Y - _dragStartPoint.Y;

        ViewModel.MoveNode(_draggingNode.Id, _nodeStartPosition.X + deltaX, _nodeStartPosition.Y + deltaY);
    }

    private void NodeOnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Button button)
        {
            button.ReleasePointerCapture(e.Pointer);
        }

        _draggingNode = null;
    }

    private void NodeOnPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        _draggingNode = null;
    }

    private void NodeOnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int nodeId)
        {
            ViewModel.SelectNode(nodeId);
            UpdateSelectionVisuals();
            RenderLinks();
        }
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

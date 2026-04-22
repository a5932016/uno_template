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
    // 深色按鈕背景色（#333333）
    private static readonly Color NodeButtonBackground = Color.FromArgb(255, 51, 51, 51);

    // Border 以 NodeInfo.Id 為鍵，供拖曳與選取使用
    private readonly Dictionary<int, Border> _nodeBorders = new();

    private NodeInfo? _draggingNode;
    private Point _dragStartPoint;
    private Point _nodeStartPosition;

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

    #endregion

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

    #region ViewModel wiring

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
        DrawGrid();
        RenderGraph();
    }

    private void DetachViewModel(NodeLinkCanvasViewModel viewModel)
    {
        viewModel.GraphChanged -= ViewModelOnGraphChanged;
        viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
    }

    private void ViewModelOnGraphChanged(object? sender, EventArgs e)
    {
        RenderNodes();
        RenderLinks();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NodeLinkCanvasViewModel.SelectedNode)
            or nameof(NodeLinkCanvasViewModel.SelectedLink))
        {
            UpdateSelectionVisuals();
            RenderLinks();
        }
    }

    #endregion

    #region Rendering

    private void RenderGraph()
    {
        RenderNodes();
        RenderLinks();
    }

    private void RenderNodes()
    {
        var nodeIds = ViewModel.GetNodes().Select(n => n.Id).ToHashSet();

        // 移除已不存在的節點視覺
        foreach (var entry in _nodeBorders.Where(e => !nodeIds.Contains(e.Key)).ToList())
        {
            UnwireNodeBorder(entry.Value);
            NodeCanvas.Children.Remove(entry.Value);
            _nodeBorders.Remove(entry.Key);
        }

        // 新增或更新節點
        foreach (var node in ViewModel.GetNodes())
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

        foreach (var link in ViewModel.GetLinks())
        {
            var fromCenter = GetNodeCenter(link.FromNode);
            var toCenter = GetNodeCenter(link.ToNode);

            var isSelected = ViewModel.SelectedLink == link;
            var strokeColor = isSelected ? Microsoft.UI.Colors.Blue : LinkColor;

            var line = new Line
            {
                X1 = fromCenter.X,
                Y1 = fromCenter.Y,
                X2 = toCenter.X,
                Y2 = toCenter.Y,
                StrokeThickness = isSelected ? 3 : 2,
                Stroke = new SolidColorBrush(strokeColor),
                Tag = link
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
    private Border CreateNodeVisual(NodeInfo node)
    {
        var imageInfo = node.ImageInfo;
        var btnInfo = node.ButtonInfo;

        var img = new Image
        {
            IsHitTestVisible = false,
            Width = imageInfo.Width,
            Height = imageInfo.Height,
            Stretch = Stretch.Uniform
        };
        if (!string.IsNullOrEmpty(imageInfo.Source))
        {
            try { img.Source = new BitmapImage(new Uri(imageInfo.Source)); }
            catch (UriFormatException) { /* 忽略無效 URI */ }
            catch (Exception) { /* 忽略其他載入錯誤 */ }
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
            Background = new SolidColorBrush(NodeButtonBackground)
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
            // IsHitTestVisible=false 在 StackPanel 層防止面板本身截取事件，
            // 確保所有指標事件都冒泡到 Border 處理。
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
            Tag = node,          // 存 NodeInfo，指標事件直接取用
            MinWidth = NodeMinWidth
        };

        border.PointerPressed += NodeOnPointerPressed;
        border.PointerMoved += NodeOnPointerMoved;
        border.PointerReleased += NodeOnPointerReleased;
        border.PointerCanceled += NodeOnPointerReleased;

        return border;
    }

    private void UnwireNodeBorder(Border border)
    {
        border.PointerPressed -= NodeOnPointerPressed;
        border.PointerMoved -= NodeOnPointerMoved;
        border.PointerReleased -= NodeOnPointerReleased;
        border.PointerCanceled -= NodeOnPointerReleased;
    }

    private void UpdateSelectionVisuals()
    {
        foreach (var (nodeId, border) in _nodeBorders)
        {
            var isSelected = ViewModel.SelectedNode?.Id == nodeId;
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

    private Point GetNodeCenter(NodeInfo node)
    {
        if (_nodeBorders.TryGetValue(node.Id, out var border))
        {
            var w = border.ActualWidth > 0 ? border.ActualWidth : node.Width;
            var h = border.ActualHeight > 0 ? border.ActualHeight : node.Height;
            return new Point(node.X + w / 2, node.Y + h / 2);
        }

        return new Point(node.X + node.Width / 2, node.Y + node.Height / 2);
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

    #endregion

    #region Pointer Events

    private void NodeOnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border border || border.Tag is not NodeInfo node) return;

        _draggingNode = node;
        _dragStartPoint = e.GetCurrentPoint(NodeCanvas).Position;
        _nodeStartPosition = new Point(node.X, node.Y);

        border.CapturePointer(e.Pointer);
        ViewModel.SelectNode(node);

        e.Handled = true;
    }

    private void NodeOnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_draggingNode is null || sender is not FrameworkElement element) return;

        var current = e.GetCurrentPoint(NodeCanvas).Position;

        // 直接更新 NodeInfo（與 ViewModel 共享同一物件參考）
        _draggingNode.X = _nodeStartPosition.X + (current.X - _dragStartPoint.X);
        _draggingNode.Y = _nodeStartPosition.Y + (current.Y - _dragStartPoint.Y);

        Canvas.SetLeft(element, _draggingNode.X);
        Canvas.SetTop(element, _draggingNode.Y);

        // 即時更新連線視覺
        RenderLinks();
    }

    private void NodeOnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.ReleasePointerCapture(e.Pointer);

        _draggingNode = null;

        // ⭐ 核心：拖曳結束後排序並重建前後鏈
        ViewModel.EndDrag();

        e.Handled = true;
    }

    private void LinkOnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Line line && line.Tag is LinkInfo link)
        {
            ViewModel.SelectLink(link);
            e.Handled = true;
        }
    }

    private void CanvasHost_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawGrid();
        RenderLinks();
    }

    #endregion
}

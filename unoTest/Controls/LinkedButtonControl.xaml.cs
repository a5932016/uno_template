using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace unoTest.Controls;

/// <summary>
/// Button 連線控件
/// 支援在 Button 項目之間繪製連接線
/// </summary>
public sealed partial class LinkedButtonControl : UserControl
{
    #region Private Fields

    private readonly Dictionary<string, FrameworkElement> _itemContainers = new();
    private readonly List<ButtonLinkInfo> _links = new();
    private LinkedButtonItem? _linkStartItem;
    private string? _linkStartSide;
    private bool _isLinking;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<LinkedButtonItem>),
            typeof(LinkedButtonControl), new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable<LinkedButtonItem>? ItemsSource
    {
        get => (IEnumerable<LinkedButtonItem>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty LinksSourceProperty =
        DependencyProperty.Register(nameof(LinksSource), typeof(IEnumerable<ButtonLinkInfo>),
            typeof(LinkedButtonControl), new PropertyMetadata(null, OnLinksSourceChanged));

    public IEnumerable<ButtonLinkInfo>? LinksSource
    {
        get => (IEnumerable<ButtonLinkInfo>?)GetValue(LinksSourceProperty);
        set => SetValue(LinksSourceProperty, value);
    }

    public static readonly DependencyProperty LinkColorProperty =
        DependencyProperty.Register(nameof(LinkColor), typeof(Brush), typeof(LinkedButtonControl),
            new PropertyMetadata(new SolidColorBrush(Microsoft.UI.Colors.Gray)));

    public Brush LinkColor
    {
        get => (Brush)GetValue(LinkColorProperty);
        set => SetValue(LinkColorProperty, value);
    }

    public static readonly DependencyProperty LinkThicknessProperty =
        DependencyProperty.Register(nameof(LinkThickness), typeof(double), typeof(LinkedButtonControl),
            new PropertyMetadata(2.0));

    public double LinkThickness
    {
        get => (double)GetValue(LinkThicknessProperty);
        set => SetValue(LinkThicknessProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<ButtonLinkInfo>? LinkCreated;
    public event EventHandler<ButtonLinkInfo>? LinkRemoved;
    public event EventHandler<LinkedButtonItem>? ItemClicked;
    public event EventHandler<LinkedButtonItem>? ItemDeleteRequested;

    #endregion

    public LinkedButtonControl()
    {
        this.InitializeComponent();

        ButtonsListView.ContainerContentChanging += ButtonsListView_ContainerContentChanging;
        SizeChanged += LinkedButtonControl_SizeChanged;
    }

    #region Property Changed Handlers

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LinkedButtonControl control)
        {
            control.ButtonsListView.ItemsSource = e.NewValue;
            control._itemContainers.Clear();
        }
    }

    private static void OnLinksSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LinkedButtonControl control && e.NewValue is IEnumerable<ButtonLinkInfo> links)
        {
            control._links.Clear();
            control._links.AddRange(links);
            control.UpdateLinks();
        }
    }

    #endregion

    #region Event Handlers

    private void ButtonsListView_ContainerContentChanging(ListViewBase sender,
        ContainerContentChangingEventArgs args)
    {
        if (args.Item is LinkedButtonItem item)
        {
            _itemContainers[item.Id] = args.ItemContainer;

            // 延遲更新連線，確保容器已經渲染
            DispatcherQueue.TryEnqueue(() => UpdateLinks());
        }
    }

    private void LinkedButtonControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateLinks();
    }

    private void ButtonItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            FindParentItem(button) is LinkedButtonItem item)
        {
            ItemClicked?.Invoke(this, item);
        }
    }

    private void Connector_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Ellipse connector &&
            FindParentItem(connector) is LinkedButtonItem item)
        {
            if (!_isLinking)
            {
                // 開始連線
                _isLinking = true;
                _linkStartItem = item;
                _linkStartSide = connector.Tag?.ToString();
                connector.Fill = new SolidColorBrush(Microsoft.UI.Colors.Orange);
            }
            else if (_linkStartItem != null && _linkStartItem.Id != item.Id)
            {
                // 完成連線
                var newLink = new ButtonLinkInfo
                {
                    FromId = _linkStartItem.Id,
                    ToId = item.Id,
                    FromSide = _linkStartSide ?? "Right",
                    ToSide = connector.Tag?.ToString() ?? "Left"
                };

                _links.Add(newLink);
                UpdateLinks();
                LinkCreated?.Invoke(this, newLink);

                ResetLinkingState();
            }
            else
            {
                // 取消連線
                ResetLinkingState();
            }
        }
    }

    private void LinkTo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem &&
            FindParentItem(menuItem) is LinkedButtonItem item)
        {
            _isLinking = true;
            _linkStartItem = item;
            _linkStartSide = "Right";
        }
    }

    private void Unlink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem &&
            FindParentItem(menuItem) is LinkedButtonItem item)
        {
            var linksToRemove = _links
                .Where(l => l.FromId == item.Id || l.ToId == item.Id)
                .ToList();

            foreach (var link in linksToRemove)
            {
                _links.Remove(link);
                LinkRemoved?.Invoke(this, link);
            }

            UpdateLinks();
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem &&
            FindParentItem(menuItem) is LinkedButtonItem item)
        {
            ItemDeleteRequested?.Invoke(this, item);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 新增連線
    /// </summary>
    public void AddLink(string fromId, string toId, string fromSide = "Right", string toSide = "Left")
    {
        var link = new ButtonLinkInfo
        {
            FromId = fromId,
            ToId = toId,
            FromSide = fromSide,
            ToSide = toSide
        };
        _links.Add(link);
        UpdateLinks();
    }

    /// <summary>
    /// 移除連線
    /// </summary>
    public void RemoveLink(string fromId, string toId)
    {
        var link = _links.FirstOrDefault(l => l.FromId == fromId && l.ToId == toId);
        if (link != null)
        {
            _links.Remove(link);
            UpdateLinks();
        }
    }

    /// <summary>
    /// 清除所有連線
    /// </summary>
    public void ClearLinks()
    {
        _links.Clear();
        LinkCanvas.Children.Clear();
    }

    /// <summary>
    /// 取得所有連線
    /// </summary>
    public IReadOnlyList<ButtonLinkInfo> GetLinks() => _links.AsReadOnly();

    #endregion

    #region Private Methods

    private void UpdateLinks()
    {
        LinkCanvas.Children.Clear();

        foreach (var link in _links)
        {
            if (_itemContainers.TryGetValue(link.FromId, out var fromContainer) &&
                _itemContainers.TryGetValue(link.ToId, out var toContainer))
            {
                DrawLink(fromContainer, toContainer, link);
            }
        }
    }

    private void DrawLink(FrameworkElement fromContainer, FrameworkElement toContainer, ButtonLinkInfo link)
    {
        try
        {
            // 計算連接點位置
            var fromConnector = FindConnector(fromContainer, link.FromSide);
            var toConnector = FindConnector(toContainer, link.ToSide);

            if (fromConnector == null || toConnector == null) return;

            var fromPos = fromConnector.TransformToVisual(LinkCanvas)
                .TransformPoint(new Point(fromConnector.ActualWidth / 2, fromConnector.ActualHeight / 2));
            var toPos = toConnector.TransformToVisual(LinkCanvas)
                .TransformPoint(new Point(toConnector.ActualWidth / 2, toConnector.ActualHeight / 2));

            // 繪製貝茲曲線
            var path = new Microsoft.UI.Xaml.Shapes.Path
            {
                Stroke = LinkColor,
                StrokeThickness = LinkThickness,
                Tag = link
            };

            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = fromPos };

            // 計算控制點（製作平滑曲線）
            var controlOffset = Math.Abs(toPos.X - fromPos.X) / 2;
            if (controlOffset < 50) controlOffset = 50;

            var cp1 = link.FromSide == "Right"
                ? new Point(fromPos.X + controlOffset, fromPos.Y)
                : new Point(fromPos.X - controlOffset, fromPos.Y);

            var cp2 = link.ToSide == "Left"
                ? new Point(toPos.X - controlOffset, toPos.Y)
                : new Point(toPos.X + controlOffset, toPos.Y);

            var bezier = new BezierSegment
            {
                Point1 = cp1,
                Point2 = cp2,
                Point3 = toPos
            };

            figure.Segments.Add(bezier);
            geometry.Figures.Add(figure);
            path.Data = geometry;

            // 添加箭頭
            DrawArrowHead(toPos, cp2);

            LinkCanvas.Children.Add(path);
        }
        catch
        {
            // 元素可能還未完全渲染
        }
    }

    private void DrawArrowHead(Point tip, Point controlPoint)
    {
        var angle = Math.Atan2(tip.Y - controlPoint.Y, tip.X - controlPoint.X);
        var arrowLength = 10;
        var arrowAngle = Math.PI / 6; // 30 度

        var p1 = new Point(
            tip.X - arrowLength * Math.Cos(angle - arrowAngle),
            tip.Y - arrowLength * Math.Sin(angle - arrowAngle));
        var p2 = new Point(
            tip.X - arrowLength * Math.Cos(angle + arrowAngle),
            tip.Y - arrowLength * Math.Sin(angle + arrowAngle));

        var arrow = new Polygon
        {
            Points = { tip, p1, p2 },
            Fill = LinkColor
        };

        LinkCanvas.Children.Add(arrow);
    }

    private static FrameworkElement? FindConnector(FrameworkElement container, string side)
    {
        return FindVisualChild<Ellipse>(container, e => e.Tag?.ToString() == side);
    }

    private static T? FindVisualChild<T>(DependencyObject parent, Func<T, bool>? predicate = null)
        where T : FrameworkElement
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild && (predicate == null || predicate(typedChild)))
            {
                return typedChild;
            }

            var result = FindVisualChild<T>(child, predicate);
            if (result != null) return result;
        }
        return null;
    }

    private LinkedButtonItem? FindParentItem(DependencyObject element)
    {
        var current = element;
        while (current != null)
        {
            if (current is FrameworkElement fe && fe.DataContext is LinkedButtonItem item)
            {
                return item;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private void ResetLinkingState()
    {
        _isLinking = false;
        _linkStartItem = null;
        _linkStartSide = null;
        UpdateLinks(); // 重繪以重置連接點顏色
    }

    #endregion
}

/// <summary>
/// Button 項目
/// </summary>
public class LinkedButtonItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = "\uE8A5";
    public object? Data { get; set; }
}

/// <summary>
/// Button 連線資訊
/// </summary>
public class ButtonLinkInfo
{
    public string FromId { get; set; } = string.Empty;
    public string ToId { get; set; } = string.Empty;
    public string FromSide { get; set; } = "Right";
    public string ToSide { get; set; } = "Left";
    public string? Label { get; set; }
}

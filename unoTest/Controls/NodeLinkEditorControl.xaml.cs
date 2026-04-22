using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace unoTest.Controls;

/// <summary>
/// 節點流程編輯器複合控件。
/// 左側嵌入 NodeLinkCanvas 畫布，右側顯示屬性面板（選取節點詳情 + 節點列表）。
/// 只需將 ViewModel（NodeLinkCanvasViewModel）指定給此控件，即可完整使用。
/// </summary>
public sealed partial class NodeLinkEditorControl : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel), typeof(NodeLinkCanvasViewModel), typeof(NodeLinkEditorControl),
            new PropertyMetadata(null, OnViewModelChanged));

    public NodeLinkCanvasViewModel? ViewModel
    {
        get => (NodeLinkCanvasViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    #endregion

    public NodeLinkEditorControl()
    {
        this.InitializeComponent();
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NodeLinkEditorControl ctrl) return;

        if (e.OldValue is NodeLinkCanvasViewModel oldVm)
        {
            oldVm.GraphChanged -= ctrl.OnGraphChanged;
            oldVm.PropertyChanged -= ctrl.OnViewModelPropertyChanged;
        }

        if (e.NewValue is NodeLinkCanvasViewModel newVm)
        {
            newVm.GraphChanged += ctrl.OnGraphChanged;
            newVm.PropertyChanged += ctrl.OnViewModelPropertyChanged;
        }

        ctrl.RefreshAll();
    }

    private void OnGraphChanged(object? sender, EventArgs e) => RefreshAll();

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NodeLinkCanvasViewModel.SelectedNode)
            or nameof(NodeLinkCanvasViewModel.SelectedLink))
        {
            RefreshSelectionPanel();
            // 節點列表需同步更新選取高亮
            RefreshNodeList();
        }
    }

    // ── Refresh helpers ────────────────────────────────────────────────────

    private void RefreshAll()
    {
        RefreshStats();
        RefreshSelectionPanel();
        RefreshNodeList();
    }

    private void RefreshStats()
    {
        NodeCountText.Text = (ViewModel?.GetNodes().Count ?? 0).ToString();
        LinkCountText.Text = (ViewModel?.GetLinks().Count ?? 0).ToString();
    }

    private void RefreshSelectionPanel()
    {
        var node = ViewModel?.SelectedNode;

        NoSelectionPanel.Visibility = node is null ? Visibility.Visible : Visibility.Collapsed;
        NodePropertiesPanel.Visibility = node is not null ? Visibility.Visible : Visibility.Collapsed;

        if (node is null) return;

        NodeIdText.Text = $"#{node.Id}";
        NodeTitleBox.Text = node.Title;
        NodePositionText.Text = $"X: {node.X:F0}   Y: {node.Y:F0}";
    }

    private void RefreshNodeList()
    {
        NodeListPanel.Children.Clear();

        var nodes = ViewModel?.GetNodes() ?? Array.Empty<NodeInfo>();
        var selectedNode = ViewModel?.SelectedNode;

        foreach (var node in nodes)
        {
            var isSelected = node == selectedNode;

            // 使用 Border + TextBlock 模擬按鈕，方便控制選取外觀
            var label = new TextBlock
            {
                Text = $"#{node.Id}  {node.Title}",
                FontSize = 13,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = isSelected ? 1.0 : 0.85
            };

            var itemBorder = new Border
            {
                Background = isSelected
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemAccentColorLight2Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 7, 10, 7),
                Child = label,
                Tag = node,
            };

            itemBorder.PointerPressed += NodeListItem_PointerPressed;
            NodeListPanel.Children.Add(itemBorder);
        }
    }

    private void NodeListItem_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border && border.Tag is NodeInfo node)
        {
            ViewModel?.SelectNode(node);
            e.Handled = true;
        }
    }

    private void NodeTitleBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.SelectedNode is not NodeInfo node) return;

        var newTitle = NodeTitleBox.Text.Trim();
        if (string.IsNullOrEmpty(newTitle) || newTitle == node.Title) return;

        node.Title = newTitle;
        node.TextInfo.Text = newTitle;

        // EndDrag 觸發排序 + 重建連線 + GraphChanged（讓畫布刷新節點文字）
        ViewModel.EndDrag();
    }
}

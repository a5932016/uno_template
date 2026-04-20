using Microsoft.UI.Xaml.Controls;
using unoTest.Controls;

namespace unoTest.Presentation;

/// <summary>
/// 節點連線示範頁面
/// </summary>
public sealed partial class NodeLinkDemoPage : Page
{
    public NodeLinkDemoPage()
    {
        this.InitializeComponent();
        this.Loaded += NodeLinkDemoPage_Loaded;
    }

    private void NodeLinkDemoPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // 預設添加一些示範節點
        var node1 = Canvas.AddNode("開始", 100, 100);
        var node2 = Canvas.AddNode("處理 A", 300, 100);
        var node3 = Canvas.AddNode("處理 B", 300, 250);
        var node4 = Canvas.AddNode("判斷", 500, 175);
        var node5 = Canvas.AddNode("結束", 700, 175);

        // 添加連線
        Canvas.AddLink(node1, node2);
        Canvas.AddLink(node1, node3);
        Canvas.AddLink(node2, node4);
        Canvas.AddLink(node3, node4);
        Canvas.AddLink(node4, node5);
    }

    private void Canvas_NodeAdded(object? sender, NodeInfo e)
    {
        System.Diagnostics.Debug.WriteLine($"新增節點：{e.Title}");
    }

    private void Canvas_LinkAdded(object? sender, LinkInfo e)
    {
        System.Diagnostics.Debug.WriteLine($"新增連線：{e.FromNode.Title} → {e.ToNode.Title}");
    }

    private void Canvas_NodeSelected(object? sender, NodeInfo e)
    {
        System.Diagnostics.Debug.WriteLine($"選擇節點：{e.Title}");
    }
}

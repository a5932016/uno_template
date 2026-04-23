namespace unoTest.Presentation;

/// <summary>
/// 節點流程編輯器頁面的 ViewModel。
/// 繼承 NodeLinkEditorViewModelBase，取得 SQLite 儲存、初始化、重置等共用功能。
/// 使用 NodeLinkEditorControl（包含 NodeLinkCanvas + 屬性面板）呈現畫布。
/// </summary>
public sealed class NodeLinkEditorViewModel : NodeLinkEditorViewModelBase
{
    protected override string GraphKey => "node-link-editor";

    public NodeLinkEditorViewModel(INodeGraphRepository repository)
        : base(repository, "節點流程編輯器")
    {
    }

    /// <summary>預設模板：四個線性節點，由左至右連接。</summary>
    protected override NodeGraphDocument CreateTemplateDocument()
    {
        return new NodeGraphDocument
        {
            GraphKey = GraphKey,
            Nodes =
            [
                new NodeGraphNodeDocument { Id = 1, Title = "起始",  X = 100, Y = 200 },
                new NodeGraphNodeDocument { Id = 2, Title = "步驟一", X = 260, Y = 200 },
                new NodeGraphNodeDocument { Id = 3, Title = "步驟二", X = 420, Y = 200 },
                new NodeGraphNodeDocument { Id = 4, Title = "完成",  X = 580, Y = 200 }
            ]
        };
    }
}

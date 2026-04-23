namespace unoTest.Presentation;

public sealed class NodeLinkDemoViewModel : NodeLinkEditorViewModelBase
{
    protected override string GraphKey => "node-link-demo";

    public NodeLinkDemoViewModel(INodeGraphRepository repository)
        : base(repository, "節點連線示範（MVVM + SQLite）")
    {
    }

    protected override NodeGraphDocument CreateTemplateDocument()
    {
        return new NodeGraphDocument
        {
            GraphKey = GraphKey,
            Nodes =
            [
                new NodeGraphNodeDocument { Id = 1, Title = "開始", X = 100, Y = 200 },
                new NodeGraphNodeDocument { Id = 2, Title = "處理 A", X = 260, Y = 200 },
                new NodeGraphNodeDocument { Id = 3, Title = "處理 B", X = 420, Y = 200 },
                new NodeGraphNodeDocument { Id = 4, Title = "判斷", X = 580, Y = 200 },
                new NodeGraphNodeDocument { Id = 5, Title = "結束", X = 740, Y = 200 }
            ]
        };
    }
}

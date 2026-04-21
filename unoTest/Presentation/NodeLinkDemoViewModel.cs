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
                new NodeGraphNodeDocument { Id = 1, Title = "開始", X = 100, Y = 100 },
                new NodeGraphNodeDocument { Id = 2, Title = "處理 A", X = 300, Y = 100 },
                new NodeGraphNodeDocument { Id = 3, Title = "處理 B", X = 300, Y = 250 },
                new NodeGraphNodeDocument { Id = 4, Title = "判斷", X = 500, Y = 175 },
                new NodeGraphNodeDocument { Id = 5, Title = "結束", X = 700, Y = 175 }
            ],
            Links =
            [
                new NodeGraphLinkDocument { Id = 1, FromNodeId = 1, ToNodeId = 2 },
                new NodeGraphLinkDocument { Id = 2, FromNodeId = 1, ToNodeId = 3 },
                new NodeGraphLinkDocument { Id = 3, FromNodeId = 2, ToNodeId = 4 },
                new NodeGraphLinkDocument { Id = 4, FromNodeId = 3, ToNodeId = 4 },
                new NodeGraphLinkDocument { Id = 5, FromNodeId = 4, ToNodeId = 5 }
            ]
        };
    }
}

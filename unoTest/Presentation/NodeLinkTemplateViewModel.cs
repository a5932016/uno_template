namespace unoTest.Presentation;

public sealed class NodeLinkTemplateViewModel : NodeLinkEditorViewModelBase
{
    protected override string GraphKey => "node-link-template";

    public NodeLinkTemplateViewModel(INodeGraphRepository repository)
        : base(repository, "流程模板編輯器（重用 NodeLinkCanvas）")
    {
    }

    protected override NodeGraphDocument CreateTemplateDocument()
    {
        return new NodeGraphDocument
        {
            GraphKey = GraphKey,
            Nodes =
            [
                new NodeGraphNodeDocument { Id = 1, Title = "需求整理", X = 120, Y = 120 },
                new NodeGraphNodeDocument { Id = 2, Title = "技術設計", X = 340, Y = 120 },
                new NodeGraphNodeDocument { Id = 3, Title = "開發實作", X = 560, Y = 120 },
                new NodeGraphNodeDocument { Id = 4, Title = "整合測試", X = 340, Y = 300 },
                new NodeGraphNodeDocument { Id = 5, Title = "部署發布", X = 560, Y = 300 }
            ],
            Links =
            [
                new NodeGraphLinkDocument { Id = 1, FromNodeId = 1, ToNodeId = 2 },
                new NodeGraphLinkDocument { Id = 2, FromNodeId = 2, ToNodeId = 3 },
                new NodeGraphLinkDocument { Id = 3, FromNodeId = 3, ToNodeId = 4 },
                new NodeGraphLinkDocument { Id = 4, FromNodeId = 4, ToNodeId = 5 }
            ]
        };
    }
}

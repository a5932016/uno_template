namespace unoTest.Presentation;

/// <summary>
/// 頁面布局教學頁面的 ViewModel
/// </summary>
public partial class LayoutDemoViewModel : ObservableObject
{
    public LayoutDemoViewModel()
    {
        // 初始化示範項目
        DemoItems = new List<string>
        {
            "項目 1", "項目 2", "項目 3", "項目 4",
            "項目 5", "項目 6", "項目 7", "項目 8"
        };
    }

    public string Title => "頁面布局教學";

    public List<string> DemoItems { get; }
}

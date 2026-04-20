using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace unoTest.Presentation;

/// <summary>
/// 功能示範索引頁面
/// </summary>
public sealed partial class DemoIndexPage : Page
{
    public DemoIndexPage()
    {
        this.InitializeComponent();
    }
}

/// <summary>
/// 示範索引頁面 ViewModel
/// </summary>
public partial class DemoIndexViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    
    public List<DemoItem> DemoItems { get; }

    public DemoIndexViewModel(INavigator navigator)
    {
        _navigator = navigator;
        
        DemoItems = new List<DemoItem>
        {
            new DemoItem("登入頁面", "完整的登入流程，支援第三方登入和表單驗證", "\uE77B", 
                Windows.UI.Color.FromArgb(255, 99, 102, 241), "Login", true, NavigateTo),
            
            new DemoItem("頁面布局教學", "Grid、StackPanel、響應式設計等布局技巧示範", "\uF0E2", 
                Windows.UI.Color.FromArgb(255, 16, 185, 129), "LayoutDemo", false, NavigateTo),
            
            new DemoItem("CRUD 示範", "產品管理的增刪改查、搜尋、分頁功能完整示範", "\uE8A5", 
                Windows.UI.Color.FromArgb(255, 245, 158, 11), "CrudDemo", true, NavigateTo),
            
            new DemoItem("節點連線", "Button 之間用線條連接的互動式流程圖畫布", "\uE71B", 
                Windows.UI.Color.FromArgb(255, 139, 92, 246), "NodeLinkDemo", true, NavigateTo),
            
            new DemoItem("設定頁面", "主題、語言、通知等應用程式設定範例", "\uE713", 
                Windows.UI.Color.FromArgb(255, 107, 114, 128), "Settings", false, NavigateTo),
            
            new DemoItem("主頁面", "原始的主頁面範例與導航", "\uE80F", 
                Windows.UI.Color.FromArgb(255, 59, 130, 246), "Main", false, NavigateTo)
        };
    }
    
    private async void NavigateTo(string route)
    {
        await _navigator.NavigateRouteAsync(this, route);
    }
}

/// <summary>
/// 示範項目
/// </summary>
public class DemoItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public Brush IconBackground { get; set; }
    public string Route { get; set; }
    public bool IsNew { get; set; }
    public ICommand NavigateCommand { get; set; }
    
    public DemoItem(string title, string description, string icon, Windows.UI.Color color, string route, bool isNew, Action<string> navigateAction)
    {
        Title = title;
        Description = description;
        Icon = icon;
        IconBackground = new SolidColorBrush(color);
        Route = route;
        IsNew = isNew;
        NavigateCommand = new RelayCommand(() => navigateAction(route));
    }
}

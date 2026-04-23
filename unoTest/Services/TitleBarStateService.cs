namespace unoTest.Services;

/// <summary>
/// TitleBar 的顯示模式
/// </summary>
public enum TitleBarMode
{
    /// <summary>主頁面：TitleBar 中間顯示 Tab 導航列</summary>
    Tabs,
    /// <summary>詳情頁面：TitleBar 顯示返回按鈕 + 頁面標題</summary>
    Detail
}

/// <summary>
/// 管理 TitleBar 顯示狀態的 Singleton 服務
/// 作為 「頁面 ViewModel → TitleBar」之間的通訊橋樑
/// 頁面 ViewModel 呼叫此服務切換 TitleBar 樣式，CustomTitleBar 透過 TitleBarViewModel 反應
/// </summary>
// ★ partial 是必要的：[ObservableProperty] 由 CommunityToolkit.Mvvm source generator 產生 partial 類別
public partial class TitleBarStateService : ObservableObject
{
    [ObservableProperty]
    private TitleBarMode _mode = TitleBarMode.Tabs;

    [ObservableProperty]
    private string _detailPageTitle = string.Empty;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    /// <summary>
    /// 切換為 Tab 模式（主頁面用）
    /// </summary>
    /// <param name="tabIndex">要選中的 Tab 索引（-1 = 不改變）</param>
    public void SetTabsMode(int tabIndex = -1)
    {
        Mode = TitleBarMode.Tabs;
        if (tabIndex >= 0)
            SelectedTabIndex = tabIndex;
    }

    /// <summary>
    /// 切換為 Detail 模式（詳情頁面用），TitleBar 顯示返回按鈕 + 標題
    /// </summary>
    /// <param name="pageTitle">頁面標題文字</param>
    public void SetDetailMode(string pageTitle)
    {
        Mode = TitleBarMode.Detail;
        DetailPageTitle = pageTitle;
    }
}

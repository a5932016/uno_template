using Microsoft.UI.Xaml.Controls;

namespace unoTest.Presentation;

/// <summary>
/// Tab 頁面切換示範
/// </summary>
public sealed partial class TabDemoPage : Page
{
    private int _tabCounter = 3;

    public TabDemoPage()
    {
        this.InitializeComponent();
    }

    #region TabView Events

    private void TabView_AddTabButtonClick(TabView sender, object args)
    {
        _tabCounter++;
        var newTab = new TabViewItem
        {
            Header = $"新分頁 {_tabCounter}",
            IconSource = new SymbolIconSource { Symbol = Symbol.Document },
            Content = new Grid
            {
                Padding = new Thickness(16),
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 12,
                        Children =
                        {
                            new TextBlock 
                            { 
                                Text = $"新分頁 {_tabCounter}", 
                                Style = (Style)Resources["TitleTextBlockStyle"] 
                            },
                            new TextBlock 
                            { 
                                Text = $"這是動態新增的分頁內容",
                                Opacity = 0.7
                            }
                        }
                    }
                }
            }
        };

        sender.TabItems.Add(newTab);
        sender.SelectedItem = newTab;
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        sender.TabItems.Remove(args.Tab);
    }

    private void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 分頁切換時的處理
        if (sender is TabView tabView && tabView.SelectedItem is TabViewItem selectedTab)
        {
            System.Diagnostics.Debug.WriteLine($"切換到分頁: {selectedTab.Header}");
        }
    }

    #endregion

    #region NavigationView Events

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            NavViewContent.Text = tag switch
            {
                "Overview" => "概覽頁面內容",
                "Analytics" => "分析頁面內容",
                "Reports" => "報表頁面內容",
                _ => "未知頁面"
            };
        }
    }

    #endregion

    #region Segmented Control Events

    private void Segment_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radio)
        {
            var content = radio.Content?.ToString();
            SegmentContent.Text = $"顯示「{content}」的內容";
            SegmentDescription.Text = content switch
            {
                "日" => "每日數據統計",
                "週" => "每週數據統計",
                "月" => "每月數據統計",
                "年" => "每年數據統計",
                _ => ""
            };
        }
    }

    #endregion

    #region TabBar Events

    private void TabBar_SelectionChanged(object sender, Uno.Toolkit.UI.TabBarSelectionChangedEventArgs e)
    {
        if (e.NewItem is Uno.Toolkit.UI.TabBarItem item)
        {
            var content = item.Content?.ToString() ?? "未知";
            TabBarText.Text = content;
            TabBarIcon.Glyph = content switch
            {
                "首頁" => "\uE80F",
                "搜尋" => "\uE721",
                "通知" => "\uE7E7",
                "我的" => "\uE77B",
                _ => "\uE80F"
            };
        }
    }

    #endregion
}

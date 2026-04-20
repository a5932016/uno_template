using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using unoTest.Controls;

namespace unoTest.Presentation;

/// <summary>
/// List 內 Button 連線示範頁面
/// </summary>
public sealed partial class LinkedListDemoPage : Page
{
    private int _itemCounter = 0;
    private ObservableCollection<ListLinkItem> _items = new();

    public LinkedListDemoPage()
    {
        this.InitializeComponent();
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        _items = new ObservableCollection<ListLinkItem>
        {
            new ListLinkItem { Id = "1", Title = "開始", Description = "流程起點", Icon = "\uE768" },
            new ListLinkItem { Id = "2", Title = "資料驗證", Description = "檢查輸入資料", Icon = "\uE73E" },
            new ListLinkItem { Id = "3", Title = "處理資料", Description = "執行業務邏輯", Icon = "\uE90F" },
            new ListLinkItem { Id = "4", Title = "儲存結果", Description = "寫入資料庫", Icon = "\uE74E" },
            new ListLinkItem { Id = "5", Title = "結束", Description = "流程終點", Icon = "\uE73B" },
        };
        _itemCounter = 5;

        LinkedList.ItemsSource = _items;

        // 預設連線
        LinkedList.AddLink("1", "2");
        LinkedList.AddLink("2", "3");
        LinkedList.AddLink("3", "4");
        LinkedList.AddLink("4", "5");
    }

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        _itemCounter++;
        var newItem = new ListLinkItem
        {
            Id = _itemCounter.ToString(),
            Title = $"步驟 {_itemCounter}",
            Description = "新增的步驟",
            Icon = "\uE8A5"
        };
        _items.Add(newItem);
    }

    private void ClearLinksButton_Click(object sender, RoutedEventArgs e)
    {
        LinkedList.ClearLinks();
    }

    private void LinkedList_LinkCreated(object? sender, ListLinkInfo e)
    {
        System.Diagnostics.Debug.WriteLine($"連線已建立: {e.FromId} -> {e.ToId}");
    }

    private void LinkedList_LinkRemoved(object? sender, ListLinkInfo e)
    {
        System.Diagnostics.Debug.WriteLine($"連線已移除: {e.FromId} -> {e.ToId}");
    }

    private async void LinkedList_ItemEditRequested(object? sender, ListLinkItem e)
    {
        var dialog = new ContentDialog
        {
            Title = "編輯項目",
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBox { Header = "標題", Text = e.Title, Name = "TitleBox" },
                    new TextBox { Header = "描述", Text = e.Description, Name = "DescBox" }
                }
            },
            PrimaryButtonText = "儲存",
            CloseButtonText = "取消",
            XamlRoot = this.XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            if (dialog.Content is StackPanel panel)
            {
                if (panel.Children[0] is TextBox titleBox)
                    e.Title = titleBox.Text;
                if (panel.Children[1] is TextBox descBox)
                    e.Description = descBox.Text;
            }
        }
    }

    private async void LinkedList_ItemDeleteRequested(object? sender, ListLinkItem e)
    {
        var dialog = new ContentDialog
        {
            Title = "確認刪除",
            Content = $"確定要刪除「{e.Title}」嗎？相關的連線也會一併移除。",
            PrimaryButtonText = "刪除",
            CloseButtonText = "取消",
            XamlRoot = this.XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            _items.Remove(e);
        }
    }
}

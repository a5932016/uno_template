using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace unoTest.Services;

/// <summary>
/// 對話框服務介面
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// 顯示訊息對話框
    /// </summary>
    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// 顯示確認對話框
    /// </summary>
    Task<bool> ShowConfirmAsync(string title, string message, string confirmText = "確定", string cancelText = "取消");

    /// <summary>
    /// 顯示輸入對話框
    /// </summary>
    Task<string?> ShowInputAsync(string title, string placeholder = "", string defaultValue = "");

    /// <summary>
    /// 顯示選擇對話框
    /// </summary>
    Task<T?> ShowSelectionAsync<T>(string title, IEnumerable<T> items, Func<T, string> displaySelector);

    /// <summary>
    /// 顯示自訂內容對話框
    /// </summary>
    Task<ContentDialogResult> ShowCustomAsync(string title, object content, 
        string? primaryButtonText = null, string? secondaryButtonText = null, string? closeButtonText = "關閉");

    /// <summary>
    /// 顯示載入中對話框
    /// </summary>
    IDisposable ShowLoading(string message = "載入中...");

    /// <summary>
    /// 顯示錯誤對話框
    /// </summary>
    Task ShowErrorAsync(string title, string message, string? errorCode = null, Exception? exception = null);

    /// <summary>
    /// 顯示成功通知
    /// </summary>
    void ShowSuccess(string message, int durationMs = 3000);

    /// <summary>
    /// 顯示警告通知
    /// </summary>
    void ShowWarning(string message, int durationMs = 3000);

    /// <summary>
    /// 顯示資訊通知
    /// </summary>
    void ShowInfo(string message, int durationMs = 3000);
}

/// <summary>
/// 對話框服務實作
/// </summary>
public class DialogService : IDialogService
{
    private XamlRoot? _xamlRoot;
    private ContentDialog? _loadingDialog;

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public async Task ShowMessageAsync(string title, string message)
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "確定",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task<bool> ShowConfirmAsync(string title, string message, 
        string confirmText = "確定", string cancelText = "取消")
    {
        if (_xamlRoot == null) return false;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = confirmText,
            CloseButtonText = cancelText,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async Task<string?> ShowInputAsync(string title, string placeholder = "", string defaultValue = "")
    {
        if (_xamlRoot == null) return null;

        var inputBox = new TextBox
        {
            PlaceholderText = placeholder,
            Text = defaultValue,
            AcceptsReturn = false
        };

        var dialog = new ContentDialog
        {
            Title = title,
            Content = inputBox,
            PrimaryButtonText = "確定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? inputBox.Text : null;
    }

    public async Task<T?> ShowSelectionAsync<T>(string title, IEnumerable<T> items, 
        Func<T, string> displaySelector)
    {
        if (_xamlRoot == null) return default;

        var listView = new ListView
        {
            ItemsSource = items.Select(item => new SelectionItem<T>
            {
                Value = item,
                DisplayText = displaySelector(item)
            }).ToList(),
            SelectionMode = ListViewSelectionMode.Single,
            MaxHeight = 300
        };

        listView.ItemTemplate = CreateSelectionItemTemplate();

        var dialog = new ContentDialog
        {
            Title = title,
            Content = listView,
            PrimaryButtonText = "選擇",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && listView.SelectedItem is SelectionItem<T> selected)
        {
            return selected.Value;
        }
        return default;
    }

    public async Task<ContentDialogResult> ShowCustomAsync(string title, object content,
        string? primaryButtonText = null, string? secondaryButtonText = null, string? closeButtonText = "關閉")
    {
        if (_xamlRoot == null) return ContentDialogResult.None;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButtonText ?? string.Empty,
            SecondaryButtonText = secondaryButtonText ?? string.Empty,
            CloseButtonText = closeButtonText ?? string.Empty,
            XamlRoot = _xamlRoot
        };

        return await dialog.ShowAsync();
    }

    public IDisposable ShowLoading(string message = "載入中...")
    {
        if (_xamlRoot == null) return new EmptyDisposable();

        _loadingDialog = new ContentDialog
        {
            Content = new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    new ProgressRing { IsActive = true, Width = 48, Height = 48 },
                    new TextBlock { Text = message, HorizontalAlignment = HorizontalAlignment.Center }
                }
            },
            XamlRoot = _xamlRoot
        };

        _ = _loadingDialog.ShowAsync();
        return new LoadingDialogDisposer(_loadingDialog);
    }

    public async Task ShowErrorAsync(string title, string message, string? errorCode = null, Exception? exception = null)
    {
        if (_xamlRoot == null) return;

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap });

        if (!string.IsNullOrEmpty(errorCode))
        {
            content.Children.Add(new TextBlock
            {
                Text = $"錯誤代碼: {errorCode}",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                FontSize = 12
            });
        }

        if (exception != null)
        {
            var expander = new Expander
            {
                Header = "詳細資訊",
                Content = new ScrollViewer
                {
                    MaxHeight = 200,
                    Content = new TextBlock
                    {
                        Text = exception.ToString(),
                        FontSize = 11,
                        FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                        TextWrapping = TextWrapping.Wrap,
                        IsTextSelectionEnabled = true
                    }
                }
            };
            content.Children.Add(expander);
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "關閉",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }

    public void ShowSuccess(string message, int durationMs = 3000)
    {
        ShowNotification(message, InfoBarSeverity.Success, durationMs);
    }

    public void ShowWarning(string message, int durationMs = 3000)
    {
        ShowNotification(message, InfoBarSeverity.Warning, durationMs);
    }

    public void ShowInfo(string message, int durationMs = 3000)
    {
        ShowNotification(message, InfoBarSeverity.Informational, durationMs);
    }

    private void ShowNotification(string message, InfoBarSeverity severity, int durationMs)
    {
        // 這裡需要配合 UI 層的 InfoBar 控件
        // 可以透過事件或其他機制來顯示
        NotificationRequested?.Invoke(this, new NotificationEventArgs(message, severity, durationMs));
    }

    public event EventHandler<NotificationEventArgs>? NotificationRequested;

    private static DataTemplate CreateSelectionItemTemplate()
    {
        // 簡單的文字模板
        return (DataTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"
            <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                <TextBlock Text=""{Binding DisplayText}"" Padding=""8""/>
            </DataTemplate>");
    }

    private class SelectionItem<T>
    {
        public T? Value { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    private class LoadingDialogDisposer : IDisposable
    {
        private readonly ContentDialog _dialog;

        public LoadingDialogDisposer(ContentDialog dialog)
        {
            _dialog = dialog;
        }

        public void Dispose()
        {
            _dialog.Hide();
        }
    }

    private class EmptyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

/// <summary>
/// 通知事件參數
/// </summary>
public class NotificationEventArgs : EventArgs
{
    public string Message { get; }
    public InfoBarSeverity Severity { get; }
    public int DurationMs { get; }

    public NotificationEventArgs(string message, InfoBarSeverity severity, int durationMs)
    {
        Message = message;
        Severity = severity;
        DurationMs = durationMs;
    }
}

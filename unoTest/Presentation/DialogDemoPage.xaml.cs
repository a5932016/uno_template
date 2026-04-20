using Microsoft.UI.Xaml.Controls;

namespace unoTest.Presentation;

/// <summary>
/// 彈出視窗示範頁面
/// </summary>
public sealed partial class DialogDemoPage : Page
{
    public DialogDemoPage()
    {
        this.InitializeComponent();
    }

    #region ContentDialog

    private async void ShowMessage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "訊息",
            Content = "這是一個簡單的訊息對話框。",
            CloseButtonText = "確定",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async void ShowConfirm_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "確認刪除",
            Content = "確定要刪除這個項目嗎？此操作無法復原。",
            PrimaryButtonText = "刪除",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ShowNotification("項目已刪除", InfoBarSeverity.Success);
        }
        else
        {
            ShowNotification("已取消刪除", InfoBarSeverity.Informational);
        }
    }

    private async void ShowInput_Click(object sender, RoutedEventArgs e)
    {
        var inputBox = new TextBox
        {
            PlaceholderText = "請輸入名稱",
            Header = "名稱"
        };

        var dialog = new ContentDialog
        {
            Title = "輸入資料",
            Content = inputBox,
            PrimaryButtonText = "確定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(inputBox.Text))
        {
            ShowNotification($"您輸入的是: {inputBox.Text}", InfoBarSeverity.Success);
        }
    }

    private async void ShowSelection_Click(object sender, RoutedEventArgs e)
    {
        var items = new List<string> { "選項 A", "選項 B", "選項 C", "選項 D" };
        var listView = new ListView
        {
            ItemsSource = items,
            SelectionMode = ListViewSelectionMode.Single,
            MaxHeight = 200
        };

        var dialog = new ContentDialog
        {
            Title = "選擇一個選項",
            Content = listView,
            PrimaryButtonText = "選擇",
            CloseButtonText = "取消",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && listView.SelectedItem is string selected)
        {
            ShowNotification($"您選擇了: {selected}", InfoBarSeverity.Success);
        }
    }

    private async void ShowCustom_Click(object sender, RoutedEventArgs e)
    {
        var content = new StackPanel { Spacing = 12 };
        content.Children.Add(new TextBox { Header = "姓名", PlaceholderText = "請輸入姓名" });
        content.Children.Add(new TextBox { Header = "Email", PlaceholderText = "請輸入 Email" });
        content.Children.Add(new ComboBox
        {
            Header = "部門",
            ItemsSource = new[] { "研發部", "行銷部", "業務部", "人資部" },
            SelectedIndex = 0
        });
        content.Children.Add(new CheckBox { Content = "訂閱電子報" });

        var dialog = new ContentDialog
        {
            Title = "新增員工",
            Content = content,
            PrimaryButtonText = "新增",
            SecondaryButtonText = "新增並繼續",
            CloseButtonText = "取消",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        switch (result)
        {
            case ContentDialogResult.Primary:
                ShowNotification("員工已新增", InfoBarSeverity.Success);
                break;
            case ContentDialogResult.Secondary:
                ShowNotification("員工已新增，請繼續輸入下一位", InfoBarSeverity.Informational);
                break;
        }
    }

    #endregion

    #region Error Dialog

    private async void ShowSimpleError_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "錯誤",
            Content = "操作失敗，請稍後再試。",
            CloseButtonText = "關閉",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async void ShowErrorWithCode_Click(object sender, RoutedEventArgs e)
    {
        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(new TextBlock { Text = "無法連線到伺服器，請檢查網路連線。", TextWrapping = TextWrapping.Wrap });
        content.Children.Add(new TextBlock
        {
            Text = "錯誤代碼: ERR_NETWORK_001",
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
            FontSize = 12
        });

        var dialog = new ContentDialog
        {
            Title = "連線錯誤",
            Content = content,
            PrimaryButtonText = "重試",
            CloseButtonText = "關閉",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ShowNotification("正在重試...", InfoBarSeverity.Informational);
        }
    }

    private async void ShowErrorWithException_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 模擬例外
            throw new InvalidOperationException("這是一個模擬的例外狀況，用於展示錯誤對話框。");
        }
        catch (Exception ex)
        {
            var content = new StackPanel { Spacing = 8 };
            content.Children.Add(new TextBlock { Text = ex.Message, TextWrapping = TextWrapping.Wrap });
            content.Children.Add(new TextBlock
            {
                Text = "錯誤代碼: ERR_INVALID_OP_002",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                FontSize = 12
            });

            var expander = new Expander
            {
                Header = "詳細資訊",
                Content = new ScrollViewer
                {
                    MaxHeight = 150,
                    Content = new TextBlock
                    {
                        Text = ex.ToString(),
                        FontSize = 11,
                        FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                        TextWrapping = TextWrapping.Wrap,
                        IsTextSelectionEnabled = true
                    }
                },
                Margin = new Thickness(0, 8, 0, 0)
            };
            content.Children.Add(expander);

            var dialog = new ContentDialog
            {
                Title = "發生錯誤",
                Content = content,
                PrimaryButtonText = "複製錯誤資訊",
                CloseButtonText = "關閉",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(ex.ToString());
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                ShowNotification("錯誤資訊已複製到剪貼簿", InfoBarSeverity.Success);
            }
        }
    }

    #endregion

    #region Loading Dialog

    private async void ShowLoading_Click(object sender, RoutedEventArgs e)
    {
        var loadingContent = new StackPanel
        {
            Spacing = 16,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        loadingContent.Children.Add(new ProgressRing { IsActive = true, Width = 48, Height = 48 });
        loadingContent.Children.Add(new TextBlock { Text = "載入中，請稍候..." });

        var dialog = new ContentDialog
        {
            Content = loadingContent,
            XamlRoot = this.XamlRoot
        };

        // 開啟對話框（不等待關閉）
        _ = dialog.ShowAsync();

        // 模擬載入
        await Task.Delay(3000);

        // 關閉對話框
        dialog.Hide();
        ShowNotification("載入完成！", InfoBarSeverity.Success);
    }

    private async void ShowProgress_Click(object sender, RoutedEventArgs e)
    {
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Width = 300
        };
        var statusText = new TextBlock { Text = "準備中..." };

        var content = new StackPanel { Spacing = 16 };
        content.Children.Add(statusText);
        content.Children.Add(progressBar);

        var dialog = new ContentDialog
        {
            Title = "下載進度",
            Content = content,
            CloseButtonText = "取消",
            XamlRoot = this.XamlRoot
        };

        var cancelled = false;
        dialog.CloseButtonClick += (s, args) => cancelled = true;

        // 開啟對話框（不等待關閉）
        _ = dialog.ShowAsync();

        // 模擬進度
        for (int i = 0; i <= 100 && !cancelled; i += 10)
        {
            progressBar.Value = i;
            statusText.Text = $"下載中... {i}%";
            await Task.Delay(300);
        }

        dialog.Hide();

        if (cancelled)
        {
            ShowNotification("下載已取消", InfoBarSeverity.Warning);
        }
        else
        {
            ShowNotification("下載完成！", InfoBarSeverity.Success);
        }
    }

    #endregion

    #region Flyout

    private void CloseFlyout_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var parent = button.Parent;
            while (parent != null)
            {
                if (parent is FlyoutPresenter presenter)
                {
                    // 找到 Flyout 並關閉
                    break;
                }
                parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
            }
        }
    }

    private void DatePicker_DatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
    {
        ShowNotification($"選擇的日期: {args.NewDate:yyyy/MM/dd}", InfoBarSeverity.Informational);
    }

    private void TimePicker_TimePicked(TimePickerFlyout sender, TimePickedEventArgs args)
    {
        ShowNotification($"選擇的時間: {args.NewTime:hh\\:mm}", InfoBarSeverity.Informational);
    }

    #endregion

    #region TeachingTip

    private void ShowTeachingTip_Click(object sender, RoutedEventArgs e)
    {
        FeatureTip.IsOpen = true;
    }

    #endregion

    #region InfoBar

    private void ShowSuccessInfo_Click(object sender, RoutedEventArgs e)
    {
        ShowNotification("操作成功完成！", InfoBarSeverity.Success);
    }

    private void ShowWarningInfo_Click(object sender, RoutedEventArgs e)
    {
        ShowNotification("注意：此操作可能會影響效能", InfoBarSeverity.Warning);
    }

    private void ShowErrorInfo_Click(object sender, RoutedEventArgs e)
    {
        ShowNotification("發生錯誤，請稍後再試", InfoBarSeverity.Error);
    }

    private void ShowInfo_Click(object sender, RoutedEventArgs e)
    {
        ShowNotification("這是一則資訊通知", InfoBarSeverity.Informational);
    }

    private void ShowNotification(string message, InfoBarSeverity severity)
    {
        NotificationBar.Message = message;
        NotificationBar.Severity = severity;
        NotificationBar.IsOpen = true;

        // 自動關閉（5秒後）
        DispatcherQueue.TryEnqueue(async () =>
        {
            await Task.Delay(5000);
            NotificationBar.IsOpen = false;
        });
    }

    #endregion
}

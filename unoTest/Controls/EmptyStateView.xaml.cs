using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace unoTest.Controls;

/// <summary>
/// 空狀態視圖控件
/// 用於顯示無資料、錯誤、空搜尋結果等狀態
/// </summary>
public sealed partial class EmptyStateView : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(EmptyStateView),
            new PropertyMetadata("\uE7BA")); // 預設為空白圖標

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(EmptyStateView),
            new PropertyMetadata("沒有資料"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(EmptyStateView),
            new PropertyMetadata(string.Empty));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty ActionTextProperty =
        DependencyProperty.Register(nameof(ActionText), typeof(string), typeof(EmptyStateView),
            new PropertyMetadata(string.Empty));

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public static readonly DependencyProperty ActionCommandProperty =
        DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(EmptyStateView),
            new PropertyMetadata(null));

    public ICommand ActionCommand
    {
        get => (ICommand)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    #endregion

    public EmptyStateView()
    {
        this.InitializeComponent();
    }

    #region Static Presets

    /// <summary>
    /// 建立「無資料」狀態
    /// </summary>
    public static EmptyStateView CreateNoData(string? description = null, string? actionText = null, ICommand? actionCommand = null)
    {
        return new EmptyStateView
        {
            Icon = "\uE7BA",
            Title = "沒有資料",
            Description = description ?? "目前沒有可顯示的資料",
            ActionText = actionText ?? "",
            ActionCommand = actionCommand
        };
    }

    /// <summary>
    /// 建立「搜尋無結果」狀態
    /// </summary>
    public static EmptyStateView CreateNoSearchResults(string? keyword = null)
    {
        return new EmptyStateView
        {
            Icon = "\uE721",
            Title = "找不到結果",
            Description = string.IsNullOrEmpty(keyword) 
                ? "沒有符合搜尋條件的項目" 
                : $"找不到「{keyword}」的相關結果"
        };
    }

    /// <summary>
    /// 建立「錯誤」狀態
    /// </summary>
    public static EmptyStateView CreateError(string? message = null, string? actionText = "重試", ICommand? retryCommand = null)
    {
        return new EmptyStateView
        {
            Icon = "\uE783",
            Title = "發生錯誤",
            Description = message ?? "無法載入資料，請稍後再試",
            ActionText = actionText ?? "",
            ActionCommand = retryCommand
        };
    }

    /// <summary>
    /// 建立「無網路」狀態
    /// </summary>
    public static EmptyStateView CreateOffline(ICommand? retryCommand = null)
    {
        return new EmptyStateView
        {
            Icon = "\uE839",
            Title = "無法連線",
            Description = "請檢查您的網路連線",
            ActionText = "重試",
            ActionCommand = retryCommand
        };
    }

    #endregion
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace unoTest.Controls;

/// <summary>
/// 側邊導航選單控件
/// </summary>
public sealed partial class NavigationMenu : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(NavigationMenu),
            new PropertyMetadata(true, OnIsExpandedChanged));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty MenuItemsProperty =
        DependencyProperty.Register(nameof(MenuItems), typeof(ObservableCollection<MenuItem>), typeof(NavigationMenu),
            new PropertyMetadata(new ObservableCollection<MenuItem>()));

    public ObservableCollection<MenuItem> MenuItems
    {
        get => (ObservableCollection<MenuItem>)GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    public static readonly DependencyProperty SettingsCommandProperty =
        DependencyProperty.Register(nameof(SettingsCommand), typeof(ICommand), typeof(NavigationMenu),
            new PropertyMetadata(null));

    public ICommand SettingsCommand
    {
        get => (ICommand)GetValue(SettingsCommandProperty);
        set => SetValue(SettingsCommandProperty, value);
    }

    public static readonly DependencyProperty ExpandedWidthProperty =
        DependencyProperty.Register(nameof(ExpandedWidth), typeof(double), typeof(NavigationMenu),
            new PropertyMetadata(250.0));

    public double ExpandedWidth
    {
        get => (double)GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }

    public static readonly DependencyProperty CollapsedWidthProperty =
        DependencyProperty.Register(nameof(CollapsedWidth), typeof(double), typeof(NavigationMenu),
            new PropertyMetadata(60.0));

    public double CollapsedWidth
    {
        get => (double)GetValue(CollapsedWidthProperty);
        set => SetValue(CollapsedWidthProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<MenuItem>? MenuItemSelected;
    public event EventHandler<bool>? ExpandedChanged;

    #endregion

    public NavigationMenu()
    {
        this.InitializeComponent();
        UpdateExpandState();
    }

    private void CollapseButton_Click(object sender, RoutedEventArgs e)
    {
        IsExpanded = !IsExpanded;
    }

    private void MenuListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MenuListView.SelectedItem is MenuItem item)
        {
            MenuItemSelected?.Invoke(this, item);
        }
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavigationMenu menu)
        {
            menu.UpdateExpandState();
            menu.ExpandedChanged?.Invoke(menu, menu.IsExpanded);
        }
    }

    private void UpdateExpandState()
    {
        Width = IsExpanded ? ExpandedWidth : CollapsedWidth;
        CollapseIcon.Glyph = IsExpanded ? "\uE76B" : "\uE76C"; // 左箭頭/右箭頭
    }

    /// <summary>
    /// 選擇指定的選單項目
    /// </summary>
    public void SelectItem(string key)
    {
        var item = MenuItems.FirstOrDefault(m => m.Key == key);
        if (item != null)
        {
            MenuListView.SelectedItem = item;
        }
    }
}

/// <summary>
/// 選單項目
/// </summary>
public class MenuItem
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "\uE80F"; // 預設為首頁圖標
    public Type? PageType { get; set; }
    public Type? ViewModelType { get; set; }
    public List<MenuItem> Children { get; set; } = new();
    public bool HasChildren => Children.Count > 0;
    public object? Tag { get; set; }
}

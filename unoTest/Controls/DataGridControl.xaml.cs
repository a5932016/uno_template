using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections;

namespace unoTest.Controls;

/// <summary>
/// 可重複使用的數據表格控件
/// 支援排序、搜尋、分頁、選擇等功能
/// </summary>
public sealed partial class DataGridControl : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DataGridControl),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(DataGridControl),
            new PropertyMetadata(ListViewSelectionMode.Single));

    public ListViewSelectionMode SelectionMode
    {
        get => (ListViewSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(DataGridControl),
            new PropertyMetadata(10, OnPaginationChanged));

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(DataGridControl),
            new PropertyMetadata(1, OnPaginationChanged));

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public static readonly DependencyProperty TotalItemsProperty =
        DependencyProperty.Register(nameof(TotalItems), typeof(int), typeof(DataGridControl),
            new PropertyMetadata(0, OnPaginationChanged));

    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<string>? SearchTextChanged;
    public event EventHandler? RefreshRequested;
    public event EventHandler? ExportRequested;
    public event EventHandler<object?>? SelectionChanged;
    public event EventHandler<int>? PageChanged;

    #endregion

    #region Properties

    public int TotalPages => TotalItems > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 1;
    public bool CanGoFirst => CurrentPage > 1;
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;
    public bool CanGoLast => CurrentPage < TotalPages;

    public object? SelectedItem => DataListView?.SelectedItem;
    public IList<object> SelectedItems => DataListView?.SelectedItems?.Cast<object>().ToList() ?? new List<object>();

    #endregion

    public DataGridControl()
    {
        this.InitializeComponent();
    }

    #region Event Handlers

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SearchTextChanged?.Invoke(this, SearchBox.Text);
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        ExportRequested?.Invoke(this, EventArgs.Empty);
    }

    private void DataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectionChanged?.Invoke(this, DataListView.SelectedItem);
    }

    private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PageSizeComboBox.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int pageSize))
        {
            PageSize = pageSize;
            CurrentPage = 1; // 重置到第一頁
        }
    }

    private void FirstPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (CanGoFirst)
        {
            CurrentPage = 1;
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void PrevPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (CanGoNext)
        {
            CurrentPage++;
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void LastPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (CanGoLast)
        {
            CurrentPage = TotalPages;
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    #endregion

    #region Private Methods

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGridControl control)
        {
            control.UpdatePaginationInfo();
        }
    }

    private static void OnPaginationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGridControl control)
        {
            control.UpdatePaginationInfo();
        }
    }

    private void UpdatePaginationInfo()
    {
        int startIndex = (CurrentPage - 1) * PageSize + 1;
        int endIndex = Math.Min(CurrentPage * PageSize, TotalItems);

        PageInfoText.Text = $"顯示 {startIndex}-{endIndex} 共 {TotalItems} 筆";
        CurrentPageText.Text = $"{CurrentPage} / {TotalPages}";

        // 更新按鈕狀態
        FirstPageButton.IsEnabled = CanGoFirst;
        PrevPageButton.IsEnabled = CanGoPrevious;
        NextPageButton.IsEnabled = CanGoNext;
        LastPageButton.IsEnabled = CanGoLast;
    }

    #endregion

    #region Public Methods

    public void ClearSelection()
    {
        DataListView?.SelectedItems?.Clear();
    }

    public void SelectAll()
    {
        if (DataListView != null)
        {
            DataListView.SelectAll();
        }
    }

    public void RefreshData()
    {
        UpdatePaginationInfo();
    }

    #endregion
}

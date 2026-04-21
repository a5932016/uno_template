using Microsoft.UI.Xaml.Controls;

namespace unoTest.Presentation;

/// <summary>
/// TitleBar 示範頁面
/// </summary>
public sealed partial class TitleBarDemoPage : Page
{
    private readonly TitleBarBindingModel _model = new();

    public TitleBarDemoPage()
    {
        this.InitializeComponent();
        // 將示範用 BindingModel 設為 CustomTitleBar 的 DataContext
        PreviewTitleBar.DataContext = _model;
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _model.Title = TitleTextBox.Text;
    }

    private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _model.UserName = UserNameTextBox.Text;
    }

    private void NotificationSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        int count = (int)e.NewValue;
        _model.NotificationCount = count;
        _model.HasNotifications = count > 0;
        NotificationCountText.Text = $"{count} 則通知";
    }

    private void CanGoBackToggle_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _model.CanGoBack = CanGoBackToggle.IsOn;
    }

    private void ShowUserNameToggle_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _model.ShowUserName = ShowUserNameToggle.IsOn;
        PreviewTitleBar.ShowUserName = ShowUserNameToggle.IsOn;
    }

    private void ShowWindowControlsToggle_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        PreviewTitleBar.ShowWindowControls = ShowWindowControlsToggle.IsOn;
    }

    private void HasNotificationsToggle_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _model.HasNotifications = HasNotificationsToggle.IsOn;
    }

    private void DarkThemeToggle_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _model.IsDarkTheme = DarkThemeToggle.IsOn;
    }
}

/// <summary>
/// TitleBar 示範用的綁定模型
/// 提供 CustomTitleBar 所需的所有屬性與無操作的指令
/// </summary>
public partial class TitleBarBindingModel : ObservableObject
{
    [ObservableProperty] private string _title = "Uno Platform App";
    [ObservableProperty] private string _userName = "訪客";
    [ObservableProperty] private bool _showUserName = true;
    [ObservableProperty] private bool _hasNotifications = false;
    [ObservableProperty] private int _notificationCount = 0;
    [ObservableProperty] private bool _isDarkTheme = false;
    [ObservableProperty] private bool _canGoBack = true;

    public ICommand BackCommand { get; } = new RelayCommand(() => { });
    public ICommand HomeCommand { get; } = new RelayCommand(() => { });
    public ICommand RefreshCommand { get; } = new RelayCommand(() => { });
    public ICommand SearchCommand { get; } = new RelayCommand(() => { });
    public ICommand NotificationCommand { get; } = new RelayCommand(() => { });
    public ICommand ToggleThemeCommand { get; } = new RelayCommand(() => { });
    public ICommand SettingsCommand { get; } = new RelayCommand(() => { });
    public ICommand ProfileCommand { get; } = new RelayCommand(() => { });
    public ICommand AccountSettingsCommand { get; } = new RelayCommand(() => { });
    public ICommand LogoutCommand { get; } = new RelayCommand(() => { });
}

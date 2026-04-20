using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

namespace unoTest.Presentation;

/// <summary>
/// 圖片庫頁面
/// </summary>
public sealed partial class ImageGalleryPage : Page
{
    public ImageGalleryViewModel ViewModel => (ImageGalleryViewModel)DataContext;

    public ImageGalleryPage()
    {
        this.InitializeComponent();
    }

    private async void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.PickAndUploadImagesAsync();
    }

    private void ViewModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModeCombo.SelectedIndex >= 0)
        {
            ViewModel.ViewMode = (ImageViewMode)ViewModeCombo.SelectedIndex;
        }
    }

    private void ImageGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ImageGridView.SelectedItem is ImageItem item)
        {
            ViewModel.SelectedImage = item;
        }
    }

    private void OpenImage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is ImageItem image)
        {
            ViewModel.OpenImage(image);
        }
    }

    private async void DownloadImage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is ImageItem image)
        {
            await ViewModel.DownloadImageAsync(image);
        }
    }

    private void CopyImage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is ImageItem image)
        {
            ViewModel.CopyImageToClipboard(image);
        }
    }

    private async void DeleteImage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is ImageItem image)
        {
            await ViewModel.DeleteImageAsync(image);
        }
    }

    private void ImageProperties_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is ImageItem image)
        {
            ViewModel.ShowImageProperties(image);
        }
    }

    private void FullscreenView_Click(object sender, RoutedEventArgs e)
    {
        FullscreenOverlay.Visibility = Visibility.Visible;
    }

    private void CloseFullscreen_Click(object sender, RoutedEventArgs e)
    {
        FullscreenOverlay.Visibility = Visibility.Collapsed;
    }

    private void EditImage_Click(object sender, RoutedEventArgs e)
    {
        // 開啟圖片編輯功能（待實作）
    }

    private void ShareImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ShareImage();
    }

    private void PreviousImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToPreviousImage();
    }

    private void NextImage_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToNextImage();
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        // 縮放功能由 ScrollViewer 的 ZoomMode 處理
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        // 縮放功能由 ScrollViewer 的 ZoomMode 處理
    }

    private void ZoomToFit_Click(object sender, RoutedEventArgs e)
    {
        // 重置縮放
    }
}

/// <summary>
/// 圖片庫 ViewModel
/// </summary>
public partial class ImageGalleryViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private ObservableCollection<ImageItem> _images = new();

    [ObservableProperty]
    private ImageItem? _selectedImage;

    [ObservableProperty]
    private ImageViewMode _viewMode = ImageViewMode.Grid;

    [ObservableProperty]
    private bool _isLoading;

    public bool HasNoImages => Images.Count == 0;

    public ImageGalleryViewModel(INavigator navigator)
    {
        _navigator = navigator;
        LoadSampleImages();
    }

    private void LoadSampleImages()
    {
        // 載入範例圖片
        Images = new ObservableCollection<ImageItem>
        {
            new ImageItem
            {
                Id = "1",
                FileName = "sunset.jpg",
                ThumbnailUrl = "ms-appx:///Assets/Icons/icon.png",
                FullUrl = "ms-appx:///Assets/Icons/icon.png",
                FileSize = 2048576,
                Width = 1920,
                Height = 1080,
                Format = "JPEG",
                CreatedAt = DateTime.Now.AddDays(-5),
                ModifiedAt = DateTime.Now.AddDays(-2)
            },
            new ImageItem
            {
                Id = "2",
                FileName = "mountain.png",
                ThumbnailUrl = "ms-appx:///Assets/Icons/icon.png",
                FullUrl = "ms-appx:///Assets/Icons/icon.png",
                FileSize = 3145728,
                Width = 2560,
                Height = 1440,
                Format = "PNG",
                CreatedAt = DateTime.Now.AddDays(-10),
                ModifiedAt = DateTime.Now.AddDays(-10)
            },
            new ImageItem
            {
                Id = "3",
                FileName = "portrait.jpg",
                ThumbnailUrl = "ms-appx:///Assets/Icons/icon.png",
                FullUrl = "ms-appx:///Assets/Icons/icon.png",
                FileSize = 1572864,
                Width = 1200,
                Height = 1600,
                Format = "JPEG",
                CreatedAt = DateTime.Now.AddDays(-3),
                ModifiedAt = DateTime.Now.AddDays(-1)
            }
        };
        OnPropertyChanged(nameof(HasNoImages));
    }

    public async Task PickAndUploadImagesAsync()
    {
        try
        {
#if WINDOWS
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".webp");

            // 初始化 picker（Windows 需要）
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var files = await picker.PickMultipleFilesAsync();
            if (files != null && files.Count > 0)
            {
                IsLoading = true;
                foreach (var file in files)
                {
                    var imageItem = new ImageItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        FileName = file.Name,
                        ThumbnailUrl = file.Path,
                        FullUrl = file.Path,
                        Format = file.FileType.TrimStart('.').ToUpper(),
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now
                    };

                    // 取得檔案大小
                    var props = await file.GetBasicPropertiesAsync();
                    imageItem.FileSize = (long)props.Size;

                    Images.Add(imageItem);
                }
                OnPropertyChanged(nameof(HasNoImages));
            }
#else
            // 其他平台的實作
            await Task.CompletedTask;
#endif
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OpenImage(ImageItem image)
    {
        SelectedImage = image;
    }

    public async Task DownloadImageAsync(ImageItem image)
    {
        // 實作下載功能
        await Task.CompletedTask;
    }

    public void CopyImageToClipboard(ImageItem image)
    {
        // 實作複製到剪貼簿
    }

    public async Task DeleteImageAsync(ImageItem image)
    {
        Images.Remove(image);
        if (SelectedImage == image)
        {
            SelectedImage = Images.FirstOrDefault();
        }
        OnPropertyChanged(nameof(HasNoImages));
        await Task.CompletedTask;
    }

    public void ShowImageProperties(ImageItem image)
    {
        SelectedImage = image;
    }

    public void ShareImage()
    {
        // 實作分享功能
    }

    public void NavigateToPreviousImage()
    {
        if (SelectedImage == null || Images.Count == 0) return;

        var currentIndex = Images.IndexOf(SelectedImage);
        if (currentIndex > 0)
        {
            SelectedImage = Images[currentIndex - 1];
        }
        else
        {
            SelectedImage = Images[Images.Count - 1]; // 循環到最後
        }
    }

    public void NavigateToNextImage()
    {
        if (SelectedImage == null || Images.Count == 0) return;

        var currentIndex = Images.IndexOf(SelectedImage);
        if (currentIndex < Images.Count - 1)
        {
            SelectedImage = Images[currentIndex + 1];
        }
        else
        {
            SelectedImage = Images[0]; // 循環到開頭
        }
    }
}

/// <summary>
/// 圖片項目
/// </summary>
public class ImageItem : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string FileSizeText => FormatFileSize(FileSize);
    public string DimensionsText => $"{Width} x {Height}";
    public string CreatedAtText => CreatedAt.ToString("yyyy/MM/dd HH:mm");
    public string ModifiedAtText => ModifiedAt.ToString("yyyy/MM/dd HH:mm");

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}

/// <summary>
/// 檢視模式
/// </summary>
public enum ImageViewMode
{
    Grid,
    List,
    Details
}

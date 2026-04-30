using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace unoTest.Controls;

public sealed partial class ImageAnnotationEditorControl : UserControl
{
    private const double MinimumShapeSize = 6;
    private const double PolygonCloseDistance = 14;
    private const double HandleSize = 12;

    private readonly List<AnnotationItem> _annotations = [];
    private readonly Dictionary<Shape, AnnotationItem> _shapeLookup = [];
    private readonly List<FrameworkElement> _adornerElements = [];

    private bool _isPanning;
    private Point _panStartPoint;
    private double _startHorizontalOffset;
    private double _startVerticalOffset;

    private Point _rectangleStartPoint;
    private Rectangle? _draftRectangle;
    private Windows.UI.Color _draftRectangleColor;

    private readonly List<Point> _polygonPoints = [];
    private Polyline? _draftPolyline;
    private Line? _polygonGuideLine;
    private Windows.UI.Color _draftPolygonColor;

    private AnnotationItem? _selectedAnnotation;

    private AnnotationItem? _draggingAnnotation;
    private Point _dragStartPoint;
    private double _dragStartLeft;
    private double _dragStartTop;
    private double _dragStartWidth;
    private double _dragStartHeight;
    private List<Point>? _dragStartPolygonPoints;

    private AnnotationHandleTag? _activeHandleTag;
    private Point _resizeDragStartPoint;
    private double _resizeStartLeft;
    private double _resizeStartTop;
    private double _resizeStartWidth;
    private double _resizeStartHeight;
    private List<Point>? _resizeStartPolygonPoints;

    public static readonly DependencyProperty ShowToolbarProperty =
        DependencyProperty.Register(
            nameof(ShowToolbar),
            typeof(bool),
            typeof(ImageAnnotationEditorControl),
            new PropertyMetadata(true, OnShowToolbarChanged));

    public ImageAnnotationEditorViewModel ViewModel { get; } = new();

    public bool ShowToolbar
    {
        get => (bool)GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    public ImageAnnotationEditorControl()
    {
        this.InitializeComponent();
        InitializeCanvas(ImageAnnotationEditorViewModel.DefaultCanvasWidth, ImageAnnotationEditorViewModel.DefaultCanvasHeight);
        ApplyZoom(ViewModel.ZoomFactor, resetOffsets: true);
        ApplyToolbarVisibility();
    }

    private static void OnShowToolbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageAnnotationEditorControl control)
        {
            control.ApplyToolbarVisibility();
        }
    }

    public bool SetTool(AnnotationTool tool)
    {
        if (ViewModel.ActiveTool == tool)
        {
            return true;
        }

        ViewModel.ActiveTool = tool;
        SyncToolComboBox(tool);

        if (tool != AnnotationTool.Polygon)
        {
            if (_polygonPoints.Count >= 3)
            {
                FinishActivePolygon();
            }
            else
            {
                CancelInProgressPolygon();
            }
        }

        UpdateHint(ImageAnnotationEditorViewModel.GetToolHint(tool));
        return true;
    }

    public bool TrySetTool(string rawTool)
    {
        if (!Enum.TryParse<AnnotationTool>(rawTool, out var tool))
        {
            return false;
        }

        return SetTool(tool);
    }

    public bool TrySetColor(string colorKey)
    {
        if (!ViewModel.TrySetColor(colorKey))
        {
            return false;
        }

        SyncColorComboBox(colorKey);
        return true;
    }

    public void SetStrokeColor(Windows.UI.Color color)
    {
        ViewModel.ActiveColor = color;
        SyncColorComboBox(null);
    }

    public float SetZoom(float zoomFactor)
    {
        ApplyZoom(zoomFactor, resetOffsets: false);
        return ViewModel.ZoomFactor;
    }

    public float ZoomIn(float factor = 1.25f)
    {
        if (factor <= 1f)
        {
            factor = 1.25f;
        }

        ApplyZoom(ViewModel.ZoomFactor * factor, resetOffsets: false);
        return ViewModel.ZoomFactor;
    }

    public float ZoomOut(float factor = 0.8f)
    {
        if (factor >= 1f || factor <= 0f)
        {
            factor = 0.8f;
        }

        ApplyZoom(ViewModel.ZoomFactor * factor, resetOffsets: false);
        return ViewModel.ZoomFactor;
    }

    public bool UndoLastAction()
    {
        if (_draftRectangle is not null)
        {
            OverlayCanvas.Children.Remove(_draftRectangle);
            _draftRectangle = null;
            UpdateHint("已取消尚未完成的方框。");
            return true;
        }

        if (_polygonPoints.Count > 0 || _draftPolyline is not null || _polygonGuideLine is not null)
        {
            CancelInProgressPolygon();
            UpdateHint("已取消尚未完成的多邊形。");
            return true;
        }

        var target = _selectedAnnotation ?? _annotations.LastOrDefault();
        if (target is null)
        {
            UpdateHint("目前沒有可取消的標註。");
            return false;
        }

        RemoveAnnotation(target);
        SelectAnnotation(null);
        UpdateHint("已取消一筆標註。");
        return true;
    }

    public AnnotationSelectionInfo? GetCurrentSelection()
    {
        if (_selectedAnnotation is null)
        {
            return null;
        }

        return CreateSelectionInfo(_selectedAnnotation);
    }

    public async Task<bool> LoadImageAsync(StorageFile file)
    {
        using var stream = await file.OpenReadAsync();
        return await LoadImageAsync(stream, file.Name);
    }

    public async Task<bool> LoadImageAsync(IRandomAccessStream stream, string? sourceFileName = null)
    {
        if (stream is null)
        {
            return false;
        }

        stream.Seek(0);
        var bitmap = new BitmapImage();
        await bitmap.SetSourceAsync(stream);

        if (bitmap.PixelWidth <= 0 || bitmap.PixelHeight <= 0)
        {
            return false;
        }

        LoadedImage.Source = bitmap;
        InitializeCanvas(bitmap.PixelWidth, bitmap.PixelHeight);
        ClearAllAnnotations();
        EmptyHintContainer.Visibility = Visibility.Collapsed;
        ViewModel.SetSourceFileName(sourceFileName ?? ViewModel.SourceFileName);
        ApplyZoom(1f, resetOffsets: true);
        UpdateHint($"已載入 {ViewModel.SourceFileName}，可開始標註。已支援拖曳與調整。");
        return true;
    }

    private async void LoadImageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".webp");

#if WINDOWS
            if (App.CurrentMainWindow is not null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentMainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }
#endif

            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                UpdateHint("已取消載入圖片。");
                return;
            }

            var isLoaded = await LoadImageAsync(file);
            if (!isLoaded)
            {
                await ShowMessageAsync("讀取失敗", "無法取得圖片尺寸，請改用其他圖片。");
                return;
            }
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("載入失敗", ex.Message);
        }
    }

    private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
    {
        if (LoadedImage.Source is null)
        {
            await ShowMessageAsync("提醒", "請先載入圖片後再另存新檔。");
            return;
        }

        if (_polygonPoints.Count >= 3)
        {
            FinishActivePolygon();
        }

        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = $"{ViewModel.SourceFileName}-annotated"
            };
            picker.FileTypeChoices.Add("PNG 圖片", [".png"]);

#if WINDOWS
            if (App.CurrentMainWindow is not null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentMainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }
#endif

            var targetFile = await picker.PickSaveFileAsync();
            if (targetFile is null)
            {
                UpdateHint("已取消另存新檔。");
                return;
            }

            var previousSelection = _selectedAnnotation;
            ClearAdorners();

            var renderTarget = new RenderTargetBitmap();
            await renderTarget.RenderAsync(EditorSurface);
            var pixels = (await renderTarget.GetPixelsAsync()).ToArray();

            using var outputStream = await targetFile.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                (uint)renderTarget.PixelWidth,
                (uint)renderTarget.PixelHeight,
                96,
                96,
                pixels);
            await encoder.FlushAsync();

            if (previousSelection is not null)
            {
                SelectAnnotation(previousSelection);
            }

            UpdateHint($"已儲存：{targetFile.Name}");
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("儲存失敗", ex.Message);
        }
    }

    private void ToolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ToolComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string rawTool)
        {
            return;
        }

        if (!Enum.TryParse<AnnotationTool>(rawTool, out var selectedTool))
        {
            return;
        }

        SetTool(selectedTool);
    }

    private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ColorComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string colorKey)
        {
            return;
        }

        if (TrySetColor(colorKey))
        {
            UpdateHint($"已切換線條顏色：{item.Content}");
        }
    }

    private void FinishPolygonButton_Click(object sender, RoutedEventArgs e)
    {
        FinishActivePolygon();
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        ZoomOut();
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        ZoomIn();
    }

    private void OverlayCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (LoadedImage.Source is null)
        {
            UpdateHint("請先載入圖片。");
            return;
        }

        var pointOnCanvas = e.GetCurrentPoint(OverlayCanvas).Position;

        switch (ViewModel.ActiveTool)
        {
            case AnnotationTool.MoveImage:
                _isPanning = true;
                _panStartPoint = e.GetCurrentPoint(this).Position;
                _startHorizontalOffset = EditorScrollViewer.HorizontalOffset;
                _startVerticalOffset = EditorScrollViewer.VerticalOffset;
                OverlayCanvas.CapturePointer(e.Pointer);
                break;

            case AnnotationTool.Rectangle:
                SelectAnnotation(null);
                _rectangleStartPoint = ClampPointToCanvas(pointOnCanvas);
                _draftRectangleColor = ViewModel.ActiveColor;
                _draftRectangle = new Rectangle
                {
                    Stroke = new SolidColorBrush(_draftRectangleColor),
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
                };
                OverlayCanvas.Children.Add(_draftRectangle);
                Canvas.SetLeft(_draftRectangle, _rectangleStartPoint.X);
                Canvas.SetTop(_draftRectangle, _rectangleStartPoint.Y);
                OverlayCanvas.CapturePointer(e.Pointer);
                break;

            case AnnotationTool.Polygon:
                AddPolygonPoint(ClampPointToCanvas(pointOnCanvas));
                break;
        }
    }

    private void OverlayCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var pointOnCanvas = e.GetCurrentPoint(OverlayCanvas).Position;

        if (ViewModel.ActiveTool == AnnotationTool.MoveImage && _isPanning)
        {
            var pointOnPage = e.GetCurrentPoint(this).Position;
            var deltaX = pointOnPage.X - _panStartPoint.X;
            var deltaY = pointOnPage.Y - _panStartPoint.Y;
            var nextX = Math.Max(0, _startHorizontalOffset - deltaX);
            var nextY = Math.Max(0, _startVerticalOffset - deltaY);
            EditorScrollViewer.ChangeView(nextX, nextY, null, true);
            return;
        }

        if (ViewModel.ActiveTool == AnnotationTool.Rectangle && _draftRectangle is not null)
        {
            UpdateRectangle(_rectangleStartPoint, ClampPointToCanvas(pointOnCanvas), _draftRectangle);
            return;
        }

        if (ViewModel.ActiveTool == AnnotationTool.Polygon && _polygonGuideLine is not null && _polygonPoints.Count > 0)
        {
            var anchor = _polygonPoints[^1];
            var guidePoint = ClampPointToCanvas(pointOnCanvas);
            _polygonGuideLine.X1 = anchor.X;
            _polygonGuideLine.Y1 = anchor.Y;
            _polygonGuideLine.X2 = guidePoint.X;
            _polygonGuideLine.Y2 = guidePoint.Y;
        }
    }

    private void OverlayCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;
            OverlayCanvas.ReleasePointerCapture(e.Pointer);
            UpdateHint("移動完成，可繼續操作。");
        }

        if (_draftRectangle is not null)
        {
            OverlayCanvas.ReleasePointerCapture(e.Pointer);
            if (_draftRectangle.Width < MinimumShapeSize || _draftRectangle.Height < MinimumShapeSize)
            {
                OverlayCanvas.Children.Remove(_draftRectangle);
            }
            else
            {
                var createdItem = RegisterAnnotation(_draftRectangle, AnnotationShapeKind.Rectangle);
                SelectAnnotation(createdItem);
                UpdateHint("方框已建立，可直接拖曳移動或拉角點調整大小。");
            }

            _draftRectangle = null;
        }
    }

    private void OverlayCanvas_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        _isPanning = false;

        if (_draftRectangle is not null)
        {
            OverlayCanvas.Children.Remove(_draftRectangle);
            _draftRectangle = null;
        }
    }

    private void AnnotationShape_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Shape shape || !_shapeLookup.TryGetValue(shape, out var item))
        {
            return;
        }

        if (ViewModel.ActiveTool == AnnotationTool.MoveImage)
        {
            return;
        }

        if (ViewModel.ActiveTool == AnnotationTool.Polygon && _draftPolyline is not null)
        {
            return;
        }

        SelectAnnotation(item);

        _draggingAnnotation = item;
        _dragStartPoint = e.GetCurrentPoint(OverlayCanvas).Position;

        if (item.Kind == AnnotationShapeKind.Rectangle && item.ShapeElement is Rectangle rectangle)
        {
            _dragStartLeft = GetCanvasLeft(rectangle);
            _dragStartTop = GetCanvasTop(rectangle);
            _dragStartWidth = rectangle.Width;
            _dragStartHeight = rectangle.Height;
        }
        else if (item.Kind == AnnotationShapeKind.Polygon && item.ShapeElement is Polygon polygon)
        {
            _dragStartPolygonPoints = polygon.Points.Select(p => new Point(p.X, p.Y)).ToList();
        }

        shape.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void AnnotationShape_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_draggingAnnotation is null)
        {
            return;
        }

        var currentPoint = e.GetCurrentPoint(OverlayCanvas).Position;
        var deltaX = currentPoint.X - _dragStartPoint.X;
        var deltaY = currentPoint.Y - _dragStartPoint.Y;

        if (_draggingAnnotation.Kind == AnnotationShapeKind.Rectangle && _draggingAnnotation.ShapeElement is Rectangle rectangle)
        {
            var nextLeft = Clamp(_dragStartLeft + deltaX, 0, GetCanvasWidth() - _dragStartWidth);
            var nextTop = Clamp(_dragStartTop + deltaY, 0, GetCanvasHeight() - _dragStartHeight);

            Canvas.SetLeft(rectangle, nextLeft);
            Canvas.SetTop(rectangle, nextTop);
            return;
        }

        if (_draggingAnnotation.Kind == AnnotationShapeKind.Polygon
            && _draggingAnnotation.ShapeElement is Polygon polygon
            && _dragStartPolygonPoints is not null)
        {
            var (fixedDeltaX, fixedDeltaY) = ClampPolygonDelta(_dragStartPolygonPoints, deltaX, deltaY);

            for (var i = 0; i < polygon.Points.Count; i++)
            {
                var originalPoint = _dragStartPolygonPoints[i];
                polygon.Points[i] = new Point(originalPoint.X + fixedDeltaX, originalPoint.Y + fixedDeltaY);
            }
        }
    }

    private void AnnotationShape_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Shape shape)
        {
            shape.ReleasePointerCapture(e.Pointer);
        }

        if (_draggingAnnotation is not null)
        {
            _draggingAnnotation = null;
            _dragStartPolygonPoints = null;
            RenderAdorners();
            UpdateHint("已更新標註位置。");
        }
    }

    private void AnnotationShape_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        _draggingAnnotation = null;
        _dragStartPolygonPoints = null;
        RenderAdorners();
    }

    private void Handle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border handle || handle.Tag is not AnnotationHandleTag tag)
        {
            return;
        }

        _activeHandleTag = tag;
        _resizeDragStartPoint = e.GetCurrentPoint(OverlayCanvas).Position;

        if (tag.Owner.Kind == AnnotationShapeKind.Rectangle && tag.Owner.ShapeElement is Rectangle rectangle)
        {
            _resizeStartLeft = GetCanvasLeft(rectangle);
            _resizeStartTop = GetCanvasTop(rectangle);
            _resizeStartWidth = rectangle.Width;
            _resizeStartHeight = rectangle.Height;
        }
        else if (tag.Owner.Kind == AnnotationShapeKind.Polygon && tag.Owner.ShapeElement is Polygon polygon)
        {
            _resizeStartPolygonPoints = polygon.Points.Select(p => new Point(p.X, p.Y)).ToList();
        }

        handle.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void Handle_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_activeHandleTag is null)
        {
            return;
        }

        var currentPoint = e.GetCurrentPoint(OverlayCanvas).Position;
        var deltaX = currentPoint.X - _resizeDragStartPoint.X;
        var deltaY = currentPoint.Y - _resizeDragStartPoint.Y;

        if (_activeHandleTag.Owner.Kind == AnnotationShapeKind.Rectangle
            && _activeHandleTag.Owner.ShapeElement is Rectangle rectangle
            && _activeHandleTag.RectangleHandle.HasValue)
        {
            ResizeRectangle(rectangle, _activeHandleTag.RectangleHandle.Value, deltaX, deltaY);
            return;
        }

        if (_activeHandleTag.Owner.Kind == AnnotationShapeKind.Polygon
            && _activeHandleTag.Owner.ShapeElement is Polygon polygon
            && _activeHandleTag.PolygonVertexIndex.HasValue
            && _resizeStartPolygonPoints is not null)
        {
            var vertexIndex = _activeHandleTag.PolygonVertexIndex.Value;
            if (vertexIndex >= 0 && vertexIndex < _resizeStartPolygonPoints.Count)
            {
                var startPoint = _resizeStartPolygonPoints[vertexIndex];
                var movedPoint = ClampPointToCanvas(new Point(startPoint.X + deltaX, startPoint.Y + deltaY));
                polygon.Points[vertexIndex] = movedPoint;
            }
        }
    }

    private void Handle_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border handle)
        {
            handle.ReleasePointerCapture(e.Pointer);
        }

        if (_activeHandleTag is not null)
        {
            _activeHandleTag = null;
            _resizeStartPolygonPoints = null;
            RenderAdorners();
            UpdateHint("已更新標註尺寸或節點。");
        }
    }

    private void Handle_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        _activeHandleTag = null;
        _resizeStartPolygonPoints = null;
        RenderAdorners();
    }

    private void AddPolygonPoint(Point point)
    {
        if (_draftPolyline is null)
        {
            SelectAnnotation(null);

            _draftPolygonColor = ViewModel.ActiveColor;
            _draftPolyline = new Polyline
            {
                Stroke = new SolidColorBrush(_draftPolygonColor),
                StrokeThickness = 2
            };

            _polygonGuideLine = new Line
            {
                Stroke = new SolidColorBrush(_draftPolygonColor),
                StrokeThickness = 1,
                StrokeDashArray = [2, 2]
            };

            OverlayCanvas.Children.Add(_draftPolyline);
            OverlayCanvas.Children.Add(_polygonGuideLine);
        }

        if (_polygonPoints.Count >= 3 && IsNear(point, _polygonPoints[0], PolygonCloseDistance))
        {
            FinishActivePolygon();
            return;
        }

        _polygonPoints.Add(point);
        _draftPolyline.Points.Add(point);
        FinishPolygonButton.IsEnabled = _polygonPoints.Count >= 3;
        UpdateHint("多邊形模式：持續點擊加點，回到起點或按完成按鈕結束。");
    }

    private void FinishActivePolygon()
    {
        if (_draftPolyline is null)
        {
            FinishPolygonButton.IsEnabled = false;
            return;
        }

        if (_polygonPoints.Count < 3)
        {
            CancelInProgressPolygon();
            UpdateHint("多邊形至少需要 3 個點，已取消。");
            return;
        }

        OverlayCanvas.Children.Remove(_draftPolyline);
        if (_polygonGuideLine is not null)
        {
            OverlayCanvas.Children.Remove(_polygonGuideLine);
        }

        var polygon = new Polygon
        {
            Stroke = new SolidColorBrush(_draftPolygonColor),
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
        };

        foreach (var point in _polygonPoints)
        {
            polygon.Points.Add(point);
        }

        OverlayCanvas.Children.Add(polygon);
        var createdItem = RegisterAnnotation(polygon, AnnotationShapeKind.Polygon);
        SelectAnnotation(createdItem);

        _draftPolyline = null;
        _polygonGuideLine = null;
        _polygonPoints.Clear();
        FinishPolygonButton.IsEnabled = false;
        UpdateHint("多邊形已建立，可拖曳外框移動，或拉頂點調整線條。");
    }

    private void CancelInProgressPolygon()
    {
        if (_draftPolyline is not null)
        {
            OverlayCanvas.Children.Remove(_draftPolyline);
        }

        if (_polygonGuideLine is not null)
        {
            OverlayCanvas.Children.Remove(_polygonGuideLine);
        }

        _draftPolyline = null;
        _polygonGuideLine = null;
        _polygonPoints.Clear();
        FinishPolygonButton.IsEnabled = false;
    }

    private AnnotationItem RegisterAnnotation(Shape shape, AnnotationShapeKind kind)
    {
        shape.PointerPressed += AnnotationShape_PointerPressed;
        shape.PointerMoved += AnnotationShape_PointerMoved;
        shape.PointerReleased += AnnotationShape_PointerReleased;
        shape.PointerCanceled += AnnotationShape_PointerCanceled;

        var item = new AnnotationItem
        {
            Kind = kind,
            ShapeElement = shape
        };

        _annotations.Add(item);
        _shapeLookup[shape] = item;

        return item;
    }

    private void SelectAnnotation(AnnotationItem? item)
    {
        _selectedAnnotation = item;

        foreach (var annotation in _annotations)
        {
            annotation.ShapeElement.StrokeThickness = annotation == _selectedAnnotation ? 3 : 2;
        }

        RenderAdorners();
    }

    private void RenderAdorners()
    {
        ClearAdorners();

        if (_selectedAnnotation is null)
        {
            return;
        }

        var bounds = GetBounds(_selectedAnnotation);
        if (bounds.Width < 1 || bounds.Height < 1)
        {
            return;
        }

        var outline = new Rectangle
        {
            Width = bounds.Width,
            Height = bounds.Height,
            Stroke = new SolidColorBrush(Microsoft.UI.Colors.DeepSkyBlue),
            StrokeThickness = 1,
            StrokeDashArray = [4, 2],
            Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            IsHitTestVisible = false
        };
        Canvas.SetLeft(outline, bounds.X);
        Canvas.SetTop(outline, bounds.Y);
        AddAdornerElement(outline);

        if (_selectedAnnotation.Kind == AnnotationShapeKind.Rectangle)
        {
            AddRectangleResizeHandle(new Point(bounds.X, bounds.Y), RectangleHandle.TopLeft);
            AddRectangleResizeHandle(new Point(bounds.X + bounds.Width, bounds.Y), RectangleHandle.TopRight);
            AddRectangleResizeHandle(new Point(bounds.X, bounds.Y + bounds.Height), RectangleHandle.BottomLeft);
            AddRectangleResizeHandle(new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height), RectangleHandle.BottomRight);
            return;
        }

        if (_selectedAnnotation.Kind == AnnotationShapeKind.Polygon && _selectedAnnotation.ShapeElement is Polygon polygon)
        {
            for (var i = 0; i < polygon.Points.Count; i++)
            {
                AddPolygonVertexHandle(polygon.Points[i], i);
            }
        }
    }

    private void AddRectangleResizeHandle(Point point, RectangleHandle handleType)
    {
        var handle = CreateHandle(point);
        handle.Tag = new AnnotationHandleTag
        {
            Owner = _selectedAnnotation!,
            RectangleHandle = handleType
        };
    }

    private void AddPolygonVertexHandle(Point point, int vertexIndex)
    {
        var handle = CreateHandle(point);
        handle.Tag = new AnnotationHandleTag
        {
            Owner = _selectedAnnotation!,
            PolygonVertexIndex = vertexIndex
        };
    }

    private Border CreateHandle(Point point)
    {
        var handle = new Border
        {
            Width = HandleSize,
            Height = HandleSize,
            Background = new SolidColorBrush(Microsoft.UI.Colors.White),
            BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DeepSkyBlue),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2)
        };

        Canvas.SetLeft(handle, point.X - HandleSize / 2);
        Canvas.SetTop(handle, point.Y - HandleSize / 2);

        handle.PointerPressed += Handle_PointerPressed;
        handle.PointerMoved += Handle_PointerMoved;
        handle.PointerReleased += Handle_PointerReleased;
        handle.PointerCanceled += Handle_PointerCanceled;

        AddAdornerElement(handle);
        return handle;
    }

    private void AddAdornerElement(FrameworkElement element)
    {
        _adornerElements.Add(element);
        AdornerCanvas.Children.Add(element);
    }

    private void ClearAdorners()
    {
        foreach (var element in _adornerElements)
        {
            if (element is Border handle)
            {
                handle.PointerPressed -= Handle_PointerPressed;
                handle.PointerMoved -= Handle_PointerMoved;
                handle.PointerReleased -= Handle_PointerReleased;
                handle.PointerCanceled -= Handle_PointerCanceled;
            }
        }

        _adornerElements.Clear();
        AdornerCanvas.Children.Clear();
    }

    private void ResizeRectangle(Rectangle rectangle, RectangleHandle handle, double deltaX, double deltaY)
    {
        var left = _resizeStartLeft;
        var top = _resizeStartTop;
        var right = _resizeStartLeft + _resizeStartWidth;
        var bottom = _resizeStartTop + _resizeStartHeight;

        switch (handle)
        {
            case RectangleHandle.TopLeft:
                left += deltaX;
                top += deltaY;
                break;
            case RectangleHandle.TopRight:
                right += deltaX;
                top += deltaY;
                break;
            case RectangleHandle.BottomLeft:
                left += deltaX;
                bottom += deltaY;
                break;
            case RectangleHandle.BottomRight:
                right += deltaX;
                bottom += deltaY;
                break;
        }

        var maxWidth = GetCanvasWidth();
        var maxHeight = GetCanvasHeight();

        left = Clamp(left, 0, maxWidth - MinimumShapeSize);
        top = Clamp(top, 0, maxHeight - MinimumShapeSize);
        right = Clamp(right, left + MinimumShapeSize, maxWidth);
        bottom = Clamp(bottom, top + MinimumShapeSize, maxHeight);

        Canvas.SetLeft(rectangle, left);
        Canvas.SetTop(rectangle, top);
        rectangle.Width = Math.Max(MinimumShapeSize, right - left);
        rectangle.Height = Math.Max(MinimumShapeSize, bottom - top);
    }

    private (double deltaX, double deltaY) ClampPolygonDelta(IReadOnlyCollection<Point> points, double deltaX, double deltaY)
    {
        var minX = points.Min(point => point.X);
        var maxX = points.Max(point => point.X);
        var minY = points.Min(point => point.Y);
        var maxY = points.Max(point => point.Y);

        if (minX + deltaX < 0)
        {
            deltaX = -minX;
        }

        if (maxX + deltaX > GetCanvasWidth())
        {
            deltaX = GetCanvasWidth() - maxX;
        }

        if (minY + deltaY < 0)
        {
            deltaY = -minY;
        }

        if (maxY + deltaY > GetCanvasHeight())
        {
            deltaY = GetCanvasHeight() - maxY;
        }

        return (deltaX, deltaY);
    }

    private void ClearAllAnnotations()
    {
        CancelInProgressPolygon();

        foreach (var annotation in _annotations)
        {
            annotation.ShapeElement.PointerPressed -= AnnotationShape_PointerPressed;
            annotation.ShapeElement.PointerMoved -= AnnotationShape_PointerMoved;
            annotation.ShapeElement.PointerReleased -= AnnotationShape_PointerReleased;
            annotation.ShapeElement.PointerCanceled -= AnnotationShape_PointerCanceled;
        }

        _annotations.Clear();
        _shapeLookup.Clear();
        _selectedAnnotation = null;
        _draggingAnnotation = null;
        _activeHandleTag = null;
        _draftRectangle = null;
        OverlayCanvas.Children.Clear();
        ClearAdorners();
    }

    private void RemoveAnnotation(AnnotationItem annotation)
    {
        annotation.ShapeElement.PointerPressed -= AnnotationShape_PointerPressed;
        annotation.ShapeElement.PointerMoved -= AnnotationShape_PointerMoved;
        annotation.ShapeElement.PointerReleased -= AnnotationShape_PointerReleased;
        annotation.ShapeElement.PointerCanceled -= AnnotationShape_PointerCanceled;

        _annotations.Remove(annotation);
        _shapeLookup.Remove(annotation.ShapeElement);
        OverlayCanvas.Children.Remove(annotation.ShapeElement);

        if (_selectedAnnotation == annotation)
        {
            _selectedAnnotation = null;
        }

        if (_draggingAnnotation == annotation)
        {
            _draggingAnnotation = null;
        }
    }

    private void InitializeCanvas(double width, double height)
    {
        ViewModel.SetCanvasSize(width, height);

        EditorSurface.Width = ViewModel.BaseCanvasWidth;
        EditorSurface.Height = ViewModel.BaseCanvasHeight;
        LoadedImage.Width = ViewModel.BaseCanvasWidth;
        LoadedImage.Height = ViewModel.BaseCanvasHeight;
        OverlayCanvas.Width = ViewModel.BaseCanvasWidth;
        OverlayCanvas.Height = ViewModel.BaseCanvasHeight;
        AdornerCanvas.Width = ViewModel.BaseCanvasWidth;
        AdornerCanvas.Height = ViewModel.BaseCanvasHeight;

        ZoomHost.Width = ViewModel.BaseCanvasWidth * ViewModel.ZoomFactor;
        ZoomHost.Height = ViewModel.BaseCanvasHeight * ViewModel.ZoomFactor;
    }

    private void ApplyZoom(float zoomFactor, bool resetOffsets)
    {
        var zoom = ViewModel.SetZoom(zoomFactor);
        EditorScaleTransform.ScaleX = zoom;
        EditorScaleTransform.ScaleY = zoom;
        ZoomHost.Width = ViewModel.BaseCanvasWidth * zoom;
        ZoomHost.Height = ViewModel.BaseCanvasHeight * zoom;

        if (resetOffsets)
        {
            EditorScrollViewer.ChangeView(0, 0, null, true);
        }
    }

    private Rect GetBounds(AnnotationItem item)
    {
        if (item.Kind == AnnotationShapeKind.Rectangle && item.ShapeElement is Rectangle rectangle)
        {
            return new Rect(GetCanvasLeft(rectangle), GetCanvasTop(rectangle), rectangle.Width, rectangle.Height);
        }

        if (item.Kind == AnnotationShapeKind.Polygon && item.ShapeElement is Polygon polygon && polygon.Points.Count > 0)
        {
            var minX = polygon.Points.Min(point => point.X);
            var maxX = polygon.Points.Max(point => point.X);
            var minY = polygon.Points.Min(point => point.Y);
            var maxY = polygon.Points.Max(point => point.Y);
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        return Rect.Empty;
    }

    private static void UpdateRectangle(Point start, Point current, Rectangle rectangle)
    {
        var x = Math.Min(start.X, current.X);
        var y = Math.Min(start.Y, current.Y);
        var width = Math.Abs(current.X - start.X);
        var height = Math.Abs(current.Y - start.Y);

        Canvas.SetLeft(rectangle, x);
        Canvas.SetTop(rectangle, y);
        rectangle.Width = width;
        rectangle.Height = height;
    }

    private Point ClampPointToCanvas(Point point)
    {
        return new Point(
            Clamp(point.X, 0, GetCanvasWidth()),
            Clamp(point.Y, 0, GetCanvasHeight()));
    }

    private double GetCanvasWidth() => OverlayCanvas.Width > 0 ? OverlayCanvas.Width : OverlayCanvas.ActualWidth;

    private double GetCanvasHeight() => OverlayCanvas.Height > 0 ? OverlayCanvas.Height : OverlayCanvas.ActualHeight;

    private static double GetCanvasLeft(FrameworkElement element)
    {
        var left = Canvas.GetLeft(element);
        return double.IsNaN(left) ? 0 : left;
    }

    private static double GetCanvasTop(FrameworkElement element)
    {
        var top = Canvas.GetTop(element);
        return double.IsNaN(top) ? 0 : top;
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    private static bool IsNear(Point current, Point target, double threshold)
    {
        var dx = current.X - target.X;
        var dy = current.Y - target.Y;
        return dx * dx + dy * dy <= threshold * threshold;
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "關閉"
        };

        if (this.XamlRoot is not null)
        {
            dialog.XamlRoot = this.XamlRoot;
        }

        await dialog.ShowAsync();
    }

    private void UpdateHint(string message)
    {
        ViewModel.HintText = message;
    }

    private void ApplyToolbarVisibility()
    {
        if (ToolbarContainer is null)
        {
            return;
        }

        ToolbarContainer.Visibility = ShowToolbar ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SyncToolComboBox(AnnotationTool tool)
    {
        if (ToolComboBox is null)
        {
            return;
        }

        foreach (var entry in ToolComboBox.Items.OfType<ComboBoxItem>())
        {
            if (entry.Tag is string tag && string.Equals(tag, tool.ToString(), StringComparison.Ordinal))
            {
                ToolComboBox.SelectedItem = entry;
                break;
            }
        }
    }

    private void SyncColorComboBox(string? colorKey)
    {
        if (ColorComboBox is null)
        {
            return;
        }

        ComboBoxItem? matchedItem = null;

        if (!string.IsNullOrWhiteSpace(colorKey))
        {
            matchedItem = ColorComboBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Tag as string, colorKey, StringComparison.Ordinal));
        }

        if (matchedItem is null)
        {
            matchedItem = ColorComboBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag is string tag && ViewModel.TryGetColor(tag, out var knownColor) && knownColor == ViewModel.ActiveColor);
        }

        if (matchedItem is not null && !ReferenceEquals(ColorComboBox.SelectedItem, matchedItem))
        {
            ColorComboBox.SelectedItem = matchedItem;
        }
    }

    private AnnotationSelectionInfo CreateSelectionInfo(AnnotationItem annotation)
    {
        var bounds = GetBounds(annotation);
        var color = annotation.ShapeElement.Stroke is SolidColorBrush brush ? brush.Color : ViewModel.ActiveColor;
        var points = annotation.ShapeElement is Polygon polygon
            ? polygon.Points.Select(point => new Point(point.X, point.Y)).ToList()
            : [];

        return new AnnotationSelectionInfo
        {
            Kind = annotation.Kind == AnnotationShapeKind.Rectangle ? AnnotationSelectionKind.Rectangle : AnnotationSelectionKind.Polygon,
            Bounds = bounds,
            StrokeColor = color,
            Points = points
        };
    }

    private enum AnnotationShapeKind
    {
        Rectangle,
        Polygon
    }

    private enum RectangleHandle
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private sealed class AnnotationItem
    {
        public AnnotationShapeKind Kind { get; init; }

        public Shape ShapeElement { get; init; } = null!;
    }

    private sealed class AnnotationHandleTag
    {
        public AnnotationItem Owner { get; init; } = null!;

        public RectangleHandle? RectangleHandle { get; init; }

        public int? PolygonVertexIndex { get; init; }
    }
}

public enum AnnotationSelectionKind
{
    Rectangle,
    Polygon
}

public sealed class AnnotationSelectionInfo
{
    public AnnotationSelectionKind Kind { get; init; }

    public Rect Bounds { get; init; }

    public Windows.UI.Color StrokeColor { get; init; }

    public IReadOnlyList<Point> Points { get; init; } = [];
}

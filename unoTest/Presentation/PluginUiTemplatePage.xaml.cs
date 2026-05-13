using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace unoTest.Presentation;

public sealed partial class PluginUiTemplatePage : Page
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly Dictionary<string, PluginUiActionDocument> _actionsById = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, object?> _state = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FrameworkElement> _boundControls = new(StringComparer.OrdinalIgnoreCase);

    private PluginUiTemplateDocument? _template;
    private bool _isApplyingState;

    public PluginUiTemplatePage()
    {
        this.InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await LoadTemplateAndRenderAsync();
    }

    private async Task LoadTemplateAndRenderAsync()
    {
        var template = await LoadTemplateAsync();

        if (template is null)
        {
            SetStatus("找不到模板文件：Docs/Templates/Pages/PluginUiTemplatePage/plugin-ui.template.json", isError: true);
            return;
        }

        _template = template;

        PageTitleText.Text = string.IsNullOrWhiteSpace(template.Title) ? "Plugin 動態模板頁" : template.Title;
        PageDescriptionText.Text = template.Description;

        _actionsById.Clear();
        foreach (var action in template.Actions)
        {
            if (!string.IsNullOrWhiteSpace(action.Id))
            {
                _actionsById[action.Id] = action;
            }
        }

        _state.Clear();
        foreach (var item in template.InitialState)
        {
            _state[item.Key] = NormalizeStateValue(item.Value);
        }

        _boundControls.Clear();
        SectionsHost.Children.Clear();

        foreach (var section in template.Sections)
        {
            SectionsHost.Children.Add(BuildSectionView(section));
        }

        await ExecutePageActionsAsync();
        RenderStatePreview();
        SetStatus("模板已載入，請操作欄位或按鈕測試 plugin action。", isError: false);
    }

    private Border BuildSectionView(PluginUiSectionDocument section)
    {
        var root = new StackPanel
        {
            Spacing = 10
        };

        if (!string.IsNullOrWhiteSpace(section.Header))
        {
            root.Children.Add(new TextBlock
            {
                Text = section.Header,
                Style = TryGetTextStyle("BodyStrongTextBlockStyle")
            });
        }

        foreach (var element in section.Elements)
        {
            root.Children.Add(BuildElementView(element));
        }

        return new Border
        {
            Background = (Brush?)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12),
            Child = root
        };
    }

    private FrameworkElement BuildElementView(PluginUiElementDocument element)
    {
        var stack = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var isButton = string.Equals(element.ControlType, "Button", StringComparison.OrdinalIgnoreCase);
        if (!isButton)
        {
            stack.Children.Add(new TextBlock
            {
                Text = element.IsRequired ? $"{element.Label} *" : element.Label,
                Style = TryGetTextStyle("CaptionTextBlockStyle")
            });
        }

        FrameworkElement control = CreateControl(element);
        stack.Children.Add(control);

        return stack;
    }

    private FrameworkElement CreateControl(PluginUiElementDocument element)
    {
        var type = element.ControlType?.Trim().ToLowerInvariant() ?? "textbox";

        return type switch
        {
            "textbox" => CreateTextBox(element, acceptsReturn: false),
            "textarea" => CreateTextBox(element, acceptsReturn: true),
            "numberbox" => CreateNumberBox(element),
            "combobox" => CreateComboBox(element),
            "toggleswitch" => CreateToggleSwitch(element),
            "button" => CreateButton(element),
            _ => new TextBlock
            {
                Text = $"Unsupported ControlType: {element.ControlType}",
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.OrangeRed)
            }
        };
    }

    private TextBox CreateTextBox(PluginUiElementDocument element, bool acceptsReturn)
    {
        var textBox = new TextBox
        {
            PlaceholderText = element.Placeholder,
            IsEnabled = element.IsEnabled,
            AcceptsReturn = acceptsReturn,
            TextWrapping = acceptsReturn ? TextWrapping.Wrap : TextWrapping.NoWrap,
            MinHeight = acceptsReturn ? 96 : 0
        };

        var stateKey = element.Binding?.StateKey;
        if (!string.IsNullOrWhiteSpace(stateKey))
        {
            textBox.Text = ReadStateAsString(stateKey);
            _boundControls[stateKey] = textBox;
        }

        textBox.TextChanged += async (_, _) =>
        {
            if (_isApplyingState)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(stateKey))
            {
                _state[stateKey] = textBox.Text;
            }

            await ExecuteElementActionsAsync(element, "changed");
        };

        return textBox;
    }

    private NumberBox CreateNumberBox(PluginUiElementDocument element)
    {
        var numberBox = new NumberBox
        {
            IsEnabled = element.IsEnabled,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        };

        var stateKey = element.Binding?.StateKey;
        if (!string.IsNullOrWhiteSpace(stateKey))
        {
            numberBox.Value = ReadStateAsDouble(stateKey);
            _boundControls[stateKey] = numberBox;
        }

        numberBox.ValueChanged += async (_, _) =>
        {
            if (_isApplyingState)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(stateKey))
            {
                _state[stateKey] = numberBox.Value;
            }

            await ExecuteElementActionsAsync(element, "changed");
        };

        return numberBox;
    }

    private ComboBox CreateComboBox(PluginUiElementDocument element)
    {
        var comboBox = new ComboBox
        {
            IsEnabled = element.IsEnabled,
            ItemsSource = element.Options,
            DisplayMemberPath = nameof(PluginUiOptionDocument.Text),
            SelectedValuePath = nameof(PluginUiOptionDocument.Value)
        };

        var stateKey = element.Binding?.StateKey;
        if (!string.IsNullOrWhiteSpace(stateKey))
        {
            comboBox.SelectedValue = ReadStateAsString(stateKey);
            _boundControls[stateKey] = comboBox;
        }

        comboBox.SelectionChanged += async (_, _) =>
        {
            if (_isApplyingState)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(stateKey))
            {
                _state[stateKey] = comboBox.SelectedValue?.ToString() ?? string.Empty;
            }

            await ExecuteElementActionsAsync(element, "changed");
        };

        return comboBox;
    }

    private ToggleSwitch CreateToggleSwitch(PluginUiElementDocument element)
    {
        var toggle = new ToggleSwitch
        {
            IsEnabled = element.IsEnabled,
            OnContent = element.Label,
            OffContent = element.Label
        };

        var stateKey = element.Binding?.StateKey;
        if (!string.IsNullOrWhiteSpace(stateKey))
        {
            toggle.IsOn = ReadStateAsBool(stateKey);
            _boundControls[stateKey] = toggle;
        }

        toggle.Toggled += async (_, _) =>
        {
            if (_isApplyingState)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(stateKey))
            {
                _state[stateKey] = toggle.IsOn;
            }

            await ExecuteElementActionsAsync(element, "toggled");
        };

        return toggle;
    }

    private Button CreateButton(PluginUiElementDocument element)
    {
        var button = new Button
        {
            Content = string.IsNullOrWhiteSpace(element.Label) ? "執行" : element.Label,
            IsEnabled = element.IsEnabled,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        if (element.Metadata.TryGetValue("Style", out var styleName))
        {
            var key = styleName.ToLowerInvariant() switch
            {
                "filled" => "MaterialFilledButtonStyle",
                "outlined" => "MaterialOutlinedButtonStyle",
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(key) && Application.Current.Resources.ContainsKey(key))
            {
                button.Style = (Style)Application.Current.Resources[key];
            }
        }

        button.Click += async (_, _) => await ExecuteElementActionsAsync(element, "click");
        return button;
    }

    private async Task ExecutePageActionsAsync()
    {
        if (_template is null || DataContext is not PluginUiTemplateViewModel vm)
        {
            return;
        }

        foreach (var action in _template.PageActions)
        {
            var result = await vm.ActionDispatcher.DispatchAsync(
                action,
                _template.TemplateId,
                eventName: "loaded",
                elementId: "page",
                state: _state);

            ApplyStatePatch(result.StatePatch);
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                SetStatus(result.Message, !result.IsSuccess);
            }
        }
    }

    private async Task ExecuteElementActionsAsync(PluginUiElementDocument element, string eventName)
    {
        if (_template is null || DataContext is not PluginUiTemplateViewModel vm)
        {
            return;
        }

        var eventBindings = element.Events
            .Where(x => string.Equals(x.Name, eventName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (eventBindings.Count == 0)
        {
            RenderStatePreview();
            return;
        }

        foreach (var binding in eventBindings)
        {
            if (!_actionsById.TryGetValue(binding.ActionId, out var action))
            {
                SetStatus($"找不到 Action: {binding.ActionId}", isError: true);
                continue;
            }

            var result = await vm.ActionDispatcher.DispatchAsync(
                action,
                _template.TemplateId,
                eventName,
                element.Id,
                _state);

            ApplyStatePatch(result.StatePatch);

            var message = string.IsNullOrWhiteSpace(result.Message)
                ? $"已執行 action: {action.Id}"
                : result.Message;

            SetStatus(message, !result.IsSuccess);
        }

        RenderStatePreview();
    }

    private void ApplyStatePatch(IReadOnlyDictionary<string, object?> patch)
    {
        if (patch.Count == 0)
        {
            return;
        }

        foreach (var item in patch)
        {
            var value = NormalizeStateValue(item.Value);
            _state[item.Key] = value;

            if (_boundControls.TryGetValue(item.Key, out var control))
            {
                ApplyStateToControl(control, value);
            }
        }
    }

    private void ApplyStateToControl(FrameworkElement control, object? value)
    {
        _isApplyingState = true;
        try
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.Text = Convert.ToString(value) ?? string.Empty;
                    break;
                case NumberBox numberBox:
                    numberBox.Value = TryConvertToDouble(value);
                    break;
                case ComboBox comboBox:
                    comboBox.SelectedValue = Convert.ToString(value) ?? string.Empty;
                    break;
                case ToggleSwitch toggle:
                    toggle.IsOn = TryConvertToBool(value);
                    break;
            }
        }
        finally
        {
            _isApplyingState = false;
        }
    }

    private void RenderStatePreview()
    {
        StatePreviewTextBox.Text = JsonSerializer.Serialize(_state, JsonOptions);
    }

    private void SetStatus(string message, bool isError)
    {
        StatusText.Text = message;
        StatusText.Foreground = new SolidColorBrush(isError ? Microsoft.UI.Colors.OrangeRed : Microsoft.UI.Colors.ForestGreen);
    }

    private static Style? TryGetTextStyle(string key)
    {
        if (Application.Current.Resources.ContainsKey(key))
        {
            return (Style)Application.Current.Resources[key];
        }

        return null;
    }

    private async Task<PluginUiTemplateDocument?> LoadTemplateAsync()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Docs", "Templates", "Pages", "PluginUiTemplatePage", "plugin-ui.template.json"),
            Path.Combine(AppContext.BaseDirectory, "unoTest", "Docs", "Templates", "Pages", "PluginUiTemplatePage", "plugin-ui.template.json"),
            Path.Combine(Environment.CurrentDirectory, "Docs", "Templates", "Pages", "PluginUiTemplatePage", "plugin-ui.template.json")
        };

        var filePath = candidates.FirstOrDefault(File.Exists);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<PluginUiTemplateDocument>(json, JsonOptions);
    }

    private string ReadStateAsString(string key)
    {
        if (!_state.TryGetValue(key, out var value) || value is null)
        {
            return string.Empty;
        }

        if (value is JsonElement json)
        {
            return json.ValueKind switch
            {
                JsonValueKind.String => json.GetString() ?? string.Empty,
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                JsonValueKind.Null => string.Empty,
                _ => json.ToString() ?? string.Empty
            };
        }

        return Convert.ToString(value) ?? string.Empty;
    }

    private double ReadStateAsDouble(string key)
    {
        if (!_state.TryGetValue(key, out var value) || value is null)
        {
            return 0;
        }

        return TryConvertToDouble(value);
    }

    private bool ReadStateAsBool(string key)
    {
        if (!_state.TryGetValue(key, out var value) || value is null)
        {
            return false;
        }

        return TryConvertToBool(value);
    }

    private static object? NormalizeStateValue(object? value)
    {
        return value switch
        {
            null => null,
            JsonElement json => json.ValueKind switch
            {
                JsonValueKind.String => json.GetString(),
                JsonValueKind.Number => json.TryGetInt64(out var i) ? i : json.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => json.ToString()
            },
            _ => value
        };
    }

    private static double TryConvertToDouble(object? value)
    {
        if (value is null)
        {
            return 0;
        }

        if (value is double d)
        {
            return d;
        }

        if (value is JsonElement json && json.ValueKind == JsonValueKind.Number)
        {
            return json.GetDouble();
        }

        return double.TryParse(Convert.ToString(value), out var result) ? result : 0;
    }

    private static bool TryConvertToBool(object? value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is bool b)
        {
            return b;
        }

        if (value is JsonElement json)
        {
            return json.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(json.GetString(), out var parsed) && parsed,
                _ => false
            };
        }

        return bool.TryParse(Convert.ToString(value), out var result) && result;
    }
}
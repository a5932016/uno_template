using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using Windows.Globalization;

namespace unoTest.Presentation;

/// <summary>
/// 多語系示範頁面
/// </summary>
public sealed partial class LocalizationDemoPage : Page
{
    private readonly IStringLocalizer _localizer;

    public LocalizationDemoPage(IStringLocalizer localizer)
    {
        _localizer = localizer;
        this.InitializeComponent();
        InitializeLanguageCombo();
        UpdateLocalizedTexts();
        UpdateFormattedTexts();
    }

    public LocalizationDemoPage() : this(null!)
    {
        // 設計時支援
    }

    private void InitializeLanguageCombo()
    {
        // 取得目前語言
        var currentLanguage = ApplicationLanguages.PrimaryLanguageOverride;
        if (string.IsNullOrEmpty(currentLanguage))
        {
            currentLanguage = CultureInfo.CurrentUICulture.Name;
        }

        // 選中對應項目
        foreach (ComboBoxItem item in LanguageCombo.Items)
        {
            if (item.Tag?.ToString() == currentLanguage ||
                currentLanguage.StartsWith(item.Tag?.ToString()?.Split('-')[0] ?? ""))
            {
                LanguageCombo.SelectedItem = item;
                break;
            }
        }

        // 如果沒有找到，選擇第一個
        if (LanguageCombo.SelectedItem == null && LanguageCombo.Items.Count > 0)
        {
            LanguageCombo.SelectedIndex = 0;
        }
    }

    private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageCombo.SelectedItem is ComboBoxItem item && item.Tag is string languageTag)
        {
            // 設定應用程式語言
            ApplicationLanguages.PrimaryLanguageOverride = languageTag;

            // 更新文字 (部分會立即生效，部分需要重啟)
            UpdateLocalizedTexts();
            UpdateFormattedTexts();
        }
    }

    private void UpdateLocalizedTexts()
    {
        if (_localizer == null) return;

        try
        {
            // 動態文字示範
            WelcomeText.Text = _localizer["WelcomeMessage"] ?? "歡迎使用多語系功能！";
            DateTimeText.Text = string.Format(_localizer["CurrentDateTime"] ?? "目前時間：{0}",
                DateTime.Now.ToString("F", CultureInfo.CurrentUICulture));

            // 更新複數形式
            UpdatePluralText((int)ItemCountBox.Value);
        }
        catch
        {
            // 資源可能尚未載入
            WelcomeText.Text = "歡迎使用多語系功能！";
            DateTimeText.Text = $"目前時間：{DateTime.Now:F}";
        }
    }

    private void UpdateFormattedTexts()
    {
        var culture = CultureInfo.CurrentUICulture;
        var now = DateTime.Now;
        var number = 1234567.89;
        var percent = 0.1234;

        // 日期格式
        DateFormatText.Text = $"{now.ToString("D", culture)} ({now.ToString("d", culture)})";

        // 時間格式
        TimeFormatText.Text = $"{now.ToString("T", culture)} ({now.ToString("t", culture)})";

        // 數字格式
        NumberFormatText.Text = number.ToString("N", culture);

        // 貨幣格式
        CurrencyFormatText.Text = number.ToString("C", culture);

        // 百分比格式
        PercentFormatText.Text = percent.ToString("P", culture);
    }

    private void ItemCountBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
        {
            UpdatePluralText((int)args.NewValue);
        }
    }

    private void UpdatePluralText(int count)
    {
        // 複數形式示範
        // 在實際應用中，應該使用適當的複數形式庫
        string text;
        if (_localizer != null)
        {
            // 嘗試取得複數形式
            var key = count switch
            {
                0 => "ItemCount_Zero",
                1 => "ItemCount_One",
                _ => "ItemCount_Other"
            };

            var template = _localizer[key];
            if (template != null && !template.ResourceNotFound)
            {
                text = string.Format(template.Value, count);
            }
            else
            {
                text = GetDefaultPluralText(count);
            }
        }
        else
        {
            text = GetDefaultPluralText(count);
        }

        PluralText.Text = text;
    }

    private static string GetDefaultPluralText(int count)
    {
        return count switch
        {
            0 => "沒有項目",
            1 => "1 個項目",
            _ => $"{count} 個項目"
        };
    }
}

/// <summary>
/// 多語系工具類別
/// </summary>
public static class LocalizationHelper
{
    /// <summary>
    /// 取得支援的語言列表
    /// </summary>
    public static IReadOnlyList<LanguageInfo> GetSupportedLanguages()
    {
        return new List<LanguageInfo>
        {
            new("zh-Hant-TW", "繁體中文", "Chinese (Traditional)"),
            new("en-US", "English", "English (US)"),
            new("es", "Español", "Spanish"),
            new("fr", "Français", "French"),
            new("pt-BR", "Português", "Portuguese (Brazil)")
        };
    }

    /// <summary>
    /// 取得目前語言
    /// </summary>
    public static string GetCurrentLanguage()
    {
        var override_lang = ApplicationLanguages.PrimaryLanguageOverride;
        if (!string.IsNullOrEmpty(override_lang))
        {
            return override_lang;
        }
        return CultureInfo.CurrentUICulture.Name;
    }

    /// <summary>
    /// 設定語言
    /// </summary>
    public static void SetLanguage(string languageTag)
    {
        ApplicationLanguages.PrimaryLanguageOverride = languageTag;
    }

    /// <summary>
    /// 取得本地化的列舉顯示文字
    /// </summary>
    public static string GetEnumDisplayName<T>(T value, IStringLocalizer localizer) where T : Enum
    {
        var key = $"Enum_{typeof(T).Name}_{value}";
        var localized = localizer[key];
        return localized.ResourceNotFound ? value.ToString() : localized.Value;
    }
}

/// <summary>
/// 語言資訊
/// </summary>
public record LanguageInfo(string Tag, string NativeName, string EnglishName);

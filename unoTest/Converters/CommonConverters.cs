using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace unoTest.Converters;

/// <summary>
/// 布林值轉 Visibility 轉換器
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            // 檢查是否需要反轉
            bool invert = parameter?.ToString()?.ToLower() == "invert";
            if (invert) boolValue = !boolValue;
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            bool result = visibility == Visibility.Visible;
            bool invert = parameter?.ToString()?.ToLower() == "invert";
            return invert ? !result : result;
        }
        return false;
    }
}

/// <summary>
/// 反向布林值轉 Visibility 轉換器
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }
        return true;
    }
}

/// <summary>
/// 主題圖標轉換器
/// </summary>
public class ThemeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isDarkTheme)
        {
            // 深色主題顯示太陽圖標，淺色主題顯示月亮圖標
            return isDarkTheme ? "\uE706" : "\uE708"; // E706=太陽, E708=月亮
        }
        return "\uE708";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 空字串檢查轉換器
/// </summary>
public class StringEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var str = value?.ToString();
        bool isEmpty = string.IsNullOrWhiteSpace(str);
        bool invert = parameter?.ToString()?.ToLower() == "invert";
        
        if (invert)
            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        
        return isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Null 檢查轉 Visibility 轉換器
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isNull = value == null;
        bool invert = parameter?.ToString()?.ToLower() == "invert";
        
        if (invert)
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        
        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 數值轉 Visibility 轉換器（>0 時顯示）
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        int count = 0;
        if (value is int intValue) count = intValue;
        else if (value is long longValue) count = (int)longValue;
        else if (int.TryParse(value?.ToString(), out int parsed)) count = parsed;
        
        bool invert = parameter?.ToString()?.ToLower() == "invert";
        
        if (invert)
            return count > 0 ? Visibility.Collapsed : Visibility.Visible;
        
        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 枚舉轉布林值轉換器
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null) return false;
        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            return Enum.Parse(targetType, parameter.ToString()!);
        }
        return DependencyProperty.UnsetValue;
    }
}

/// <summary>
/// 日期格式化轉換器
/// </summary>
public class DateFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            string format = parameter?.ToString() ?? "yyyy/MM/dd HH:mm";
            return dateTime.ToString(format);
        }
        if (value is DateTimeOffset dateTimeOffset)
        {
            string format = parameter?.ToString() ?? "yyyy/MM/dd HH:mm";
            return dateTimeOffset.ToString(format);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (DateTime.TryParse(value?.ToString(), out DateTime result))
        {
            return result;
        }
        return DateTime.MinValue;
    }
}

/// <summary>
/// 數值格式化轉換器
/// </summary>
public class NumberFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string format = parameter?.ToString() ?? "N0";
        
        if (value is int intValue) return intValue.ToString(format);
        if (value is long longValue) return longValue.ToString(format);
        if (value is double doubleValue) return doubleValue.ToString(format);
        if (value is decimal decimalValue) return decimalValue.ToString(format);
        if (value is float floatValue) return floatValue.ToString(format);
        
        return value?.ToString() ?? "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (double.TryParse(value?.ToString(), out double result))
        {
            return result;
        }
        return 0;
    }
}

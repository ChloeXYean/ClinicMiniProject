using System;
using System.Globalization;

namespace ClinicMiniProject.Converters
{
    public sealed class InquiryStatusToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var s = value?.ToString() ?? string.Empty;
            if (string.Equals(s, "Pending", StringComparison.OrdinalIgnoreCase))
                return Color.FromArgb("#FFB020");

            if (string.Equals(s, "Replied", StringComparison.OrdinalIgnoreCase))
                return Color.FromArgb("#22C55E");

            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
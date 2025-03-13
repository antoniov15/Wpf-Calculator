using System;
using System.Globalization;
using System.Windows.Data;

namespace Calculator
{
    public class DigitGroupingConverter : IValueConverter
    {
        //private UserSettings settings;

        //public DigitGroupingConverter()
        //{
        //    settings = new UserSettings();
        //}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            string displayText = value.ToString();

            // If the text doesn't represent a valid number, return it as is
            if (!double.TryParse(displayText, out double number))
                return displayText;

            // Get a fresh instance of settings each time to ensure the latest value is used
            UserSettings settings = new UserSettings();

            // Apply digit grouping if enabled
            if (settings.GetUseDigitGrouping())
            {
                // Get the current culture to use the correct digit grouping symbol
                NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;

                // Split the number into integer and decimal parts
                string[] parts = displayText.Split('.');

                // Format the integer part with digit grouping
                parts[0] = long.Parse(parts[0]).ToString("N0", nfi);

                // If there's a decimal part, rejoin with the formatted integer part
                if (parts.Length > 1)
                    return parts[0] + nfi.NumberDecimalSeparator + parts[1];
                else
                    return parts[0];
            }

            return displayText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed for one-way binding
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;

namespace BankApp.Helpers
{
    public class BooleanToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditing && isEditing)
            {
                return "Edit Customer Profile";
            }
            return "Register New Customer";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

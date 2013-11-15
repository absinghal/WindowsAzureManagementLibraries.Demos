using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WamlDemos.Navigation;
using WamlDemos.ViewModels;

namespace WamlDemos.Converters
{
    public class WAMLViewModelToViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Canvas();
            }

            var toFind = (IHostedControlViewModel) value;
            return ModelBasedNavigationHelper.GetControl(toFind);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((FrameworkElement)value).DataContext;
        }
    }
}

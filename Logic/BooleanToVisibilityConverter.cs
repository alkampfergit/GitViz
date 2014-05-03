using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace GitViz.Logic
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        #region IValueConverter Members

        public BooleanToVisibilityConverter()
        {
            FalseVisibilityValue = Visibility.Collapsed;
        }

        public Visibility FalseVisibilityValue { get; set; }

        public Boolean ConditionForHide { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return ((bool)value == ConditionForHide) ? FalseVisibilityValue : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DependOnMe.VsExtension.ModuleAdornment.UI
{
    internal sealed class DependencyModuleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Aggregate(
                new List<object>(),
                (acc, item) =>
                {
                    if (item is IEnumerable a)
                    {
                        foreach (var i in a)
                        {
                            acc.Add(i);
                        }

                        return acc;
                    }

                    acc.Add(item);

                    return acc;
                }).ToArray();


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot perform reverse-conversion");
        }
    }
}
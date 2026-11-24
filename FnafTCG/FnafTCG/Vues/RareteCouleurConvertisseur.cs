using System.Globalization;
using FnafTCG.VuesModeles;

namespace FnafTCG.Vues;

public class RareteCouleurConvertisseur : IValueConverter
{
    public object Convert(object? value, Type targetType,
                          object? parameter, CultureInfo culture)
        => CartesVueModele.CouleurRarete(value as string);

    public object ConvertBack(object? value, Type targetType,
                              object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
namespace MadWorldNL.EarCut.Logic.Extensions;

internal static class DoubleExtensions
{
    private const double Tolerance = 1e-9;
    
    internal static bool NotEqual(this double value1, double value2)
    {
        return !value1.Equal(value2);
    }
    
    internal static bool Equal(this double value1, double value2)
    {
        return Math.Abs(value1 - value2) < Tolerance;
    }
}
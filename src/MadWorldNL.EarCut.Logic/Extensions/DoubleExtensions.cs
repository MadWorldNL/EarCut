namespace MadWorldNL.EarCut.Logic.Extensions;

public static class DoubleExtensions
{
    private const double Tolerance = 1e-9;
    
    public static bool NotEqual(this double value1, double value2)
    {
        return !value1.Equal(value2);
    }
    
    public static bool Equal(this double value1, double value2)
    {
        return Math.Abs(value1 - value2) < Tolerance;
    }
}
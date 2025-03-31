using System.Numerics;

namespace MadWorldNL.EarCut.Logic.Extensions;

internal static class VertexExtensions
{
    internal static bool NotEqual<TVertex>(this TVertex value1, TVertex value2) where TVertex : INumber<TVertex>
    {
        return !value1.Equal(value2);
    }
    
    internal static bool Equal<TVertex>(this TVertex value1, TVertex value2) where TVertex : INumber<TVertex>
    {
        return value1.Equals(value2);
    }
}
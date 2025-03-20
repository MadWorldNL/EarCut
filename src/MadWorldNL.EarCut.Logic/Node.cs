using System.Numerics;
using System.Text;

namespace MadWorldNL.EarCut.Logic;

internal class Node<TVertex> where TVertex : INumber<TVertex>, IMinMaxValue<TVertex>
{
    internal readonly int I;
    internal readonly TVertex X;
    internal readonly TVertex Y;
    internal TVertex Z;
    internal bool Steiner;

    internal Node<TVertex>? Prev;
    internal Node<TVertex>? Next;
    internal Node<TVertex>? PrevZ;
    internal Node<TVertex>? NextZ;

    internal Node(int i, TVertex x, TVertex y)
    {
        // vertex index in coordinates array
        I = i;

        // vertex coordinates
        X = x;
        Y = y;

        // previous and next vertex nodes in a polygon ring
        Prev = null;
        Next = null;

        // z-order curve value
        Z = TVertex.MinValue;

        // previous and next nodes in z-order
        PrevZ = null;
        NextZ = null;

        // indicates whether this is a steiner point
        Steiner = false;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("{i: ")
            .Append(I)
            .Append(", x: ")
            .Append(X)
            .Append(", y: ")
            .Append(Y)
            .Append(", prev: ")
            .Append(ToString(Prev))
            .Append(", next: ")
            .Append(ToString(Next))
            .Append("}");
        
        return sb.ToString();
    }
    
    private static string ToString(Node<TVertex>? node){
        if(node == null){
            return "null";
        }
        return "{i: " + node.I + ", x: " + node.X + ", y: " + node.Y + "}";
    }
}
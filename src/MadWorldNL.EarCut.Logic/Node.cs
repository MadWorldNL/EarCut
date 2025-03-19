using System.Text;

namespace MadWorldNL.EarCut.Logic;

internal class Node
{
    internal int i;
    internal double x;
    internal double y;
    internal double z;
    internal bool steiner;

    internal Node? prev;
    internal Node? next;
    internal Node? prevZ;
    internal Node? nextZ;

    internal Node(int i, double x, double y)
    {
        // vertice index in coordinates array
        this.i = i;

        // vertex coordinates
        this.x = x;
        this.y = y;

        // previous and next vertice nodes in a polygon ring
        this.prev = null;
        this.next = null;

        // z-order curve value
        this.z = double.MinValue;

        // previous and next nodes in z-order
        this.prevZ = null;
        this.nextZ = null;

        // indicates whether this is a steiner point
        this.steiner = false;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("{i: ")
            .Append(i)
            .Append(", x: ")
            .Append(x)
            .Append(", y: ")
            .Append(y)
            .Append(", prev: ")
            .Append(ToString(prev))
            .Append(", next: ")
            .Append(ToString(next))
            .Append("}");
        
        return sb.ToString();
    }
    
    private static string ToString(Node? node){
        if(node == null){
            return "null";
        }
        return "{i: " + node.i + ", x: " + node.x + ", y: " + node.y + "}";
    }
}
namespace MadWorldNL.EarCut.Tests;

public class EarCutTests
{
    [Fact]
    public void SimpleTriangle()
    {
        var triangles = Logic.EarCut.Calculate([0, 0, 0, 50, 50, 00], null, 2);
        Assert.Equal([1, 0, 2], triangles);
    }
    
    [Fact]
    public void NotSoSimpleTriangle() {
        var triangles = Logic.EarCut.Calculate([0, 0, 0, 25, 0, 50, 25, 25, 50, 0, 25, 0], null, 2);
        // Not optimal, but correct.
        Assert.Equal([ 1, 0, 5, 5, 4, 3, 3, 2, 1, 1, 5, 3 ], triangles);
    }

    [Fact]
    public void LShape()
    {
        var triangles = Logic.EarCut.Calculate([0, 0, 10, 0, 10, 5, 5, 5, 5, 15, 0, 15], null, 2);
        Assert.Equal([4, 5, 0, 0, 1, 2, 3, 4, 0, 0, 2, 3], triangles);
    }

    [Fact]
    public void SimplePolygon()
    {
        var triangles = Logic.EarCut.Calculate([10, 0, 0, 50, 60, 60, 70, 10], null, 2);
        Assert.Equal([1, 0, 3, 3, 2, 1], triangles);
    }

    [Fact]
    public void PolygonWithHole()
    {
        var triangles = Logic.EarCut.Calculate(
            [0, 0, 100, 0, 100, 100, 0, 100, 20, 20, 80, 20, 80, 80, 20, 80], [4], 2);
        Assert.Equal([ 3, 0, 4, 5, 4, 0, 3, 4, 7, 5, 0, 1, 2, 3, 7, 6, 5, 1, 2, 7, 6, 6, 1, 2], triangles);
    }

    [Fact]
    public void PolygonWith3DCoords()
    {
        double[] polygon = [10, 0, 1, 0, 50, 2, 60, 60, 3, 70, 10, 4];
        List<int> triangles = Logic.EarCut.Calculate(polygon, null, 3);
        
        Assert.Equal([ 1, 0, 3, 3, 2, 1], triangles);
    }
}
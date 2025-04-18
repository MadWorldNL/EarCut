namespace MadWorldNL.EarCut.Tests;

public class EarCutTests
{
    [Fact]
    public void SimpleTriangleDouble()
    {
        var triangles = Logic.EarCut<double>.Tessellate([0, 0, 0, 50, 50, 00]);
        Assert.Equal([1, 0, 2], triangles);
    }
    
    [Fact]
    public void SimpleTriangleFloat()
    {
        var triangles = Logic.EarCut<float>.Tessellate([0, 0, 0, 50, 50, 00]);
        Assert.Equal([1, 0, 2], triangles);
    }
    
    [Fact]
    public void NotSoSimpleTriangleDouble() {
        var triangles = Logic.EarCut<double>.Tessellate([0, 0, 0, 25, 0, 50, 25, 25, 50, 0, 25, 0]);
        // Not optimal, but correct.
        Assert.Equal([ 1, 0, 5, 5, 4, 3, 3, 2, 1, 1, 5, 3 ], triangles);
    }
    
    [Fact]
    public void NotSoSimpleTriangleFloat() {
        var triangles = Logic.EarCut<float>.Tessellate([0, 0, 0, 25, 0, 50, 25, 25, 50, 0, 25, 0]);
        // Not optimal, but correct.
        Assert.Equal([ 1, 0, 5, 5, 4, 3, 3, 2, 1, 1, 5, 3 ], triangles);
    }

    [Fact]
    public void LShapeDouble()
    {
        var triangles = Logic.EarCut<double>.Tessellate([0, 0, 10, 0, 10, 5, 5, 5, 5, 15, 0, 15]);
        Assert.Equal([4, 5, 0, 0, 1, 2, 3, 4, 0, 0, 2, 3], triangles);
    }
    
    [Fact]
    public void LShapeFloat()
    {
        var triangles = Logic.EarCut<float>.Tessellate([0, 0, 10, 0, 10, 5, 5, 5, 5, 15, 0, 15]);
        Assert.Equal([4, 5, 0, 0, 1, 2, 3, 4, 0, 0, 2, 3], triangles);
    }

    [Fact]
    public void SimplePolygonDouble()
    {
        var triangles = Logic.EarCut<double>.Tessellate([10, 0, 0, 50, 60, 60, 70, 10]);
        Assert.Equal([1, 0, 3, 3, 2, 1], triangles);
    }
    
    [Fact]
    public void SimplePolygonFloat()
    {
        var triangles = Logic.EarCut<float>.Tessellate([10, 0, 0, 50, 60, 60, 70, 10]);
        Assert.Equal([1, 0, 3, 3, 2, 1], triangles);
    }

    [Fact]
    public void PolygonWithHoleDouble()
    {
        var triangles = Logic.EarCut<double>.Tessellate(
            [0, 0, 100, 0, 100, 100, 0, 100, 20, 20, 80, 20, 80, 80, 20, 80], [4]);
        Assert.Equal([ 3, 0, 4, 5, 4, 0, 3, 4, 7, 5, 0, 1, 2, 3, 7, 6, 5, 1, 2, 7, 6, 6, 1, 2], triangles);
    }
    
    [Fact]
    public void PolygonWithHoleFloat()
    {
        var triangles = Logic.EarCut<float>.Tessellate(
            [0, 0, 100, 0, 100, 100, 0, 100, 20, 20, 80, 20, 80, 80, 20, 80], [4]);
        Assert.Equal([ 3, 0, 4, 5, 4, 0, 3, 4, 7, 5, 0, 1, 2, 3, 7, 6, 5, 1, 2, 7, 6, 6, 1, 2], triangles);
    }

    [Fact]
    public void PolygonWith3DCoordsDouble()
    {
        double[] polygon = [10, 0, 1, 0, 50, 2, 60, 60, 3, 70, 10, 4];
        List<int> triangles = Logic.EarCut<double>.Tessellate(polygon, null, 3);
        
        Assert.Equal([ 1, 0, 3, 3, 2, 1], triangles);
    }
    
    [Fact]
    public void PolygonWith3DCoordsFloat()
    {
        float[] polygon = [10, 0, 1, 0, 50, 2, 60, 60, 3, 70, 10, 4];
        List<int> triangles = Logic.EarCut<float>.Tessellate(polygon, null, 3);
        
        Assert.Equal([ 1, 0, 3, 3, 2, 1], triangles);
    }
    
    [Fact]
    public void Deviation_GivenWhenSimple2DPolygon_ThenReturnsCorrectDeviation()
    {
        // Arrange
        double[] polygon = [0, 0, 0, 50, 50, 00];
        List<int> triangles = [1, 0, 2];
        
        // Act
        var percentage = Logic.EarCut.Deviation(polygon, triangles);

        // Assert
        Assert.Equal(0, percentage);
    }
    
    [Fact]
    public void Deviation_GivenWhenSimple2DPolygon_ThenReturnsMissedPercentage()
    {
        // Arrange
        double[] polygon = [10, 0, 0, 50, 60, 60, 70, 10];
        List<int> triangles = [3, 2, 1];
        
        // Act
        var percentage = Logic.EarCut.Deviation(polygon, triangles);

        // Assert
        Assert.Equal(0.5, percentage);
    }
}
using MathNet.Numerics.LinearAlgebra;
using TradingBot.Domain.Math;

namespace TradingBot.Domain.Tests.Math;

public class MathExtensionTest
{
    // test MatrixMultiply method with 2x2 matrices
    [Fact]
    public void MatrixMultiply2X2Test()
    {
        // Arrange
        var matrix1 = new double[,] { { 1, 2 }, { 3, 4 } };
        var matrix2 = new double[,] { { 5, 6 }, { 7, 8 } };
        var expected = new double[,] { { 19, 22 }, { 43, 50 } };
        
        var m1 = Matrix<double>.Build.DenseOfArray(matrix1);
        var m2 = Matrix<double>.Build.DenseOfArray(matrix2);
        var expectedMatrix = Matrix<double>.Build.DenseOfArray(expected);

        // Act
        var result = MathExtension.MatrixMultiply(m1, m2);

        // Assert
        Assert.Equal(expectedMatrix, result);
    }
    // test MatrixMultiply method with 3x3 matrices
    [Fact]
    public void MatrixMultiply3X3Test()
    {
        // Arrange
        var matrix1 = new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
        var matrix2 = new double[,] { { 9, 8, 7 }, { 6, 5, 4 }, { 3, 2, 1 } };
        var expected = new double[,] { { 30, 24, 18 }, { 84, 69, 54 }, { 138, 114, 90 } };
        
        var m1 = Matrix<double>.Build.DenseOfArray(matrix1);
        var m2 = Matrix<double>.Build.DenseOfArray(matrix2);
        var expectedMatrix = Matrix<double>.Build.DenseOfArray(expected);

        // Act
        var result = MathExtension.MatrixMultiply(m1, m2);

        // Assert
        Assert.Equal(expectedMatrix, result);
    }
    // test MatrixMultiply method with 2x3 and 3x2 matrices
    [Fact]
    public void MatrixMultiply2X3And3X2Test()
    {
        // Arrange
        var matrix1 = new double[,] { { 1, 2, 3 }, { 4, 5, 6 } };
        var matrix2 = new double[,] { { 7, 8 }, { 9, 10 }, { 11, 12 } };
        var expected = new double[,] { { 58, 64 }, { 139, 154 } };
        
        var m1 = Matrix<double>.Build.DenseOfArray(matrix1);
        var m2 = Matrix<double>.Build.DenseOfArray(matrix2);
        var expectedMatrix = Matrix<double>.Build.DenseOfArray(expected);

        // Act
        var result = MathExtension.MatrixMultiply(m1, m2);

        // Assert
        Assert.Equal(expectedMatrix, result);
    }
    // test MatrixInverse method with invertible matrix
    [Fact]
    public void MatrixInverseTest()
    {
        // Arrange
        var matrix = new double[,] { { 1, 2, -1 }, { 4, 5, 6 }, { 7, 8, 9 } };
        var expected = new double[,]
        {
            { -0.25, -2.16666666666667, 1.41666666666667 }, 
            { 0.5, 1.3333333332999999, -0.8333333333 },
            { -0.25, 0.5, -0.25 }
        };

        var m = Matrix<double>.Build.DenseOfArray(matrix);
        var expectedMatrix = Matrix<double>.Build.DenseOfArray(expected);

        // Act
        var result = MathExtension.MatrixInverse(m);

        // Assert
        for (var i = 0; i < expectedMatrix.RowCount; i++)
        {
            for (var j = 0; j < expectedMatrix.ColumnCount; j++)
            {
                Assert.Equal(expectedMatrix[i, j], result[i, j], 10);
            }
        }
    }
    // test MatrixInverse method with non-square matrix
    [Fact]
    public void MatrixInverseNonSquareMatrixTest()
    {
        // Arrange
        var matrix = new double[,] { { 1, 2, -1 }, { 4, 5, 6 } };
        var m = Matrix<double>.Build.DenseOfArray(matrix);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => MathExtension.MatrixInverse(m));
    }
    // test MatrixInverse method with non-invertible matrix
    [Fact]
    public void MatrixInverseNonInvertibleMatrixTest()
    {
        // Arrange
        var matrix = new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
        var m = Matrix<double>.Build.DenseOfArray(matrix);

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => MathExtension.MatrixInverse(m));
    }
}
using MathNet.Numerics.LinearAlgebra;

namespace TradingBot.Domain.Math;

public static class MathExtension
{
    public static Matrix<double> MatrixMultiply(Matrix<double> a, Matrix<double> b)
    {
        return a.Multiply(b);
    }
    
    public static Matrix<double> MatrixInverse(Matrix<double> mat)
    {
        // Check if the matrix is square
        if (mat.RowCount != mat.ColumnCount)
        {
            throw new ArgumentException("The matrix must be square");
        }

        // Calculate the determinant
        var det = mat.Determinant();
        if (det == 0)
        {
            throw new InvalidOperationException("The matrix is not invertible");
        }

        // Calculate the inverse
        var invMat = mat.Inverse();

        return invMat;
    }
}
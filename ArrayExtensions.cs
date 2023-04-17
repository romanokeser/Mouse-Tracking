using System.Linq;

public static class ArrayExtensions
{
    public static double[,] ToRectangularArray(this double[][] source)
    {
        int rowCount = source.Length;
        int colCount = source.Max(subArray => subArray.Length);

        double[,] result = new double[rowCount, colCount];
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < source[i].Length; j++)
            {
                result[i, j] = source[i][j];
            }
        }

        return result;
    }
}
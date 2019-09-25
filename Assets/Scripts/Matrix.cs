using UnityEngine;
using System.Collections;

[System.Serializable]
public class Matrix
{
    public float[] value;
    public int rows;
    public int cols;

    public Matrix(float[] a)
    {
        rows = a.Length;
        cols = 1;
        value = new float[rows * cols];

        for (int j = 0; j < rows; j++)
        {
            value[j] = a[j];
        }
    }

    public Matrix(int r, int c)
    {
        value = new float[r * c];
        rows = r;
        cols = c;
    }

    public void Randomize()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                value[GetMatrixIndex(r, c, cols)] = Random.value * 2 - 1;
            }
        }
    }

    public void Fill(float num)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                value[GetMatrixIndex(r, c, cols)] = num;
            }
        }
    }

    // multiply weights
    public Matrix Dot(Matrix other)
    {
        if (cols == other.rows)
        {
            Matrix result = new Matrix(rows, other.cols);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < other.cols; c++)
                {
                    float sum = 0;
                    for (int j = 0; j < cols; j++)
                    {
                        sum += value[GetMatrixIndex(r, j, cols)] * other.value[GetMatrixIndex(j, c, other.cols)];
                    }
                    result.value[GetMatrixIndex(r, c, result.cols)] = sum;
                }
            }
            return result;
        }
        else
        {
            throw new System.Exception("Invalid Dot Product: " + cols + " x " + other.rows);
        }
    }

    // add bias
    public Matrix Add(Matrix other)
    {
        if (cols == other.cols && rows == other.rows)
        {
            Matrix result = new Matrix(rows, cols);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    result.value[GetMatrixIndex(r, c, result.cols)] = value[GetMatrixIndex(r, c, cols)] + other.value[GetMatrixIndex(r, c, other.cols)];
                }
            }
            return result;
        }
        else
        {
            throw new System.Exception("Invalid Matrix Addition: " + rows + "x" + cols + " vs " + other.rows + "x" + other.cols);
        }
    }

    // activation function
    public Matrix Activate()
    {
        Matrix result = new Matrix(rows, cols);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                result.value[GetMatrixIndex(r, c, result.cols)] = 1 / (1 + Mathf.Exp(-1 * value[GetMatrixIndex(r, c, cols)]));
            }
        }
        return result;
    }

    // cross over 2 matrices
    public Matrix[] CrossOver(Matrix other)
    {
        Matrix[] results = new Matrix[2];
        Matrix result1 = new Matrix(rows, cols);
        Matrix result2 = new Matrix(rows, cols);
        int pivot = (int)(Random.value * value.Length);

        // half this, half other
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (r * cols < pivot)
                {
                    result1.value[GetMatrixIndex(r, c, result1.cols)] = value[GetMatrixIndex(r, c, cols)];
                }
                else
                {
                    result1.value[GetMatrixIndex(r, c, result1.cols)] = other.value[GetMatrixIndex(r, c, other.cols)];
                }
            }
        }

        // half other, half this
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (r * cols < pivot)
                {
                    result2.value[GetMatrixIndex(r, c, result2.cols)] = other.value[GetMatrixIndex(r, c, other.cols)];
                }
                else
                {
                    result2.value[GetMatrixIndex(r, c, result2.cols)] = value[GetMatrixIndex(r, c, cols)];
                }
            }
        }

        results[0] = result1;
        results[1] = result2;

        return results;
    }

    // mutates weight 
    public void Mutate(float mutationRate)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (Random.value < mutationRate)
                {
                    value[GetMatrixIndex(r, c, cols)] += Random.Range(-.5f, .5f);
                    //// caps mutation
                    //if (value[GetMatrixIndex(r, c, cols)] > 1)
                    //{
                    //    value[GetMatrixIndex(r, c, cols)] = 1;
                    //}
                    //else if (value[GetMatrixIndex(r, c, cols)] < -1)
                    //{
                    //    value[GetMatrixIndex(r, c, cols)] = -1;
                    //}
                }
            }
        }
    }

    public Matrix Copy()
    {
        Matrix result = new Matrix(rows, cols);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                result.value[GetMatrixIndex(r, c, result.cols)] = value[GetMatrixIndex(r, c, cols)];
            }
        }
        return result;
    }

    public void Print()
    {
        string output = "";
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                output += value[GetMatrixIndex(r, c, cols)] + " ";
            }
            output += "\n";
        }
        Debug.Log(output);
    }

    public float[] ToArray()
    {
        float[] result = new float[rows];
        for (int j = 0; j < rows; j++)
        {
            result[j] = value[GetMatrixIndex(j, 0, cols)];
        }
        return result;
    }

    int GetMatrixIndex(int r, int c, int cols)
    {
        return r * cols + c;
    }
}

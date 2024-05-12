namespace Chromaprint.Image;

/// <summary>
/// Class for storing chroma image as a matrix
/// </summary>
public class Image
{
    private static readonly int BUFFER_BLOCK_SIZE = 2048;

    private int _rows;
    private int _columns;
    private double[] _data;

    public int Columns => _columns;
    public int Rows => _rows;
    public double[] Data => _data;

    public double this[int i, int j]
    {   
        get => _data[i * _columns + j];
        set => _data[i * _columns + j] = value;
    }


    /// <summary>
    /// Initializes an empty <see cref="Image"/> with provided image parameters.
    /// If the size of an image determined by row*columns is less
    /// than the minimum buffer block size, image size is adjusted 
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="rows"></param>
    public Image(int columns, int rows)
    {
        _rows = rows;
        _columns = columns;
        _data = new double[Math.Max(columns * rows, BUFFER_BLOCK_SIZE)];
    }

    /// <summary>
    /// Initializes an <see cref="Image"/> instance with provided data
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="data"></param>
    public Image(int columns, double[] data)
    {
        _rows = data.Length / columns;
        _columns = columns;
        _data = data;
    }

    /// <summary>
    /// Initializes an empty <see cref="Image"/> with provided amount of columns.
    /// If the size of an image determined by row*columns is less
    /// than the minimum buffer block size, image size is adjusted 
    /// </summary>
    /// <param name="columns"></param>
    public Image(int columns) 
        : this(columns, 0)
    {
    }
    
    /// <summary>
    /// Add row to the image instance. The row is appended to the end
    /// </summary>
    /// <param name="row">Row data. Expected to be composed of this.Column items</param>
    public void AddRow(double[] row)
    {
        int n = _rows * _columns;
        int size = _data.Length;
        if (n + _columns > size)
        {
            Array.Resize(ref _data, size + BUFFER_BLOCK_SIZE);
        }

        for (int i = 0; i < _columns; i++)
        {
            _data[n + i] = row[i];
        }

        _rows++;
    }

    /// <summary>
    /// Fetch i-th row of image
    /// </summary>
    /// <param name="i">Index of row. Expected to address an existing row</param>
    /// <returns>this.Column-length vector containing items of i-th row</returns>
    public double[] Row(int i)
    {
        double[] row = new double[_columns];
        int n = i * _columns;

        for (int j = 0; j < _columns; j++)
        {
            row[j] = _data[n + j];
        }

        return row;
    }

    /// <summary>
    /// Creates a copy of the existing image
    /// </summary>
    /// <returns>Shallow copy of the image instance</returns>
    public Image Copy()
    {
        double[] copyData = (double[])_data.Clone();
        return new Image(_columns, copyData);
    }
}
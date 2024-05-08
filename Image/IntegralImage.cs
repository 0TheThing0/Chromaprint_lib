namespace Chromaprint.Image;

/// <summary>
/// Class that allows to implement easy and
/// efficient access to area values of <see cref="Image"/>.
/// IntegralImage is mostly useful in <see cref="Filter"/>
/// </summary>
/// <remarks>
/// http://en.wikipedia.org/wiki/Summed_area_table
/// </remarks>
public class IntegralImage
{
    private Image _summedImage;

    public int NumRows() => _summedImage.Rows;
    public int NumColumns() => _summedImage.Columns;
    
    /// <summary>
    /// Acquire the row of the integral image
    /// </summary>
    /// <param name="i"></param>
    public double[] this[int i] => _summedImage.Row(i);
    
    /// <summary>
    /// Construct the integral image from the existing image
    /// </summary>
    /// <param name="image">Image to calculate the area values at</param>
    /// <param name="copyImage">
    /// If the value of this parameter is false,
    /// the existing image will be used to store
    /// area data and therefore will not be usable
    /// </param>
    public IntegralImage(Image image, bool copyImage = false)
    {
        _summedImage = copyImage ? image.Copy() : image;
        CalculateAreas();
    }

#region Public methods

    /// <summary>
    /// Calculates the area of the original image
    /// based on the provided area coordinates
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <returns></returns>
    public double Area(int x1, int y1, int x2, int y2)
    {
        if (x2 < x1 || y2 < y1)
        {
            return 0.0;
        }

        double area = _summedImage[x2,y2];
        if (x1 > 0)
        {
            area -= _summedImage[x1 - 1, y2];
            if (y1 > 0)
            {
                area += _summedImage[x1 - 1, y1 - 1];
            }
        }

        if (y1 > 0)
        {
            area -= _summedImage[x2, y1 - 1];
        }

        return area;
    }
#endregion
    
#region Private methods
    private void CalculateAreas()
    {
        int nRows = _summedImage.Rows;
        int nColumns = _summedImage.Columns;
        
        double[] data = _summedImage.Data;

        // these indices are introduced to properly
        // access a one-dimensional array, interpreting
        // it as a two-dimensional array
        int curr = 1;
        int last = 0;

        // 1. Calculating the first row of integral image:
        // * * * * * * * * * * * *
        // - - - - - - - - - - - - 
        // - - - - - - - - - - - - 
        // - - - - - - - - - - - - 
        for (int i = 1; i < nColumns; i++)
        {
            data[curr] += data[curr - 1];
            curr++;
        }
        
        // 2. Calculating other elements of the array
        for (int i = 1; i < nRows; i++)
        {
            // Calculating first element in the row of the integral image:
            // * * * * * * * * * * * *
            // * - - - - - - - - - - - 
            // - - - - - - - - - - - - 
            // - - - - - - - - - - - -
            data[curr] += data[last];
            curr++;
            last++;
            
            // Then, calculating other elements in the same row:
            // * * * * * * * * * * * *
            // * * * * * * * * * * * *
            // - - - - - - - - - - - - 
            // - - - - - - - - - - - -
            for (int j = 1; j < nColumns; j++)
            {
                data[curr] += data[curr - 1] + data[last] - data[last - 1];
                curr++;
                last++;
            }
        }
    }
#endregion
}
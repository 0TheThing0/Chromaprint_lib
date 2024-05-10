using Chromaprint.Image;
using NAudio.MediaFoundation;

namespace Chromaprint.Classifier;

/// <summary>
/// Class for ApplyFuncing a filter to an image representation
/// of audio in order to avoid distortions (see p.2 in remarks)
/// </summary>
/// <remarks>
/// https://dhoiem.cs.illinois.edu/publications/cvpr2005-mr.pdf
/// </remarks>
public class Filter
{
    // Delegates for distinct components of some filters:
    //  1. filter type (basically the filter map)
    //  2. compare function (compare criteria)
    public delegate double FilterApplier(IntegralImage image, int x, int y, int width, int height, NumComparer cmp);
    public delegate double NumComparer(double a, double b);
    
    
    private int _filterType;
    public FilterApplier ApplyFunc { get; private set; } = Filter0;
    
    private int _compareType;
    public NumComparer CompareFunc { get; private set; } = Subtract;
    
    // x is not included, only y. naturally, because we 
    // "move along" the image horizontally. that's why x
    // is the parameter passed to Apply method
    private int _y;
    private int _height;
    private int _width;
    
    
    public int FilterType
    {
        get => _filterType;
        set
        {
            switch (value)
            {
                case 0:
                    ApplyFunc = Filter0;
                    break;
                case 1:
                    ApplyFunc = Filter1;
                    break;
                case 2:
                    ApplyFunc = Filter2;
                    break;
                case 3:
                    ApplyFunc = Filter3;
                    break;
                case 4:
                    ApplyFunc = Filter4;
                    break;
                case 5:
                    ApplyFunc = Filter5;
                    break;
                default:
                    ApplyFunc = Filter0;
                    break;
            }
            _filterType = value;
        }
    }

    public int CompareType
    {
        get => _compareType;
        set
        {
            switch (value)
            {
                case 0:
                    CompareFunc = Subtract;
                    break;
                case 1:
                    CompareFunc = SubtractLog;
                    break;
                default:
                    CompareFunc = Subtract;
                    break;
            }
            _compareType = value;
        }
    }

    public int Y
    {
        get => _y;
        set => _y = value;
    }

    public int Height
    {
        get => _height;
        set => _height = value;
    }

    public int Width
    {
        get => _width;
        set => _width = value;
    }

    public Filter(int y = 0, int height = 0, int width = 0, int filterType = 0, int cmpType = 1)
    {
        CompareType = cmpType;
        FilterType = filterType;
        _y = y;
        _height = height;
        _width = width;
    }
    
    /// <summary>
    /// Apply the filter to an image
    /// </summary>
    /// <param name="image"></param>
    /// <param name="x">
    /// Horizontal offset, starting from
    /// which the filter will be applied
    /// </param>
    /// <returns></returns>
    public double Apply(IntegralImage image, int x)
    {
        return ApplyFunc(image, x, _y, _width, _height, CompareFunc);
    }

    #region Filter functions
    // An effort has been made to make these functions
    // to be easily applicable, but in the context of 
    // a class, as these methods should've not been 
    // static. But in order to have an ability to use
    // these methods without the need of creating an
    // instance of Filter class, these methods must be
    // static.

    /// <summary>
    /// Implements a filter application function: <br/>
    /// # # # # # # # # # # # # <br/>
    /// # # # # # # # # # # # # <br/>
    /// # # # # # # # # # # # # <br/>
    /// # # # # # # # # # # # # <br/>
    /// # # # # # # # # # # # # <br/>
    /// # # # # # # # # # # # # <br/>
    /// </summary>
    public static double Filter0(IntegralImage image, int x, int y, int w, int h, NumComparer cmp)
    {
        double a = image.Area(x, y, w - 1, y + h - 1);
        double b = 0;
        return cmp(a, b);
    }

    /// <summary>
    /// Implements a filter application function: <br/>
    /// . . . . . . . . . . . . <br/>
    /// . . . . . . . . . . . . <br/>
    /// . . . . . . . . . . . . <br/>
    /// # # # # # # # # # # # # <br/>
    /// # # # # # # # # # # # # <br/>
    /// # # # # # # # # # # # # <br/>
    /// </summary>
    public static double Filter1(IntegralImage image, int x, int y, int w, int h, NumComparer cmp)
    {
        int h_2 = h / 2;

        double a = image.Area(x, y + h_2, x + w - 1, y + h - 1);
        double b = image.Area(x, y, x + w - 1, y + h_2 - 1);

        return cmp(a, b);
    }
    
    /// <summary>
    /// Implements a filter application function: <br/>
    /// . . . . . . # # # # # # <br/>
    /// . . . . . . # # # # # # <br/>
    /// . . . . . . # # # # # # <br/>
    /// . . . . . . # # # # # # <br/>
    /// . . . . . . # # # # # # <br/>
    /// . . . . . . # # # # # # <br/>
    /// </summary>
    public static double Filter2(IntegralImage image, int x, int y, int w, int h, NumComparer cmp)
    {
        int w_2 = w / 2;

        double a = image.Area(x + w_2, y, x + w - 1, y + h - 1);
        double b = image.Area(x, y, x + w_2 - 1, y + h - 1);

        return cmp(a, b);
    }
    
    /// <summary>
    /// Implements a filter application function: <br/>
    /// . . . . . . # # # # # # <br/>
    /// . . . . . . # # # # # # <br/>
    /// . . . . . . # # # # # # <br/>
    /// # # # # # # . . . . . . <br/>
    /// # # # # # # . . . . . . <br/>
    /// # # # # # # . . . . . . <br/>
    /// </summary>
    public static double Filter3(IntegralImage image, int x, int y, int w, int h, NumComparer cmp)
    {
        int w_2 = w / 2;
        int h_2 = h / 2;

        double a = image.Area(x, y + h_2, x + w_2 - 1, y + h - 1) +
                   image.Area(x + w_2, y, x + w - 1, y + h_2 - 1);
        double b = image.Area(x, y, x + w_2 - 1, y + h_2 - 1) +
                   image.Area(x + w_2, y + h_2, x + w - 1, y + h - 1);

        return cmp(a, b);
    }

    /// <summary>
    /// Implements a filter application function: <br/>
    /// . . . . . . . . . . . . <br/>
    /// . . . . . . . . . . . . <br/>
    /// # # # # # # # # # # # # <br/>
    /// # # # # # # # # # # # # <br/>
    /// . . . . . . . . . . . . <br/>
    /// . . . . . . . . . . . . <br/>
    /// </summary>
    public static double Filter4(IntegralImage image, int x, int y, int w, int h, NumComparer cmp)
    {
        int h_3 = h / 3;

        double a = image.Area(x, y + h_3, x + w - 1, y + 2 * h_3 - 1);
        double b = image.Area(x, y, x + w - 1, y + h_3 - 1) +
                   image.Area(x, y + 2 * h_3, x + w - 1, y + h - 1);

        return cmp(a, b);
    }
    
    /// <summary>
    /// Implements a filter application function: <br/>
    /// . . . . # # # # . . . . <br/>
    /// . . . . # # # # . . . . <br/>
    /// . . . . # # # # . . . . <br/>
    /// . . . . # # # # . . . . <br/>
    /// . . . . # # # # . . . . <br/>
    /// . . . . # # # # . . . . <br/>
    /// </summary>
    public static double Filter5(IntegralImage image, int x, int y, int w, int h, NumComparer cmp)
    {
        int w_3 = w / 3;

        double a = image.Area(x + w_3, y, x + 2 * w_3 - 1, y + h - 1);
        double b = image.Area(x, y, x + w_3 - 1, y + h - 1) +
                   image.Area(x + 2 * w_3, y, x + w - 1, y + h - 1);
        
        return cmp(a, b);
    }

    #endregion

    #region CompareFunc functions

    /// <summary>
    /// Implements basic CompareFunc function: subtracts a from b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static double Subtract(double a, double b)
    {
        return a - b;
    }

    /// <summary>
    /// Implements comparison based on calculating a logarithm
    /// of the provided numbers
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static double SubtractLog(double a, double b)
    {
        double r = Math.Log((1.0 + a) / (1.0 + b));
        
        if (double.IsNaN(r))
        {
            throw new Exception("Filter: SubtractLog = NaN");
        }

        return r;
    }

    #endregion
}
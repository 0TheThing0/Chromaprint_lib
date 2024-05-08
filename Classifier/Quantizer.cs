namespace Chromaprint.Classifier;

/// <summary>
/// Class for matching a real number to a
/// specific range represented by an integer
/// number based on the value of a real number
/// </summary>
public class Quantizer
{
    private double _t0;
    private double _t1;
    private double _t2;

    public Quantizer(double t0, double t1, double t2)
    {
        _t0 = t0;
        _t1 = t1;
        _t2 = t2;

        if (t0 > t1 || t1 > t2)
        {
            throw new ArgumentException("Invalid bound correlation. Expected: t0 < t1 < t2");
        }
    }

    /// <summary>
    /// Retrieve an index for the quantizer range of the provided value 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int Quantize(double value)
    {
        if (value < _t1)
        {
            if (value < _t0)
            {
                return 0;
            }
            return 1;
        }
        else
        {
            if (value < _t2)
            {
                return 2;
            }

            return 3;
        }
    }
}
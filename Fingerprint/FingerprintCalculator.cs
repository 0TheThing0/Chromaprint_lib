using Chromaprint.Image;

namespace Chromaprint.Fingerprint;

/// <summary>
/// Class that allows to convert results of
/// <see cref="FileFingerprinter"/> into an array
/// of data of type int
/// </summary>
public class FingerprintCalculator
{
    /// <summary>
    /// Gray code that is used in encoding
    /// the classified data
    /// </summary>
    /// <remarks>https://en.wikipedia.org/wiki/Gray_code</remarks>
    public static uint[] GrayCode = { 0, 1, 3, 2 };

    private Classifier.Classifier[] _classifiers;
    private int _numClassifiers;
    private int _maxFilterWidth;

    public FingerprintCalculator(Classifier.Classifier[] classifiers)
    {
        _classifiers = classifiers;
        _numClassifiers = classifiers.Length;
        _maxFilterWidth = 0;
        for (int i = 0; i < _numClassifiers; i++)
        {
            _maxFilterWidth = Math.Max(_maxFilterWidth, classifiers[i].Filter.Width);
        }

        if (!(_maxFilterWidth > 0 && _maxFilterWidth < 256))
        {
            throw new Exception(
                "Max filter width in FingerprintCalculator instance is out of bounds. Expected: (0;255) ");
        }
    }

    /// <summary>
    /// Calculates the fingerprint and returns it as an array
    /// of gray-code-encoded array 
    /// </summary>
    /// <param name="image"></param>
    /// <returns>
    /// If the amount of provided data
    /// is less than the maximum width
    /// of the filters of the calculator,
    /// the array is empty</returns>
    public int[] Calculate(Image.Image image)
    {
        // estimating the amount of classifications needed
        // (-maxFilterWidth, as it must move as a window, 
        // and window must be applied to existing data...)
        int length = image.Rows - _maxFilterWidth + 1;
        if (length <= 0)
        {
            return Array.Empty<int>();
        }

        IntegralImage integralImage = new IntegralImage(image);
        int[] fingerprint = new int[length];
        for (int i = 0; i < length; i++)
        {
            fingerprint[i] = CalculateSubfingerprint(integralImage, i);
        }

        return fingerprint;
    }

    /// <summary>
    /// Calculate fingerprint for given image and an offset
    /// (expected that the current calculator configuration
    /// allows for fingerprint to fit into 32 bits)
    /// </summary>
    /// <param name="image"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private int CalculateSubfingerprint(IntegralImage image, int offset)
    {
        uint bits = 0;
        for (int i = 0; i < _numClassifiers; i++)
        {
            bits = (bits << 2) | GrayCode[_classifiers[i].Classify(image, offset)];
        }

        return (int)bits;
    }
}
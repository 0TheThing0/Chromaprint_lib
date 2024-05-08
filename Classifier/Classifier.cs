using Chromaprint.Image;

namespace Chromaprint.Classifier;

/// <summary>
/// Class that encapsulates the functionality of <see cref="Filter"/>
/// and <see cref="Quantizer"/>. Mostly used in <see cref="FingerprintCalculator"/>
/// to calculate sub-fingerprints of the resulting image 
/// </summary>
public class Classifier
{
    private Filter _filter;
    private Quantizer _quantizer;

    // Property that will be used to retrieve 
    // max filter width in FingerprintCalculator
    // TODO: come up with a more-or-less-normal-looking solution for this problem
    internal Filter Filter
    {
        get =>_filter;
    }

    public Classifier()
        : this(new Filter(), new Quantizer(0.0, 0.0, 0.0))
    {
    }

    public Classifier(Filter filter, Quantizer quantizer)
    {
        _filter = filter;
        _quantizer = quantizer;
    }

    public int Classify(IntegralImage image, int offset)
    {
        double value = _filter.Apply(image, offset);
        return _quantizer.Quantize(value);
    }
}
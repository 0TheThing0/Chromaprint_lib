using Chromaprint.Chroma;
using Chromaprint.Fingerprint;

namespace Chromaprint.Image;

/// <summary>
/// The main purpose of this class is to
/// build the resulting chromaprint image.
/// Used in <see cref="FileFingerprinter"/>
/// </summary>
public class ImageBuilder : IFeatureVectorConsumer
{
    private Image _image;

    public Image Image
    {
        get => _image;
        set => _image = value;
    }

    public ImageBuilder(Image image)
    {
        _image = image;
    }

    public void Reset(Image image)
    {
        _image = image;
    }

    public void Consume(double[] features)
    {
        _image.AddRow(features);
    }
}
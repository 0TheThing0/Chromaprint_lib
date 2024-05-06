namespace Chromaprint.Chroma;
/// <summary>
/// Consumer of features produced by Chroma.
/// </summary>
public interface IFeatureVectorConsumer
{
    public void Consume(double[] features);
}
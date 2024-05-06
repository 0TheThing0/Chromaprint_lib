namespace Chromaprint.Chroma;
using Chromaprint.Utilities;

/// <summary>
/// Class for normalizing chroma data (supposedly provided
/// by <see cref="ChromaFilter"/>. Current implementation of
/// this class is basically a one-line method, but with the
/// purpose of clear division of responsibilities and flexibility
/// of the component, the decision has been made to leave it as
/// a separate class, as it is in the original implementation 
/// </summary>
public class ChromaNormalizer : IFeatureVectorConsumer
{
    // Supposedly, the next stage of fingerprinting 
    // pipeline must be ImageBuilder itself
    private IFeatureVectorConsumer _consumer;

    public ChromaNormalizer(IFeatureVectorConsumer consumer)
    {
        _consumer = consumer;
    }

    public void Reset()
    {
    }

    public void Consume(double[] features)
    {
        Helper.NormalizeVector(features, Helper.EuclideanNorm(features), 0.01);
        
        _consumer.Consume(features);
    }
}
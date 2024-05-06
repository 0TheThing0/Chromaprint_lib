namespace Chromaprint.Chroma;

/// <summary>
/// Class for filtering the data acquired from <see cref="Chroma"/>
/// </summary>
public class ChromaFilter : IFeatureVectorConsumer
{
    // Filtering is based on coefficients that performed
    // best during training phase of machine learning.
    private double[] _coefficients;
    private int _length;
    private double[][] _buffer;
    private double[] _result;
    private int _buffer_offset;
    private int _buffer_size;
    
    // Supposedly, the next stage of fingerprinting 
    // pipeline must be ChromaNormalizer
    private IFeatureVectorConsumer _consumer;

    public ChromaFilter(double[] coefficients, IFeatureVectorConsumer consumer)
    {
        _coefficients = coefficients;
        _length = coefficients.Length;
        _buffer = new double[8][];
        _result = new double[Chroma.NUM_BANDS];
        _buffer_offset = 0;
        _buffer_size = 1;
        _consumer = consumer;
    }

    public void Reset()
    {
        _buffer_offset = 0;
        _buffer_size = 1;
    }
    
    public void Consume(double[] features)
    {
        // 12-element array is expected from Chroma
        // (actually, NUM_BANDS-element array)
        _buffer[_buffer_offset] = features;
        _buffer_offset = (_buffer_offset + 1) & 0b111;  // equiv. to % 8
        
        if (_buffer_size >= _length)
        {
            // acquiring the start point from the current position
            int offset = (_buffer_offset + 8 - _length) & 0b111;

            for (int i = 0; i < Chroma.NUM_BANDS; i++)
            {
                _result[i] = 0;
                for (int j = 0; j < _length; j++)
                {
                    _result[i] += _buffer[(offset + j) & 0b111][i] * _coefficients[j];
                }
            }
            
            _consumer.Consume(_result);
        }
        else
        {
            _buffer_size++;
        }
    }
}
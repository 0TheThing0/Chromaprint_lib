using Chromaprint.FFT;

namespace Chromaprint.Chroma;

/// <summary>
/// Class for Chroma algorithm
/// <seealso href="http://dub.ucsd.edu/CATbox/Reader/ThumbnailingMM05.pdf">Algorithm explanation</seealso>
/// </summary>
public class Chroma : IFFTFrameConsumer
{
    public static readonly int NUM_BANDS = 12;
    bool _interpolate;
    private int[] _notes;
    private double[] _notesFrac;
    private int _minIndex;
    private int _maxIndex;
    double[] _features;
    private IFeatureVectorConsumer _consumer;

    /// <summary>
    /// Setting for including interpolation
    /// </summary>
    public bool Interpolate
    {
        get => _interpolate;
        set => _interpolate = value;
    }

    /// <summary>
    /// Initialize the chroma
    /// </summary>
    /// <param name="minFreq">Lowest frequency for chroma</param>
    /// <param name="maxFreq">Highest frequency for chroma</param>
    /// <param name="frameSize">Size of frame for consuming</param>
    /// <param name="sampleRate"></param>
    /// <param name="consumer">Consumer of chroma result</param>
    public Chroma(int minFreq, int maxFreq, int frameSize, int sampleRate, IFeatureVectorConsumer consumer)
    {
        _interpolate = false;
        _notes = new int[frameSize];
        _notesFrac = new double[frameSize];
        _features = new double[NUM_BANDS];
        _consumer = consumer;

        PrepareNotes(minFreq,maxFreq,frameSize,sampleRate);
    }

    /// <summary>
    /// Associate frame indexes with notes
    /// </summary>
    /// <param name="minFreq">Lowest frequency for chroma</param>
    /// <param name="maxFreq">Highest frequency for chroma</param>
    /// <param name="frameSize">Size of frame for associating</param>
    /// <param name="sampleRate"></param>
    private void PrepareNotes(int minFreq, int maxFreq, int frameSize, int sampleRate)
    {
        //Counting minIndex and maxIndex for set scope for frame processing
        _minIndex = Math.Min(1, Helper.FreqToIndex(minFreq, frameSize, sampleRate));
        _maxIndex = Math.Min(frameSize / 2, Helper.FreqToIndex(maxFreq, frameSize, sampleRate));

        //Each frequency in FFT frame is (i * sampleRate) / frameSize; 
        double step = (double)sampleRate / frameSize;
        double freq = step * _minIndex;
        
        for (int i = _minIndex; i < _maxIndex; i++)
        {
            double octave = Helper.FreqToOctave(freq);
            double note = NUM_BANDS * (octave - Math.Floor(octave));
            
            _notes[i] = (int)note;
            _notesFrac[i] = note - _notes[i];
            freq += step;
        }
    }

    public void Consume(double[] frame)
    {
        //We must recreate array each time to use it correctly in ChromaFilter
        _features = new double[NUM_BANDS];
        
        for (int i = _minIndex; i < _maxIndex; i++)
        {
            int note = _notes[i];
            double energy = frame[i];
            if (_interpolate)
            {
                //If we use interpolate we must count near note to adjust
                //energy from note
                int nearNote = note;
                double k = 1.0;
                if (_notesFrac[i] < 0.5)
                {
                    //We need to take prev note
                    nearNote = (note + NUM_BANDS - 1) % NUM_BANDS;
                    k = 0.5 * _notesFrac[i];
                } 
                else if (_notesFrac[i] > 0.5)
                {
                    //We need to take next note
                    nearNote = (note + 1) % NUM_BANDS;
                    k = 1.5 - _notesFrac[i];
                }

                _features[note] += energy * k;
                _features[nearNote] += energy * (1.0 - k);
            }
            else
            {
                _features[note] += energy;
            }
        }
        _consumer.Consume(_features);
    }
}
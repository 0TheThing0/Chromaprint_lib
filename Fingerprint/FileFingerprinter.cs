
using Chromaprint.Audio;
using Chromaprint.Chroma;
using Chromaprint.FFT;
using Chromaprint.Image;

namespace Chromaprint.Fingerprint;

/// <summary>
/// Class that encapsulates all the logic of
/// computing a fingerprint for a file
/// (assuming that the data will be provided
/// from file itself using Consume() method of
/// FileFingerprinter)
/// </summary>
public class FileFingerprinter : IAudioConsumer
{
    public static readonly int SAMPLE_RATE = 11025;
    public static readonly int FRAME_SIZE = 4096;
    public static readonly int OVERLAP = FRAME_SIZE - FRAME_SIZE / 3;
    public static readonly int MIN_FREQ = 28;
    public static readonly int MAX_FREQ = 3520;
    
    // fields for FileReader instance
    public static readonly int BUFFER_SIZE = 4096;
    public static readonly int BIT_DEPTH = 16;
    public static readonly int NUM_CHANNELS = 1;
    
    private Image.Image _image;
    private ImageBuilder _imageBuilder;
    private Chroma.Chroma _chroma;
    private ChromaNormalizer _chromaNormalizer;
    private ChromaFilter _chromaFilter;
    private FFT.FFT _fft;
    private FingerprintCalculator _fingerprintCalculator;
    private FingerprinterConfiguration _fingerprinterConfiguration;
    private SilenceRemover.SilenceRemover _silenceRemover;
    private IAudioConsumer _consumer;
    
    public FileFingerprinter(FingerprinterConfiguration config, IFFTService fftService)
    {
        _image = new Image.Image(12);
        if (config == null)
        {
            config = new FingerprinterConfiguration1();
        }

        _imageBuilder = new ImageBuilder(_image);
        _chromaNormalizer = new ChromaNormalizer(_imageBuilder);
        _chromaFilter = new ChromaFilter(config.FilterCoefficients, _chromaNormalizer);
        _chroma = new Chroma.Chroma(MIN_FREQ, MAX_FREQ, FRAME_SIZE, SAMPLE_RATE, _chromaFilter);
        _fft = new FFT.FFT(FRAME_SIZE, OVERLAP, fftService, _chroma);
        if (config.RemoveSilence)
        {
            _silenceRemover = new SilenceRemover.SilenceRemover(_fft);
            _silenceRemover.Threshold = config.SilenceThreshold;
            _consumer = _silenceRemover;
        }
        else
        {
            _silenceRemover = null;
            _consumer = _fft;
        }

        _fingerprintCalculator = new FingerprintCalculator(config.Classifiers);
        _fingerprinterConfiguration = config;
    }

    /// <summary>
    /// Sets an option of some certain parameter of fingerprinter
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// For now, the only parameter that is modifiable
    /// is the silence threshold
    /// </remarks>
    public bool SetOption(string name, int value)
    {
        if (name.Equals("silence_threshold"))
        {
            if (_silenceRemover != null)
            {
                _silenceRemover.Threshold = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Initialize the fingerprinting process
    /// </summary>
    /// <param name="sampleRate"></param>
    /// <param name="numChannels"></param>
    /// <returns></returns>
    public bool Start()
    {
        // There should've been a reset for the 
        // audio processor, but assuming that the
        // input comes from an external source
        // where the data is already pre-processed,
        // this always returns true 
        _fft.Reset();
        _chroma.Reset();
        _chromaFilter.Reset();
        _chromaNormalizer.Reset();
        _image = new Image.Image(12);
        _imageBuilder.Reset(_image);
        
        return true;
    }

    /// <summary>
    /// Process a block of audio data
    /// </summary>
    /// <param name="input"></param>
    /// <param name="length"></param>
    /// <exception cref="ArgumentException"></exception>
    public void Consume(short[] input, int length)
    {
        if (length < 0)
        {
            throw new ArgumentException("Length must be positive");
        }

        _consumer.Consume(input, length);
    }

    /// <summary>
    /// Calculate fingerprint based on the provided data
    /// </summary>
    /// <returns>Array of 32-bit data that can be easily displayed as an image</returns>
    public int[] Finish()
    {
        return _fingerprintCalculator.Calculate(_image);
    }
}
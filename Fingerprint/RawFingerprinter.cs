using Chromaprint.Audio;
using Chromaprint.Chroma;
using Chromaprint.FFT;
using Chromaprint.Image;

namespace Chromaprint.Fingerprint;

public class RawFingerprinter : IAudioConsumer
{
    private static readonly int SAMPLE_RATE = 11025;
    private static readonly int FRAME_SIZE = 4096;
    private static readonly int OVERLAP = FRAME_SIZE - FRAME_SIZE / 3;
    private static readonly int MIN_FREQ = 28;
    private static readonly int MAX_FREQ = 3520;

    private Image.Image _image;
    private readonly ImageBuilder _imageBuilder;
    private readonly Chroma.Chroma _chroma;
    private readonly ChromaNormalizer _chromaNormalizer;
    private readonly ChromaFilter _chromaFilter;
    private readonly FFT.FFT _fft;
    private readonly AudioProcessor _audioProcessor;
    private readonly FingerprintCalculator _fingerprintCalculator;
    private FingerprinterConfiguration _config;
    private readonly SilenceRemover.SilenceRemover _silenceRemover;

    public RawFingerprinter(FingerprinterConfiguration config, IFFTService fftService)
    {
        _image = new Image.Image(12);
        if (config == null) config = new FingerprinterConfiguration1();

        _imageBuilder = new ImageBuilder(_image);
        _chromaNormalizer = new ChromaNormalizer(_imageBuilder);
        _chromaFilter = new ChromaFilter(config.FilterCoefficients, _chromaNormalizer);
        _chroma = new Chroma.Chroma(MIN_FREQ, MAX_FREQ, FRAME_SIZE, SAMPLE_RATE, _chromaFilter);

        _fft = new FFT.FFT(FRAME_SIZE, OVERLAP, fftService, _chroma);
        if (config.RemoveSilence)
        {
            _silenceRemover = new SilenceRemover.SilenceRemover(_fft);
            _silenceRemover.Threshold = config.SilenceThreshold;
            _audioProcessor = new AudioProcessor(SAMPLE_RATE, _silenceRemover);
        }
        else
        {
            _silenceRemover = null;
            _audioProcessor = new AudioProcessor(SAMPLE_RATE, _fft);
        }

        _fingerprintCalculator = new FingerprintCalculator(config.Classifiers);
        _config = config;
    }

    /// <summary>
    ///     Sets an option of some certain parameter of fingerprinter
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    ///     For now, the only parameter that is modifiable
    ///     is the silence threshold
    /// </remarks>
    public bool SetOption(string name, int value)
    {
        if (name.Equals("silence_threshold"))
            if (_silenceRemover != null)
            {
                _silenceRemover.Threshold = value;
                return true;
            }

        return false;
    }


    /// <summary>
    ///     Initialize the fingerprinting process
    /// </summary>
    /// <returns></returns>
    public bool Start(int sampleRate, int numChannels)
    {
        if (!_audioProcessor.Reset(sampleRate, numChannels))
            // audio processor initialization failed
            return false;

        _fft.Reset();
        _chroma.Reset();
        _chromaFilter.Reset();
        _chromaNormalizer.Reset();
        _image = new Image.Image(12);
        _imageBuilder.Reset(_image);

        return true;
    }

    /// <summary>
    ///     Process a block of audio data
    /// </summary>
    public void Consume(short[] input, int length)
    {
        if (length < 0) throw new AggregateException("Length must be positive");

        _audioProcessor.Consume(input, length);
    }

    /// <summary>
    ///     Calculate the fingerprint based on the provided data
    /// </summary>
    /// <returns>Array of 32-bit data that can be easily displayed as an image</returns>
    public int[] Finish()
    {
        _audioProcessor.Flush();
        return _fingerprintCalculator.Calculate(_image);
    }
}
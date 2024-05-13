namespace Chromaprint.Audio;

/// <summary>
///     Audio processor with multi-channel to mono
///     conversion an resampling. Typically used
///     as an entry for <see cref="RawFingerprinter" />
/// </summary>
public class AudioProcessor : IAudioConsumer
{
    private static readonly int minSampleRate = 1000;
    private static readonly int maxBufferSize = 1024 * 16;

    // configuration fields
    private static readonly int resampleFilterLength = 16;
    private static readonly int resamplePhaseCount = 10;
    private static readonly bool resampleLinear = false;
    private static readonly double resampleCutoff = 0.8;

    private readonly short[] _buffer;
    private readonly short[] _resampleBuffer;
    private int _bufferOffset;
    private readonly int _bufferSize;
    private int _numChannels;

    private Resampler _resampler;

    public int TargetSampleRate { get; set; }

    public IAudioConsumer Consumer { get; set; }

    public AudioProcessor(int sampleRate, IAudioConsumer consumer)
    {
        _bufferSize = maxBufferSize;
        TargetSampleRate = sampleRate;
        Consumer = consumer;

        _buffer = new short[maxBufferSize];
        _bufferOffset = 0;
        _resampleBuffer = new short[maxBufferSize];
    }

    #region Public methods

    /// <summary>
    ///     Prepare for a new audio stream
    /// </summary>
    /// <returns></returns>
    public bool Reset(int sampleRate, int numChannels)
    {
        if (numChannels <= 0)
            // error: no audio channels set
            return false;

        if (sampleRate <= minSampleRate)
            // error: sample rate mustn't be less than minSampleRate
            return false;

        _bufferOffset = 0;
        if (_resampler != null)
        {
            _resampler.Close();
            _resampler = null;
        }

        // if the provided values turned out to be valid, 
        // assign them

        if (sampleRate != TargetSampleRate)
        {
            _resampler = new Resampler();
            _resampler.Init(
                TargetSampleRate, sampleRate,
                resampleFilterLength,
                resamplePhaseCount,
                resampleLinear,
                resampleCutoff);
        }

        _numChannels = numChannels;
        return true;
    }

    /// <summary>
    ///     Process a chunk of data from the audio stream
    /// </summary>
    /// <param name="input"></param>
    /// <param name="length"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Consume(short[] input, int length)
    {
        if (length < 0 || length % _numChannels != 0) throw new ArgumentException("Input length is not valid");

        var offset = 0;
        length /= _numChannels;

        while (length > 0)
        {
            var consumed = Load(input, offset, length);
            offset += consumed * _numChannels;
            length -= consumed;

            if (_bufferSize == _bufferOffset)
            {
                Resample();
                if (_bufferSize == _bufferOffset)
                    // something in the resampling process went wrong...
                    return;
            }
        }

        throw new NotImplementedException();
    }

    public void Flush()
    {
        if (_bufferOffset > 0) Resample();
    }

    #endregion

    #region Private methods

    private int Load(short[] input, int offset, int length)
    {
        if (length < 0 || _bufferOffset > _bufferSize) throw new Exception("Something unexpected happened");

        length = Math.Min(length, _bufferSize - _bufferOffset);
        switch (_numChannels)
        {
            case 1:
                LoadMono(input, offset, length);
                break;
            case 2:
                LoadStereo(input, offset, length);
                break;
            default:
                LoadMultiChannel(input, offset, length);
                break;
        }

        _bufferOffset += length;
        return length;
    }

    private void LoadMono(short[] input, int offset, int length)
    {
        var i = _bufferOffset;
        var j = 0;

        while (length-- > 0)
        {
            _buffer[i + j++] = input[offset];
            offset++;
        }
    }

    private void LoadStereo(short[] input, int offset, int length)
    {
        var i = _bufferOffset;
        var j = 0;
        while (length-- > 0)
        {
            _buffer[i + j++] = (short)((input[offset] + input[offset + 1]) / 2);
            offset += 2;
        }
    }

    private void LoadMultiChannel(short[] input, int offset, int length)
    {
        var i = _bufferOffset;
        var j = 0;

        long sum;
        while (length-- > 0)
        {
            sum = 0;
            for (var c = 0; c < _numChannels; c++) sum += input[offset++];

            _buffer[i + j++] = (short)(sum / _numChannels);
        }
    }

    private void Resample()
    {
        if (_resampler == null)
        {
            Consumer.Consume(_buffer, _bufferOffset);
            _bufferOffset = 0;
        }
        else
        {
            var consumed = 0;
            var length = _resampler.Resample(_resampleBuffer, _buffer,
                ref consumed, _bufferOffset,
                maxBufferSize, true);

            if (length > maxBufferSize)
                // error: resampling overwrote output buffer
                length = maxBufferSize;

            Consumer.Consume(_resampleBuffer, length);
            var remaining = _bufferOffset - consumed;
            if (remaining > 0)
                Array.Copy(_buffer, consumed, _buffer, 0, _bufferOffset - consumed);
            else if (remaining < 0)
                // error: resampling overread input buffer
                remaining = 0;

            _bufferOffset = remaining;
        }
    }

    #endregion
}
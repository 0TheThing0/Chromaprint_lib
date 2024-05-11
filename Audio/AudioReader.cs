using NAudio.Wave;

namespace Chromaprint.Audio;

/// <summary>
/// Class for getting data from audio file and covert it if necessary format
/// </summary>
public class AudioReader
{
    private string _filePath;
    private IWaveProvider _stream;
    private bool _resample;
    private WaveFormat _waveFormat;
    private int _bufferSize;
    private IAudioConsumer _consumer;
    private byte[] _bytes;
    private short[] _data;
   
    /// <summary>
    /// Initialize reader without resampling
    /// </summary>
    /// <param name="bufferSize">Size of buffer in bytes</param>
    /// <param name="consumer">Result consumer</param>
    public AudioReader(int bufferSize,IAudioConsumer consumer)
    {
        _resample = false;
        _bufferSize = bufferSize;
        _consumer = consumer;
        _bytes = new byte[_bufferSize];
        _data = new short[_bufferSize/2];
    }

    /// <summary>
    /// Initialize reader with resampling format
    /// </summary>
    /// <param name="bufferSize">Size of buffer in bytes</param>
    /// <param name="consumer">Result consumer</param>
    /// <param name="sampleRate"></param>
    /// <param name="bitsDepth"></param>
    /// <param name="channels"></param>
    public AudioReader(int bufferSize, int sampleRate, int bitsDepth, int channels,IAudioConsumer consumer)
    {
        _bufferSize = bufferSize;
        _resample = true;
        _waveFormat = new WaveFormat(sampleRate, bitsDepth, channels);
        _consumer = consumer;
        _bytes = new byte[_bufferSize];
        _data = new short[_bufferSize/2];
    }
    
    /// <summary>
    /// Initializing resample parameter 
    /// </summary>
    /// <param name="sampleRate"></param>
    /// <param name="bitsDepth"></param>
    /// <param name="channels"></param>
    public void SetWaveFormat(int sampleRate, int bitsDepth, int channels)
    {
        _resample = true;
        _waveFormat = new WaveFormat(sampleRate, bitsDepth, channels);
    }

    /// <summary>
    ///     Resets the audio converter of audio reader instance,
    ///     unsetting the wave format, reset indicator and
    ///     IWaveProvider stream
    /// </summary>
    public void Reset()
    {
        _stream = null;
        _resample = false;
        _waveFormat = null;
    }


    // TODO: make it return a bool value: result of trying to 
    // open a file?
    
    /// <summary>
    /// Binding audio file to AudioReader
    /// </summary>
    /// <param name="path"></param>
    public void SetFile(string path)
    {
        _filePath = path;
        WaveStream fileStream = new AudioFileReader(_filePath);
        if (_resample)
        {
            _stream = new MediaFoundationResampler(fileStream,_waveFormat);
        }
        else
        {
            _stream = fileStream;
        }
    }

    /// <summary>
    /// Read all data from file and send it to the consumer
    /// </summary>
    public void ReadAll()
    {
        int readedBytes = 0;
        do
        {
            readedBytes = _stream.Read(_bytes, 0, _bufferSize);
            Buffer.BlockCopy(_bytes, 0, _data, 0, readedBytes);
            _consumer.Consume(_data,readedBytes/2);
        } while (readedBytes == _bufferSize);
    }
}
using Chromaprint.Fingerprint;
using NAudio.Wave;

namespace Chromaprint.Audio;

/// <summary>
/// Class for getting data from audio file and covert it if necessary format
/// </summary>
public class AudioReader
{
    // for now, the need to know the amount of 
    // calculated fingerprint data arose (see ReadAll method).
    // for that, it needs access to filefingerprinter, 
    // not an interface. this needs to change.

    // private IAudioConsumer _consumer;
    private readonly FileFingerprinter _fp;
    
    
    private string _filePath;
    private bool _resample;
    private WaveFormat _waveFormat;
    private int _bufferSize;
    private byte[] _bytes;
    private short[] _data;
   
    /// <summary>
    /// Initialize reader without resampling
    /// </summary>
    /// <param name="bufferSize">Size of buffer in bytes</param>
    /// <param name="consumer">Result consumer</param>
    public AudioReader(int bufferSize, FileFingerprinter fingerprinter)
    {
        _resample = false;
        _bufferSize = bufferSize;
        _fp = fingerprinter;
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
    public AudioReader(int bufferSize, int sampleRate, int bitsDepth, int channels, FileFingerprinter fingerprinter)
    {
        _bufferSize = bufferSize;
        _resample = true;
        _waveFormat = new WaveFormat(sampleRate, bitsDepth, channels);
        _fp = fingerprinter;
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
        _resample = false;
        _waveFormat = null;
    }


    /// <summary>
    /// Binding audio file to AudioReader
    /// </summary>
    /// <param name="path">Path to audio file</param>
    /// <returns>Status determining if file exists or not</returns>
    public bool SetFile(string path)
    {
        var status = File.Exists(path);
        if (status) _filePath = path;

        // TODO: decide if this implementation for SetFile is enough (meaning, checking 
        // only if file exists). Issue: AudioFileReader itself checks the file only
        // looking at extension and cannot verify if it is a valid (for ex.) .wav file
        // without looking at headers, which makes sense, but at the same time it 
        // doesn't provide any functionality on checking the file before initializing
        // the audio stream
        return status;
    }

    /// <summary>
    /// Read all data from file and send it to the consumer
    /// Read certain amount of data from file so that the size of fingerprint is satisfying and send it to the consumer
    /// </summary>
    public bool ReadFile(int? desiredFPSize = null)
    {
        // Developer note: the desired fingerprint size indeed can be
        // calculated beforehand. But there's a catch: it works in the
        // implementations where there's no silence remover. And in the
        // silence-remover-utilizing implementation desired fingerprint
        // size can be achieved by checking the current amount of fingerprint
        // components calculated on each iteration, which is basically
        // implemented here. So a choice has been made to make it general
        // for any implementation, otherwise it will make the whole
        // structure much more complex and less readable
        
        var status = true;
        WaveStream fileReader = null;
        IWaveProvider stream = null;
        
        try
        {
            fileReader = new MediaFoundationReader(_filePath);
            
            if (_resample)
                stream = new MediaFoundationResampler(fileReader, _waveFormat);
            else
                stream = fileReader;
        }
        catch (Exception ex)
        {
            status = false;
        }

        if (status && stream != null)
        {
            var readedBytes = 0;
            do
            {
                readedBytes = stream.Read(_bytes, 0, _bufferSize);
                Buffer.BlockCopy(_bytes, 0, _data, 0, readedBytes);
                _fp.Consume(_data, readedBytes / 2);
            } while (readedBytes == _bufferSize && desiredFPSize?.CompareTo(_fp.GetReadyFPSize()) > 0);
        }

        fileReader?.Dispose();
        
        return status;
    }
}
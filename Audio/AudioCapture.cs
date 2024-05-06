using System.Collections.Generic;
using System.IO;
using NAudio.Wave;

namespace Audio;

/// <summary>
/// Capture sound from input device
/// </summary>
public class AudioCapture
{
    private int _rate;
    private int _bits;
    private int _channels;
    private WaveFormat _waveFormat;
    private MemoryStream _recordedSound;
    private WaveInEvent _waveInEvent;

    /// <summary>
    /// Initialize the audio capture
    /// </summary>
    /// <param name="rate">Sample rate</param>
    /// <param name="bits">Bits amount for encoding</param>
    /// <param name="channels">Amount of channels</param>
    public AudioCapture(int rate, int bits, int channels)
    {
        this._rate = rate;
        this._bits = bits;
        this._channels = channels;
        this._waveFormat = new WaveFormat(this._rate, this._bits, this._channels);
        this._recordedSound = new MemoryStream();
        
        this._waveInEvent = new WaveInEvent()
        {
            BufferMilliseconds = 100,
            WaveFormat = _waveFormat
        };
    }

    /// <summary>
    /// Get list of all available devices for capturing sound
    /// </summary>
    /// <returns>List of devices</returns>
    public IEnumerable<WaveInCapabilities> GetDevices()
    {
        int waveInDevices = WaveInEvent.DeviceCount;
        List<WaveInCapabilities> answer = new List<WaveInCapabilities>(waveInDevices);
        for (int i = 0; i < waveInDevices; i++)
        {
           answer.Add(WaveInEvent.GetCapabilities(i));
        }
        return answer;
    }

    /// <summary>
    /// Start recording sound from chosen device into buffer
    /// </summary>
    /// <param name="deviceNumber">Number of device</param>
    public void Listen(int deviceNumber = -1)
    {
        _waveInEvent.DeviceNumber = deviceNumber;
        _waveInEvent.DataAvailable += WaveIn_DataAvailable;
        _waveInEvent.StartRecording();
    }

    /// <summary>
    /// Stop recording sound
    /// </summary>
    public void Stop()
    {
        _waveInEvent.StopRecording();
        _recordedSound.Position = 0;
    }
    
    protected virtual void WaveIn_DataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
    {
        _recordedSound.Write(e.Buffer);
    }

    //TODO: Destroy
    public void PlaySound()
    {
        var rs = new RawSourceWaveStream(this._recordedSound, this._waveFormat);
        var wa = new WaveOutEvent();
        wa.Init(rs);
        wa.Play();
    }
}
namespace Chromaprint.Audio;

/// <summary>
/// Consumer for 16-bit audio data
/// </summary>
public interface IAudioConsumer
{
    /// <summary>
    /// Consume audio data
    /// </summary>
    /// <param name="input">Input buffer</param>
    /// <param name="length">Number of samples to consume</param>
    void Consume(short[] input, int length);
}
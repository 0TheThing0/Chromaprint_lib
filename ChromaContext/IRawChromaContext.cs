using Chromaprint.Audio;
using Chromaprint.Fingerprint;

namespace Chromaprint;

/// <summary>
/// Basic Chromaprint API interface for data streams
/// </summary>
public interface IRawChromaContext : IAudioConsumer
{
    /// <summary>
    /// Return the version number of Chromaprint.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the fingerprint algorithm the context is configured to use.
    /// </summary>
    public FingerprintAlgorithm Algorithm { get; }

    /// <summary>
    /// Restart the computation of a fingerprint with a new audio file.
    /// </summary>
    /// <param name="sampleRate">sample rate of the audio stream (in Hz)</param>
    /// <param name="numChannels">numbers of channels in the audio stream (1 or 2)</param>
    /// <returns>False on error, true on success</returns>
    bool Start(int sampleRate, int numChannels);

    /// <summary>
    /// Send audio data to the fingerprint calculator.
    /// </summary>
    /// <param name="data">Raw audio data (16-bit signed PCM)</param>
    /// <param name="size">Size of the data buffer (in samples)</param>
    public void Feed(short[] data, int size);

    /// <summary>
    /// Process any remaining buffered audio data and calculate the fingerprint.
    /// </summary>
    public void Finish();

    /// <summary>
    /// Return the calculated fingerprint as a compressed string.
    /// </summary>
    /// <returns>The fingerprint as a compressed string</returns>
    public string GetFingerprint();

    /// <summary>
    /// Return the calculated fingerprint as an array of 32-bit integers.
    /// </summary>
    /// <returns>The raw fingerprint (array of 32-bit integers)</returns>
    public int[] GetRawFingerprint();

    /// <summary>
    /// Return 32-bit hash of the calculated fingerprint.
    /// </summary>
    /// <returns>The hash.</returns>
    public int GetFingerprintHash();

    /// <summary>
    ///     Returns the amount of fingerprint data (int32 values)
    ///     based on the current image state
    /// </summary>
    /// <returns></returns>
    public int GetReadyFPSize();
    
    /// <summary>
    ///     Compress a raw fingerprint and optionally apply base64 encoding
    /// </summary>
    /// <param name="fingerprint"></param>
    /// <param name="algorithm"></param>
    /// <param name="base64"></param>
    /// <returns></returns>
    public static byte[] EncodeFingerprint(int[] fingerprint, int algorithm, bool base64)
    {
        return IFileChromaContext.EncodeFingerprint(fingerprint, algorithm, base64);
    }

    /// <summary>
    ///     Decompress and optionally decode base64-encoded fingerprint
    /// </summary>
    /// <param name="encodedFingerprint"></param>
    /// <param name="base64"></param>
    /// <param name="algorithm"></param>
    /// <returns></returns>
    public static int[] DecodeFingerprint(byte[] encodedFingerprint, bool base64, out int algorithm)
    {
        return IFileChromaContext.DecodeFingerprint(encodedFingerprint, base64, out algorithm);
    }
}
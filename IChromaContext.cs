using Chromaprint.Audio;

namespace Chromaprint;

/// <summary>
///     Basic Chromaprint API interface
/// </summary>
public interface IChromaContext : IAudioConsumer
{
    // TODO: decide if the version is crucial in our project
    /// <summary>
    /// Return the version number of Chromaprint.
    /// </summary>
    //public string Version { get; }

    // TODO: decide if the hashing must be included in our project
    // (probably might be useful for a server: if 2 hashes are the same, 
    // why not output the track)
    /// <summary>
    /// Return 32-bit hash of the calculated fingerprint.
    /// </summary>
    /// <returns>The hash.</returns>
    //public int GetFingerprintHash();

    /// <summary>
    ///     Gets the fingerprint algorithm the context is configured to use.
    /// </summary>
    public int Algorithm { get; }

    // TODO: discuss Start method and its implementation. Do we have a dest. sample rate and channels?
    // or do we precompute them?
    /// <summary>
    /// Restart the computation of a fingerprint with a new audio stream.
    /// </summary>
    /// <param name="sampleRate">Sample rate of the audio stream (in Hz)</param>
    /// <param name="numChannels">Numbers of channels in the audio stream (1 or 2)</param>
    /// <returns>False on error, true on success</returns>
    //public bool Start(int sampleRate, int numChannels);

    /// <summary>
    ///     Send audio data to the fingerprint calculator.
    /// </summary>
    /// <param name="data">Raw audio data (16-bit signed PCM)</param>
    /// <param name="size">Size of the data buffer (in samples)</param>
    public void Feed(short[] data, int size);

    /// <summary>
    ///     Process any remaining buffered audio data and calculate the fingerprint.
    /// </summary>
    public void Finish();

    /// <summary>
    ///     Return the calculated fingerprint as a compressed string.
    /// </summary>
    /// <returns>The fingerprint as a compressed string</returns>
    public string GetFingerprint();

    /// <summary>
    ///     Return the calculated fingerprint as an array of 32-bit integers.
    /// </summary>
    /// <returns>The raw fingerprint (array of 32-bit integers)</returns>
    public int[] GetRawFingerprint();
}
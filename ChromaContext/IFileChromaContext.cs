using Chromaprint.Fingerprint;

namespace Chromaprint;

/// <summary>
/// Basic Chromaprint API interface for audio files
/// </summary>
public interface IFileChromaContext
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
    /// Compute the fingerprint from an audio file.
    /// </summary>
    /// <returns>False on error, true on success</returns>
    public bool ComputeFingerprint(string filePath);
    
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
    ///     Return 32-bit hash of the calculated fingerprint.
    /// </summary>
    /// <returns>The hash.</returns>
    public int GetFingerprintHash();
}
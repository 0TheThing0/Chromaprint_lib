using Chromaprint.Compression;
using Chromaprint.Fingerprint;
using Chromaprint.Utilities;

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
    /// Return 32-bit hash of the calculated fingerprint.
    /// </summary>
    /// <returns>The hash.</returns>
    public int GetFingerprintHash();

    /// <summary>
    /// Compress a raw fingerprint and optionally apply base64 encoding
    /// </summary>
    /// <param name="fingerprint"></param>
    /// <param name="algorithm"></param>
    /// <param name="base64"></param>
    /// <returns></returns>
    public static byte[] EncodeFingerprint(int[] fingerprint, int algorithm, bool base64)
    {
        var compressor = new FingerprintCompressor();
        var compressed = compressor.Compress(fingerprint, algorithm);
    
        if (base64) compressed = ChromaBase64.Encode(compressed);
    
        return ChromaBase64.ByteEncoding.GetBytes(compressed);
    }

    /// <summary>
    /// Decompress and optionally decode base64-encoded fingerprint
    /// </summary>
    /// <param name="encodedFingerprint"></param>
    /// <param name="base64"></param>
    /// <param name="algorithm"></param>
    /// <returns></returns>
    public static int[] DecodeFingerprint(byte[] encodedFingerprint, bool base64, out int algorithm)
    {
        var encoded = ChromaBase64.ByteEncoding.GetString(encodedFingerprint);
        var compressed = base64 ? ChromaBase64.Decode(encoded) : encoded;

        var decompressor = new FingerprintDecompressor();
        var decompressedData = decompressor.Decompress(compressed, out algorithm);

        var size = decompressedData.Length;
        var fingerprint = new int[size];
        Array.Copy(decompressedData, 0, fingerprint, 0, size);
        return fingerprint;
    }
}
namespace Chromaprint.Compression;

/// <summary>
/// Class that contains data about normal
/// and exception bits amount for
/// <see cref="FingerprintCompressor"/> and
/// <see cref="FingerprintDecompressor"/>
/// </summary>
/// <remarks>
/// This data must be the same both in compressor
/// and decompressor, so in order to:
///     1. not store this only in one of the classes
///     and reference these fields from the other class;
///     2. easily synchronize field values in both classes;
/// a decision has been made to make it a separate class 
/// </remarks>
public static class CompressionBitData
{
    public static readonly int MaxNormalValue = 7;     // basically (1<<NormalBits)-1
    public static readonly int NormalBits = 3;
    public static readonly int ExceptionBits = 5;
}
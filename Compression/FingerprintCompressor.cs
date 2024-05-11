using Chromaprint.Utilities;

namespace Chromaprint.Compression;

using static CompressionBitData;

/// <summary>
/// Class for compressing the acquired fingerprint
/// (compression algorithm is described in remarks).
/// Decompress using <see cref="FingerprintDecompressor"/>
/// </summary>
/// <remarks>https://oxygene.sk/2010/11/binary-fingerprint-compression/</remarks>
public class FingerprintCompressor
{
    private List<byte> _result;
    private List<byte> _bits = new List<byte>();

    public string Compress(int[] data, int algorithm = 0)
    {
        // brief compression algorithm idea:
        // usually two sequential fingerprint samples
        // (32-bit values) are not that different, so
        // xor will usually result in small values.
        // The result of xor can be delta-encoded. When 
        // the encoding value is so small that it fits
        // into 3 bits, it is put in the main block
        // (so-called Normal bits). At the same time, when
        // the value is large (which is not that frequent),
        // the value is put separately (so-called Exception
        // bits), after main data sequence, as a 5-bit value,
        // but with 7 (max 3-bit value) subtracted from the
        // value so that it'll fit into 5 bits.
        if (data.Length > 0)
        {
            ProcessSubfingerprint((uint)data[0]);
            for (int i = 1; i < data.Length; i++)
            {
                ProcessSubfingerprint((uint)data[i]^(uint)data[i-1]);
            }
        }

        int length = data.Length;
        _result = new List<byte>();
        _result.Add((byte)((int)algorithm & 255));
        _result.Add((byte)((length >> 16) & 255));
        _result.Add((byte)((length >> 8) & 255));
        _result.Add((byte)(length & 255));
        WriteNormalBits();
        WriteExceptionBits();

        return ChromaBase64.ByteEncoding.GetString(_result.ToArray());
    }

    /// <summary>
    /// Method used to retrieve data for the first step of
    /// the compression algorithm - xor sequence of the numbers.
    /// See <see cref="Compress"/> for brief explanation of
    /// the algorithm
    /// </summary>
    /// <param name="x"></param>
    private void ProcessSubfingerprint(uint x)
    {
        int bit = 1;
        int lastBit = 0;
        while (x != 0)
        {
            if ((x & 1) != 0)
            {
                _bits.Add((byte)(bit - lastBit));
                lastBit = bit;
            }
            x >>= 1;
            bit++;
        }
        _bits.Add(0);
    }

    /// <summary>
    /// Writes main compressed data (the values of which
    /// are small). See <see cref="Compress"/> for brief
    /// algorithm explanation
    /// </summary>
    private void WriteNormalBits()
    {
        BitStringWriter writer = new BitStringWriter();
        for (int i = 0; i < _bits.Count; i++)
        {
            writer.Write((uint)Math.Min((int)_bits[i],MaxNormalValue),NormalBits);
        }

        writer.Flush();
        _result.AddRange(writer.Bytes);
    }

    /// <summary>
    /// Writes auxiliary (exception) data, values of which
    /// are large and cannot fit into NormalBits.
    /// See <see cref="Compress"/> for brief
    /// algorithm explanation
    /// </summary>
    private void WriteExceptionBits()
    {
        BitStringWriter writer = new BitStringWriter();
        for (int i = 0; i < _bits.Count; i++)
        {
            if (_bits[i] >= MaxNormalValue)
            {
                writer.Write((uint)((int)_bits[i]-MaxNormalValue),ExceptionBits);
            }
        }

        writer.Flush();
        _result.AddRange(writer.Bytes);
    }


}
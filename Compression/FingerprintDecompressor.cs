using Chromaprint.Utilities;

namespace Chromaprint.Compression;

using static CompressionBitData;

/// <summary>
/// Class for decompressing the fingerprint
/// compressed using <see cref="FingerprintCompressor"/>
/// (compression algorithm is described in remarks). 
/// </summary>
/// <remarks>https://oxygene.sk/2010/11/binary-fingerprint-compression/</remarks>
public class FingerprintDecompressor
{
    private int[] _result;
    private List<byte> _bits = new List<byte>();

    public int[] Decompress(string data, out int algorithm)
    {
        // In order to understand the algorithm, it is 
        // strongly recommended to read the article 
        // provided in remarks to the class, or a brief
        // explanation in FingerprintCompressor.Compress
        algorithm = -1;

        if (data.Length < 4)
        {
            // invalid data provided: see FingerprintCompressor.Compress
            // to make sure there must be 4 bytes minimum
            return Array.Empty<int>();
        }

        algorithm = (int)data[0];
        int length = ((byte)(data[1]) << 16) | ((byte)(data[2]) << 8) | ((byte)(data[3]));

        BitStringReader reader = new BitStringReader(data);
        reader.Read(32);

        if (reader.AvailableBits() < length * NormalBits)
        {
            // no actual compressed fingerprint provided 
            return Array.Empty<int>();
        }

        _result = new int[length];
        for (int i = 0; i < length; i++)
        {
            _result[i] = -1;
        }

        // starting the decompression itself
        reader.Reset();
        if (!ReadNormalBits(reader))
        {
            return Array.Empty<int>();
        }

        reader.Reset();
        if (!ReadExceptionBits(reader))
        {
            return Array.Empty<int>();
        }

        UnpackBits();

        return _result;
    }

    /// <summary>
    /// Computes the resulting values of the fingerprint
    /// using bits read from the compressed data. Basically
    /// constructs the _result array using _bits array.
    /// See <see cref="FingerprintCompressor.Compress"/>
    /// for brief algorithm explanation
    /// </summary>
    private void UnpackBits()
    {
        int i = 0;
        int lastBit = 0;
        int value = 0;
        
        for (int j = 0; j < _bits.Count; j++)
        {
            int bit = _bits[j];
            if (bit == 0)
            {
                _result[i] = (i > 0) ? value ^ _result[i - 1] : value;
                value = 0;
                lastBit = 0;
                i++;
                continue;
            }

            bit += lastBit;
            lastBit = bit;
            value |= 1 << (bit - 1);
        }
    }

    /// <summary>
    /// Reads main compressed data (values of which
    /// are small). See <see cref="FingerprintCompressor.Compress"/>
    /// for brief algorithm explanation
    /// </summary>
    /// <param name="reader">Data bit stream</param>
    /// <returns>Result status of operation (operation succeeded or not)</returns>
    private bool ReadNormalBits(BitStringReader reader)
    {
        int i = 0;
        int resultLength = _result.Length;
        while (i < resultLength)
        {
            int bit = (int)reader.Read(NormalBits);
            if (bit == 0)
            {
                i++;
            }

            _bits.Add((byte)bit);
        }

        return true;
    }

    /// <summary>
    /// Reads auxiliary (exception) data, values of which
    /// are large and cannot fit into
    /// <see cref="CompressionBitData.NormalBits"/>.
    /// See <see cref="FingerprintCompressor.Compress"/>
    /// for brief algorithm explanation
    /// </summary>
    /// <param name="reader">Data bit stream</param>
    /// <returns>Result status of operation (operation succeeded or not)</returns>
    private bool ReadExceptionBits(BitStringReader reader)
    {
        for (int i = 0; i < _bits.Count; i++)
        {
            if (_bits[i] == MaxNormalValue)
            {
                if (reader.Eof)
                {
                    // invalid fingerprint: reached EOF while reading exception bits
                    return false;
                }
                
                _bits[i] += (byte)reader.Read(ExceptionBits);
            }
        }
        
        return true;
    }


}
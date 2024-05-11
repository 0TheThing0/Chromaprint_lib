namespace Chromaprint.Utilities;

/// <summary>
/// Class that implements easy sequential
/// retrieval of bits from an array of bytes.
/// Mostly used in <see cref="Compression.FingerprintDecompressor"/>
/// </summary>
public class BitStringReader
{
    // remark about the purpose of _buffer:
    // the data can be read by bytes, as the value
    // is stored as an array of bytes, so _valueIter
    // corresponds to the index of the next byte
    // to be read. _buffer is introduced so that in
    // cases where the amount of bits is not divisible
    // by 8, there are more bits read than needed,
    // and only the needed amount is returned
    private byte[] _value;
    private ulong _buffer;
    private int _valueIter;
    private int _bufferSize;
    private bool _eof;

    public bool Eof
    {
        get => _eof;
    }

    public BitStringReader(string input)
        : this(ChromaBase64.ByteEncoding.GetBytes(input))
    {
    }

    public BitStringReader(byte[] input)
    {
        _value = input;
        _buffer = 0;
        _bufferSize = 0;
        _eof = false;
    }

    /// <summary>
    /// Read next bits from the buffer
    /// </summary>
    /// <param name="bits">
    /// Amount of bits to read. If the value of
    /// this parameter is be greater than 32 –
    /// size of uint – 32 bits are retrieved.
    /// If the value is negative, no bits are
    /// retrieved</param>
    /// <returns>
    /// 32-bit value containing requested bits
    /// </returns>
    public uint Read(int bits)
    {
        if (bits > 32)
        {
            bits = 32;
        }

        if (bits < 0)
        {
            bits = 0;
        }

        while (_bufferSize < bits)
        {
            if (_valueIter < _value.Length)
            {
                _buffer |= (uint)(_value[_valueIter++] << _bufferSize);
                _bufferSize += 8;
            }
            else
            {
                _eof = true;
            }
        }

        // if (_bufferSize < bits)
        // {
        //     if (_valueIter < _value.Length)
        //     {
        //         _buffer |= (uint)(_value[_valueIter++] << _bufferSize);
        //         _bufferSize += 8;
        //     }
        //     else
        //     {
        //         _eof = true;
        //     }
        // }

        uint result = (uint)(_buffer & ((ulong)(1 << bits) - 1));
        _buffer >>= bits;
        _bufferSize -= bits;

        if (_bufferSize <= 0 && _valueIter == _value.Length)
        {
            _eof = true;
        }

        return result;
    }

    public void Reset()
    {
        _buffer = 0;
        _bufferSize = 0;
    }

    public int AvailableBits()
    {
        return _eof ? 0 : _bufferSize + 8 * (_value.Length - _valueIter);
    }
}
namespace Chromaprint.Utilities;

/// <summary>
/// Class that implements easy bit storage routines.
/// Mostly used in <see cref="Compression.FingerprintCompressor"/>
/// </summary>
public class BitStringWriter
{
    // original implementation contained a
    // uint buffer, but there would've been 
    // a possible loss of data in Write method
    private ulong _buffer;
    private List<byte> _value;
    private int _bufferSize;

    public byte[] Bytes
    {
        get => _value.ToArray();
    }

    public string Value
    {
        get => ChromaBase64.ByteEncoding.GetString(_value.ToArray());
    }

    public BitStringWriter()
    {
        _value = new List<byte>();
        _buffer = 0;
        _bufferSize = 0;
    }

    /// <summary>
    /// Writes a certain amount of bits of a
    /// 32-bit value to the stream
    /// </summary>
    /// <param name="x">32-bit value containing bits</param>
    /// <param name="bits">
    /// Number of bits to write from x. Must be less than 32 - size of uint
    /// </param>
    public void Write(uint x, int bits)
    {
        if (bits > 32) bits = 32;
        if (bits < 0) bits = 0;
        // in the case where the buffer contains
        // the maximum possible value of 7 bits,
        // it must accept a maximum 32-bit value
        // accurately. Size of buffer must include
        // 39 bits minimum
        x = (uint)(x & ((ulong)(1 << bits) - 1));
        _buffer |= (x << _bufferSize);
        _bufferSize += bits;
        
        // if the amount of bits written fits
        // into the size of byte, write them
        // into values list
        while (_bufferSize >= 8)
        {
            _value.Add((byte)(_buffer&255));
            _buffer >>= 8;
            _bufferSize -= 8;
        }
    }

    public void Flush()
    {
        while (_bufferSize > 0)
        {
            _value.Add((byte)(_buffer & 255));
            _buffer >>= 8;
            _bufferSize -= 8;
        }

        _bufferSize = 0;
    }


}
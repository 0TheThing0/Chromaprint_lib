using System;

namespace Chromaprint.FFT;

/// <summary>
/// Buffer for combining two arrays
/// </summary>
public class CombinedBuffer
{
    short[][] _buffer = new short[3][];
    int[] _size = new int[3];
    int _offset;

    public int Offset { get => _offset; }
    
    /// <summary>
    /// Gets the size of the combined buffer.
    /// </summary>
    public int Size
    {
        get { return _size[0] + _size[1] - _offset; }
    }
    /// <summary>
    /// Create combined buffer
    /// </summary>
    /// <param name="buffer1">First buffer</param>
    /// <param name="size1">First buffer size</param>
    /// <param name="buffer2">Second buffer</param>
    /// <param name="size2">Second buffer size</param>
    public CombinedBuffer(short[] buffer1, int size1, short[] buffer2, int size2)
    {
        _offset = 0;
        _buffer[0] = buffer1;
        _buffer[1] = buffer2;
        _buffer[2] = null;
        _size[0] = size1;
        _size[1] = size2;
        _size[2] = -1;
    }
    
    /// <summary>
    /// Gets the element at given position.
    /// </summary>
    public short this[int i]
    {
        get
        {
            int k = i + _offset;
            if (k < _size[0])
            {
                return _buffer[0][k];
            }
            k -= _size[0];
            return _buffer[1][k];
        }
    }
    
    /// <summary>
    /// Shift the buffer offset.
    /// </summary>
    /// <param name="shift">Places to shift.</param>
    /// <returns>The new buffer offset.</returns>
    public int Shift(int shift)
    {
        _offset += shift;
        return _offset;
    }
    
    /// <summary>
    /// Read a number of values from the combined buffer.
    /// </summary>
    /// <param name="buffer">Buffer to write into.</param>
    /// <param name="offset">Offset to start reading.</param>
    /// <param name="length">Number of values to read.</param>
    /// <returns>Total number of values read.</returns>
    public int Read(short[] buffer, int offset, int length)
    {
        int n = length, pos = offset + _offset;

        if (pos < _size[0] && pos + length > _size[0])
        {
            // Number of shorts to be read from first buffer
            int split = _size[0] - pos;

            // Number of shorts to be read from second buffer
            n = Math.Min(length - split, _size[1]);

            // Copy from both buffers
            Array.Copy(_buffer[0], pos, buffer, 0, split);
            Array.Copy(_buffer[1], 0, buffer, split, n);

            // Correct total length
            n += split;
        }
        else
        {
            if (pos >= _size[0])
            {
                pos -= _size[0];
                // Number of shorts to be read from second buffer
                n = Math.Min(length, _size[1] - pos);

                // Read from second buffer
                Array.Copy(_buffer[1], pos, buffer, 0, n);
            }
            else
            {
                // Read from first buffer
                Array.Copy(_buffer[0], pos, buffer, 0, n);
            }
        }

        return n;
    }
    
    /// <summary>
    /// Read all remaining values from the buffer.
    /// </summary>
    /// <param name="buffer">Buffer to write into.</param>
    public void Flush(short[] buffer)
    {
        // Read the whole buffer (offset will be taken care of).
        if (this.Size > 0)
        {
            this.Read(buffer, 0, this.Size);
        }
    }
}
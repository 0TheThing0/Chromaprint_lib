namespace Chromaprint.SilenceRemover;

/// <summary>
/// Class that implements a circular array, allowing
/// to calculate the average of a constant size of
/// elements. Used in <see cref="SilenceRemover"/> 
/// </summary>
public class MovingAverage
{
    private short[] _buffer;
    private int _size;
    private int _offset;
    private int _sum;
    private int _count;

    public MovingAverage(int size)
    {
        _offset = 0;
        _sum = 0;
        _count = 0;
        _size = size;
        _buffer = new short[size];
    }

    public void AddValue(short x)
    {
        // adjusting the sum: "replacing" the previously existing 
        // element with an incoming one
        _sum += x;
        _sum -= _buffer[_offset];
        
        // if the buffer is still not full, the average must be
        // taken dividing the sum by the amount of existing values 
        if (_count < _size)
        {
            _count++;
        }

        _buffer[_offset] = x;
        _offset = (_offset + 1) % _size;
    }

    public int GetAverage()
    {
        if (_count == 0)
        {
            return 0;
        }

        return _sum / _count;
    }

}
using Chromaprint.Audio;

namespace Chromaprint.SilenceRemover;

/// <summary>
/// Class that allows to prevent silent (meaning,
/// values that are less or equal than a certain
/// threshold, in average) from being passed to
/// its consumer 
/// </summary>
public class SilenceRemover : IAudioConsumer
{
    // basically 5ms, if the sample rate is 11025
    private const int kSilenceWindow = 55;

    private bool _start;
    private int _threshold;
    private MovingAverage _average;
    private IAudioConsumer _consumer;

    public int Threshold
    {
        get => _threshold;
        set => _threshold = value;
    }

    public IAudioConsumer Consumer
    {
        get => _consumer;
        set => _consumer = value;
    }

    public SilenceRemover(IAudioConsumer consumer, int threshold = 0, int windowSize = kSilenceWindow)
    {
        _start = true;
        _threshold = threshold;
        _average = new MovingAverage(windowSize);
        _consumer = consumer;
    }

    public void Consume(short[] input, int length)
    {
        int offset = 0;
        int n = length;

        if (_start)
        {
            while (length > 0)
            {
                _average.AddValue(Math.Abs(input[offset]));
                if (_average.GetAverage() > _threshold)
                {
                    _start = false;
                    break;
                }

                offset++;
                length--;
            }
        }

        // if there were detected silence-contributing 
        // values, do not capture them – shift the array
        if (offset > 0)
        {
            // one of the methods to shift the array would've 
            // been to use Array.Copy method on the same array,
            // but if dest and src overlap, the method acts as
            // if the original values of the src array were 
            // preserved in a temporary location. (see https://learn.microsoft.com/en-us/dotnet/api/system.array.copy?view=net-8.0)
            // and probably this approach is faster than calling
            // a separate method to shift an array
            for (int i = 0; i < n - offset; i++)
            {
                input[i] = input[i + offset];
            }
            
            // other values must not be used by the consumer, but
            // if there is some necessity to do so, this must be
            // overlooked:
            for (int i = n - offset; i < n; i++)
            {
                input[i] = 0;
            }
        }

        if (length > 0)
        {
            _consumer.Consume(input, length);
        }
    }

    // Current implementation is different from the original
    // implementation of Reset method. Original implementation
    // included sample_rate and num_channels parameters,
    // but sample_rate was not used, and num_channels only affected
    // the output value
    //
    // upd: num_channels actually affected the result of the method,
    // so the original implementation will be present here, even 
    // though sample_rate did not have any effect
    public bool Reset(int sample_rate, int num_channels)
    {
        if (num_channels != 1)
        {
            return false;
        }

        _start = true;
        return true;
    }

    public void Flush()
    {
    }
}
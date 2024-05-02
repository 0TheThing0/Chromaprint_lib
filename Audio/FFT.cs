using System;

namespace Shazam;

public class FFT
{
    private double[] _window;
    private int _frame_size;
    private short[] _buffer;
    private int _buffer_offset;
    private double[] _frame;
    private int _increment;
    private IFFTService _service;
    private IFFTFrameConsumer _consumer;
    
    // FFT input buffer
    short[] _input;
    
    public FFT(int frame_size, int overlap, IFFTService service, IFFTFrameConsumer consumer)
    {
        _service = service;
        _window = new double[frame_size];
        _buffer = new short[frame_size];
        _buffer_offset = 0;
        _frame = new double[frame_size];
        _frame_size = frame_size;
        _increment = frame_size - overlap;
        _consumer = consumer;
        
        Helper.PrepareHammingWindow(ref _window, 0, frame_size);
        for (int i = 0; i < frame_size; i++)
        {
            _window[i] /= short.MaxValue;
        }
        
        _service.Initialize(frame_size, _window);
        
        _input = new short[frame_size];
    }
    
    public void Reset()
    {
        _buffer_offset = 0;
    }

    public void Consume(short[] input, int length)
    {
        if (_buffer_offset + length < _frame_size)
        {
            Array.Copy(input,0,_buffer,_buffer_offset,length);
            _buffer_offset += length;
            return;
        }
        
        // Apply FFT on the available data
        CombinedBuffer combined_buffer = new CombinedBuffer(_buffer, _buffer_offset, input, length);

        while (combined_buffer.Size >= _frame_size)
        {
            combined_buffer.Read(_input, 0, _frame_size);
            _service.ComputeFrame(_input,_frame);
            
            _consumer.Consume(_frame);
            combined_buffer.Shift(_increment);
        }
        
        combined_buffer.Flush(_buffer);
        _buffer_offset = combined_buffer.Size;
    }
}


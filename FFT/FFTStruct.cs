using System;
using Chromaprint;
namespace Chromaprint.FFT;

public class FFTStruct
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
    
    /// <summary>
    /// Create basic fft transformer
    /// </summary>
    /// <param name="frameSize">Size of frame to process</param>
    /// <param name="overlap">Frame overlapping</param>
    /// <param name="service">Service to process input</param>
    /// <param name="consumer">Result consumer</param>
    public FFTStruct(int frameSize, int overlap, IFFTService service, IFFTFrameConsumer consumer)
    {
        _service = service;
        _window = new double[frameSize];
        _buffer = new short[frameSize];
        _buffer_offset = 0;
        _frame = new double[frameSize];
        _frame_size = frameSize;
        _increment = frameSize - overlap;
        _consumer = consumer;
        
        Helper.PrepareHammingWindow(ref _window, 0, frameSize);
        for (int i = 0; i < frameSize; i++)
        {
            _window[i] /= short.MaxValue;
        }
        
        _service.Initialize(frameSize, _window);
        
        _input = new short[frameSize];
    }
    
    public void Reset()
    {
        _buffer_offset = 0;
    }

    /// <summary>
    /// Process input data
    /// </summary>
    /// <param name="input">Int16 values</param>
    /// <param name="length">Lenght of input</param>
    public void Consume(short[] input, int length)
    {
        //Check if buffer is full
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
            
            //Sending processed data to consumer
            _consumer.Consume(_frame);
            combined_buffer.Shift(_increment);
        }
        
        combined_buffer.Flush(_buffer);
        _buffer_offset = combined_buffer.Size;
    }
}


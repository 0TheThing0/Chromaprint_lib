﻿using System;
using NAudio.Wave;
using FFT;

namespace Audio;

/// <summary>
/// Class for capturing sound from device and applying FFT
/// </summary>
public class FFTAudioCapture : AudioCapture
{
    static public readonly int FRAME_SIZE = 4096;
    static public readonly int OVERLAP = FRAME_SIZE - FRAME_SIZE / 3;
    
    private FFTStruct _fftStruct;
    public FFTAudioCapture(int rate, int bits, int channels,IFFTService service, IFFTFrameConsumer consumer)
    : base(rate,bits,channels)
    {
        _fftStruct = new FFTStruct(FRAME_SIZE, OVERLAP, service, consumer);
    }

    protected override void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        short[] sdata = new short[e.Buffer.Length / 2 + e.Buffer.Length % 2];
        Buffer.BlockCopy(e.Buffer, 0, sdata, 0, e.Buffer.Length);
        _fftStruct.Consume(sdata,sdata.Length);
    }
}
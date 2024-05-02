namespace Shazam;

/// <summary>
/// Consumer of frames produced by FFT.
/// </summary>
public interface IFFTFrameConsumer
{

    void Consume(double[] frame);

}
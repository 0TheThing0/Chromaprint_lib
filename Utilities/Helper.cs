namespace Chromaprint.Utilities;

public interface Helper
{
    /// <summary>
    /// Creating window to processing input signal
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="first"></param>
    /// <param name="last"></param>
    public static void PrepareHammingWindow(ref double[] vector, int first, int last)
    {
        int i = 0, max_i = last - first - 1;
        double scale = 2.0 * Math.PI / max_i;
        while (first != last)
        {
            vector[first] = 0.54 - 0.46 * Math.Cos(scale * i++);
            first++;
        }
    }

    /// <summary>
    /// Applying window to data
    /// </summary>
    /// <param name="in_output"></param>
    /// <param name="window"></param>
    /// <param name="size"></param>
    /// <param name="scale"></param>
    public static void ApplyWindow(ref double[] in_output, double[] window, int size, double scale)
    {
        int i = 0;
        while (size-- > 0)
        {
            in_output[i] *= (window[i] * scale);
            ++i;
        }
    }

    /// <summary>
    /// Converting frequency into equal index in FFT frame
    /// </summary>
    /// <param name="freq"></param>
    /// <param name="frameSize"></param>
    /// <param name="sampleRate"></param>
    /// <returns></returns>
    public static int FreqToIndex(double freq, int frameSize, int sampleRate)
    {
        return (int)Math.Round(frameSize * freq / sampleRate);
    }
    
    /// <summary>
    /// Convert FFT frame index into freq
    /// </summary>
    /// <param name="i"></param>
    /// <param name="frameSize"></param>
    /// <param name="sampleRate"></param>
    /// <returns></returns>
    public static double IndexToFreq(int i, int frameSize, int sampleRate)
    {
        return (double)i * sampleRate / frameSize;
    }
    
    /// <summary>
    /// Convert freq to octave
    /// </summary>
    /// <param name="freq"></param>
    /// <param name="baseNote"></param>
    /// <returns></returns>
    public static double FreqToOctave(double freq, double baseNote = 440.0 / 16.0 )
    {
        return Math.Log(freq / baseNote, 2.0);
    }


    /// <summary>
    /// Calculates the norm of a vector of an arbitrary length
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static double EuclideanNorm(double[] vector)
    {
        double squares = 0;
        double value;
        
        for (int i = 0; i < vector.Length; i++)
        {
            value = vector[i];
            squares += value * value;
        }

        return (squares > 0) ? Math.Sqrt(squares) : 0;
    }

    /// <summary>
    /// Normalize the vector of an arbitrary length
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="norm"></param>
    /// <param name="threshold">If the value of norm is less than the threshold, the vector becomes a null-vector</param>
    public static void NormalizeVector(double[] vector, double norm, double threshold = 0.01)
    {
        if (norm < threshold)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = 0.0;
            }
        }
        else
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= norm;
            }
        }
    }
}
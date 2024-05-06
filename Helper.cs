namespace Chromaprint;

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
}
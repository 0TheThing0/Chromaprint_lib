using System;

namespace Shazam;

public interface Helper
{
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
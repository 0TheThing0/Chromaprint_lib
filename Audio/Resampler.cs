namespace Chromaprint.Audio;

/// <summary>
///     Audio resampling implementation
///     (replicates FFmpeg implementation)
/// </summary>
public class Resampler
{
    private struct ResampleContext
    {
        public short[] filterBank;
        public int filterLength;
        public int idealDstIncr;
        public int dstIncr;
        public int index;
        public int frac;
        public int srcIncr;
        public int compensationDistance;
        public int phaseShift;
        public int phaseMask;
        public bool linear;
    }

    // these fields mustn't be readonly according to
    // the original implementation, but I have a feeling
    // that they shouldn't change
    private static readonly int FILTER_SHIFT = 15;
    private static readonly int WINDOW_TYPE = 9;

    private ResampleContext _ctx;

    /// <summary>
    ///     Initialize the audio resampler.
    /// </summary>
    /// <param name="outRate">
    ///     Output sample rate
    /// </param>
    /// <param name="inRate">
    ///     Input sample rate
    /// </param>
    /// <param name="filterSize">
    ///     Length of each FIR filter in the filterbank
    ///     relative to the cutoff freq
    /// </param>
    /// <param name="phaseShift">
    ///     Log2 of the number of entries in the
    ///     polyphase filterbank
    /// </param>
    /// <param name="linear">
    ///     If true then the used FIR filter will be
    ///     linearly interpolated between the 2 closest,
    ///     if false the closest will be used
    /// </param>
    /// <param name="cutoff">
    ///     Cutoff frequency, 1.0 corresponds to half
    ///     the output sampling rate
    /// </param>
    public void Init(int outRate, int inRate, int filterSize, int phaseShift,
        bool linear, double cutoff)
    {
        _ctx = default;
        var factor = Min(outRate * cutoff / inRate, 1.0);
        var phaseCount = 1 << phaseShift;

        _ctx.phaseShift = phaseShift;
        _ctx.phaseMask = phaseCount - 1;
        _ctx.linear = linear;

        _ctx.filterLength = (int)Max((int)Math.Ceiling(filterSize / factor), 1);
        _ctx.filterBank = new short[_ctx.filterLength * (phaseCount + 1)];

        BuildFilter(_ctx.filterBank, factor, _ctx.filterLength, phaseCount, 1 << FILTER_SHIFT, WINDOW_TYPE);
        Array.Copy(_ctx.filterBank, 0, _ctx.filterBank, _ctx.filterLength * phaseCount + 1, _ctx.filterLength - 1);
        _ctx.filterBank[_ctx.filterLength * phaseCount] = _ctx.filterBank[_ctx.filterLength - 1];

        _ctx.srcIncr = outRate;
        _ctx.idealDstIncr = _ctx.dstIncr = inRate * phaseCount;
        _ctx.index = -phaseCount * ((_ctx.filterLength - 1) / 2);
    }

    /// <summary>
    ///     Resample an array of samples using a previously configured context.
    /// </summary>
    /// <param name="dst"></param>
    /// <param name="src">Array of unconsumed samples </param>
    /// <param name="consumed">Number of samples of src which have been consumed are returned here</param>
    /// <param name="srcSize">Number of unconsumed samples available </param>
    /// <param name="dstSize">Amount of space in samples available in dst</param>
    /// <param name="updateCtx">
    ///     If this is false then the context will not be modified, that way several
    ///     channels can be resampled with the same context.
    /// </param>
    /// <returns>Number of samples written in dst or -1 if an error occurred</returns>
    public int Resample(short[] dst, short[] src, ref int consumed, int srcSize, int dstSize, bool updateCtx)
    {
        int dst_index, i;
        var index = _ctx.index;
        var frac = _ctx.frac;
        var dstIncrFrac = _ctx.dstIncr % _ctx.srcIncr;
        var dstIncr = _ctx.dstIncr / _ctx.srcIncr;
        var compensationDistance = _ctx.compensationDistance;

        if (compensationDistance == 0 && _ctx.filterLength == 1 && _ctx.phaseShift == 0)
        {
            var index2 = (long)index << 32;
            var incr = (1L << 32) * _ctx.dstIncr / _ctx.srcIncr;
            dstSize = (int)Math.Min(dstSize, (srcSize - 1 - index) * (long)_ctx.srcIncr / _ctx.dstIncr);

            for (dst_index = 0; dst_index < dstSize; dst_index++)
            {
                dst[dst_index] = src[index2 >> 32];
                index2 += incr;
            }

            frac += dst_index * dstIncrFrac;
            index += dst_index * dstIncr;
            index += frac / _ctx.srcIncr;
            frac %= _ctx.srcIncr;
        }
        else
        {
            for (dst_index = 0; dst_index < dstSize; dst_index++)
            {
                var filter = _ctx.filterBank;
                var filter_offset = _ctx.filterLength * (index & _ctx.phaseMask);

                var sample_index = index >> _ctx.phaseShift;
                var val = 0;

                if (sample_index < 0)
                {
                    for (i = 0; i < _ctx.filterLength; i++)
                        val += src[Abs(sample_index + i) % srcSize] * filter[filter_offset + i];
                }
                else if (sample_index + _ctx.filterLength > srcSize)
                {
                    break;
                }
                else if (_ctx.linear)
                {
                    var v2 = 0;
                    for (i = 0; i < _ctx.filterLength; i++)
                    {
                        val += src[sample_index + i] * filter[filter_offset + i];
                        v2 += src[sample_index + i] * filter[filter_offset + i + _ctx.filterLength];
                    }

                    val += (int)((v2 - val) * (long)frac / _ctx.srcIncr);
                }
                else
                {
                    for (i = 0; i < _ctx.filterLength; i++) val += src[sample_index + i] * filter[filter_offset + i];
                }

                val = (val + (1 << (FILTER_SHIFT - 1))) >> FILTER_SHIFT;
                dst[dst_index] = (short)((uint)(val + 32768) > 65535 ? (val >> 31) ^ 32767 : val);

                frac += dstIncrFrac;
                index += dstIncr;
                if (frac >= _ctx.srcIncr)
                {
                    frac -= _ctx.srcIncr;
                    index++;
                }

                if (dst_index + 1 == compensationDistance)
                {
                    compensationDistance = 0;
                    dstIncrFrac = _ctx.idealDstIncr % _ctx.srcIncr;
                    dstIncr = _ctx.idealDstIncr / _ctx.srcIncr;
                }
            }
        }

        consumed = Math.Max(index, 0) >> _ctx.phaseShift;
        if (index >= 0) index &= _ctx.phaseMask;

        if (compensationDistance != 0) compensationDistance -= dst_index;
        // the value of compensation distance must be greater than zero (?) 
        if (updateCtx)
        {
            _ctx.frac = frac;
            _ctx.index = index;
            _ctx.dstIncr = dstIncrFrac + _ctx.srcIncr * dstIncr;
            _ctx.compensationDistance = compensationDistance;
        }

        return dst_index;
    }

    public void Close()
    {
        _ctx.filterBank = null;
    }

    private void Compensate(int sampleDelta, int compensationDistance)
    {
        _ctx.compensationDistance = compensationDistance;
        _ctx.dstIncr = _ctx.idealDstIncr - (int)(_ctx.idealDstIncr * (long)sampleDelta / compensationDistance);
    }

    private static int BuildFilter(short[] filter, double factor, int tapCount, int phaseCount, int scale, int type)
    {
        int ph, i;
        double x, y, w;
        var tab = new double[tapCount];
        var center = (tapCount - 1) / 2;

        // if upsampling, only need to interpolate, no filter
        if (factor > 1.0)
            factor = 1.0;

        for (ph = 0; ph < phaseCount; ph++)
        {
            double norm = 0;
            for (i = 0; i < tapCount; i++)
            {
                x = Math.PI * (i - center - (double)ph / phaseCount) * factor;
                if (x == 0) y = 1.0;
                else y = Math.Sin(x) / x;
                switch (type)
                {
                    case 0:
                        var d = -0.5f; //first order derivative = -0.5
                        x = Math.Abs((i - center - (double)ph / phaseCount) * factor);
                        if (x < 1.0) y = 1 - 3 * x * x + 2 * x * x * x + d * (-x * x + x * x * x);
                        else y = d * (-4 + 8 * x - 5 * x * x + x * x * x);
                        break;
                    case 1:
                        w = 2.0 * x / (factor * tapCount) + Math.PI;
                        y *= 0.3635819 - 0.4891775 * Math.Cos(w) + 0.1365995 * Math.Cos(2 * w) -
                             0.0106411 * Math.Cos(3 * w);
                        break;
                    default:
                        w = 2.0 * x / (factor * tapCount * Math.PI);
                        y *= Bessel(type * Math.Sqrt(Max(1 - w * w, 0)));
                        break;
                }

                tab[i] = y;
                norm += y;
            }

            // normalize so that an uniform color remains the same
            for (i = 0; i < tapCount; i++)
                filter[ph * tapCount + i] = Clip(Floor(tab[i] * scale / norm), short.MinValue, short.MaxValue);
        }

        return 0;
    }

    #region Math helper

    private static int Abs(int a)
    {
        return a >= 0 ? a : -a;
    }

    private static int Sign(int a)
    {
        return a > 0 ? 1 : -1;
    }

    private static double Max(double a, double b)
    {
        return a > b ? a : b;
    }

    private static double Min(double a, double b)
    {
        return a > b ? b : a;
    }

    private static int Floor(double x)
    {
        return (int)Math.Floor(x + 0.5);
    }

    private static short Clip(int a, short amin, short amax)
    {
        if (a < amin) return amin;
        if (a > amax) return amax;
        // casting to short must be okay, mustn't it?
        return (short)a;
    }

    /// <summary>
    ///     0th order modified bessel function of the first kind.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private static double Bessel(double x)
    {
        double v = 1;
        double lastv = 0;
        double t = 1;
        int i;

        x = x * x / 4;
        for (i = 1; v != lastv; i++)
        {
            lastv = v;
            t *= x / (i * i);
            v += t;
        }

        return v;
    }

    #endregion
}
namespace Chromaprint.Utilities;

/// <summary>
///     SimHash implementation that provides simple
///     methods for hashing
/// </summary>
public static class SimHash
{
    /// <summary>
    ///     Generate a 32-bit hash for a given array of data
    /// </summary>
    /// <param name="data">Data to be hashed</param>
    /// <returns>32-bit SimHash</returns>
    public static uint Compute(int[] data)
    {
        return Compute(data, 0, data.Length);
    }

    /// <summary>
    ///     Generate a 32-bit hash for a given array of data with given constraints
    /// </summary>
    /// <param name="data">Data to be hashed</param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns>32-bit SimHash</returns>
    public static uint Compute(int[] data, int start, int end)
    {
        var v = new int[32];

        // nulling bit counters 
        for (var i = 0; i < 32; i++) v[i] = 0;

        for (var i = start; i < end; i++)
        {
            var localHash = (uint)data[i];
            // for current element determine: if the bit is set,
            // inc the bit counter for this bit, otherwise – dec
            for (var j = 0; j < 32; j++) v[j] += (localHash & (1 << j)) == 0 ? -1 : 1;
        }

        uint hash = 0;
        // if the bit count is positive = resulting
        // bit is set, otherwise – unset
        for (var i = 0; i < 32; i++)
            if (v[i] > 0)
                hash |= (uint)(1 << i);

        return hash;
    }
}
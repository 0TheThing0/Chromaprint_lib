namespace Chromaprint;

public class ChromaContext : IChromaContext
{
    public void Consume(short[] input, int length)
    {
        throw new NotImplementedException();
    }

    public int Algorithm { get; }

    public void Feed(short[] data, int size)
    {
        throw new NotImplementedException();
    }

    public void Finish()
    {
        throw new NotImplementedException();
    }

    public string GetFingerprint()
    {
        throw new NotImplementedException();
    }

    public int[] GetRawFingerprint()
    {
        throw new NotImplementedException();
    }
}
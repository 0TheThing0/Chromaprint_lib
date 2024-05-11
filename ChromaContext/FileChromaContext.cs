using Chromaprint.Audio;
using Chromaprint.Compression;
using Chromaprint.FFT;
using Chromaprint.Fingerprint;
using Chromaprint.Utilities;

namespace Chromaprint;

public class FileChromaContext : IFileChromaContext
{
    private readonly FileFingerprinter _fingerprinter;
    private IFFTService _fftService;
    private int[] _fingerprint;
    private readonly AudioReader _audioReader;

    public string Version => "100.0.0";

    public FingerprintAlgorithm Algorithm { get; }

    public FileChromaContext()
        : this(FingerprintAlgorithm.FP2)
    {
    }

    public FileChromaContext(IFFTService fftService)
        : this(FingerprintAlgorithm.FP2, fftService)
    {
    }

    public FileChromaContext(FingerprintAlgorithm algorithm)
        : this(algorithm, new LomontFFTService())
    {
    }

    public FileChromaContext(FingerprintAlgorithm fingerprintAlgorithm, IFFTService fftService)
    {
        _fingerprint = Array.Empty<int>();
        Algorithm = fingerprintAlgorithm;
        _fftService = fftService;

        var config =
            FingerprinterConfiguration.CreateConfiguration(fingerprintAlgorithm);
        _fingerprinter = new FileFingerprinter(config, fftService);
        _audioReader = new AudioReader(
            FileFingerprinter.BUFFER_SIZE,
            FileFingerprinter.SAMPLE_RATE,
            FileFingerprinter.BIT_DEPTH,
            FileFingerprinter.NUM_CHANNELS,
            _fingerprinter);
    }

    /// <summary>
    ///     Set a configuration option for the selected fingerprint algorithm.
    /// </summary>
    /// <param name="name">option name</param>
    /// <param name="value">option value</param>
    /// <returns>False on error, true on success</returns>
    /// <remarks>
    ///     NOTE: DO NOT USE THIS FUNCTION IF YOU ARE PLANNING TO USE
    ///     THE GENERATED FINGERPRINTS WITH THE ACOUSTID SERVICE.
    ///     Possible options:
    ///     - silence_threshold: threshold for detecting silence, 0-32767
    /// </remarks>
    public bool SetOption(string name, int value)
    {
        return _fingerprinter.SetOption(name, value);
    }

    /// <summary>
    ///     Compute the fingerprint from an audio file.
    /// </summary>
    /// <returns>False on error, true on success</returns>
    public bool ComputeFingerprint(string filePath)
    {
        _fingerprinter.Start();

        // TODO: check SetFile status here
        _audioReader.SetFile(filePath);
        _audioReader.ReadAll();
        _fingerprint = _fingerprinter.Finish();

        return true;
    }

    /// <summary>
    ///     Return the calculated fingerprint as a compressed string.
    /// </summary>
    /// <returns>The fingerprint as a compressed string</returns>
    public string GetFingerprint()
    {
        var compressor = new FingerprintCompressor();
        return ChromaBase64.Encode(compressor.Compress(_fingerprint, (int)Algorithm));
    }

    /// <summary>
    ///     Return the calculated fingerprint as an array of 32-bit integers.
    /// </summary>
    /// <returns>The raw fingerprint (array of 32-bit integers)</returns>
    public int[] GetRawFingerprint()
    {
        var size = _fingerprint.Length;
        var fp = new int[size];

        Array.Copy(_fingerprint, 0, fp, 0, size);
        return fp;
    }

    public int GetFingerprintHash()
    {
        return SimHash.Compute(_fingerprint);
    }
}
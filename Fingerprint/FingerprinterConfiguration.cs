namespace Chromaprint.Fingerprint;
using Chromaprint.Classifier;

/// <summary>
/// Enum used to create one of the pre-defined
/// fingerprint configurations (values were
/// retrieved from ML-training algorithms)
/// </summary>
public enum FingerprintAlgorithm
{
    // Note: in the original implementation there is a
    // file called ChromaprintAlgorithm. A decision has
    // been made to make it a fingerprint algorithm and
    // include it into this file, as it corresponds to
    // FingerprinterConfiguration class only.
    
    FP1 = 0,
    FP2 = 1,
    FP3 = 2,
    FP4 = 3
}


/// <summary>
/// Class that contains important properties
/// needed in the fingerprinting process
/// (classifiers, coefficients, use silence
/// removal or not, etc.) 
/// </summary>
public class FingerprinterConfiguration
{
    protected static readonly double[] kChromaFilterCoefficients = { 0.25, 0.75, 1.0, 0.75, 0.25 };

    protected Classifier[] _classifiers;
    protected double[] _filterCoefficients;
    protected bool _interpolate;
    protected bool _removeSilence;
    protected int _silenceThreshold;

    public Classifier[] Classifiers
    {
        get => _classifiers;
    }

    public double[] FilterCoefficients
    {
        get => _filterCoefficients;
    }

    public bool Interpolate
    {
        get => _interpolate;
    }

    public bool RemoveSilence
    {
        get => _removeSilence;
    }

    public int SilenceThreshold
    {
        get => _silenceThreshold;
    }

    public static FingerprinterConfiguration CreateConfiguration(FingerprintAlgorithm algorithm)
    {
        FingerprinterConfiguration config = null;
        switch (algorithm)
        {
            case FingerprintAlgorithm.FP1:
                return new FingerprinterConfiguration1();
            case FingerprintAlgorithm.FP2:
                return new FingerprinterConfiguration2();
            case FingerprintAlgorithm.FP3:
                return new FingerprinterConfiguration3();
            case FingerprintAlgorithm.FP4:
                return new FingerprinterConfiguration4();
        }

        return config;
    }
}

/// <summary>
/// Pre-defined fingerprinter configuration (trained on random data) 
/// </summary>
public class FingerprinterConfiguration1 : FingerprinterConfiguration
{
    private static Classifier[] kClassifiers1 = {
        new Classifier(new Filter(0, 3, 15, 0), new Quantizer(2.10543, 2.45354, 2.69414)),
        new Classifier(new Filter(0, 4, 14, 1), new Quantizer(-0.345922, 0.0463746, 0.446251)),
        new Classifier(new Filter(4, 4, 11, 1), new Quantizer(-0.392132, 0.0291077, 0.443391)),
        new Classifier(new Filter(0, 4, 14, 3), new Quantizer(-0.192851, 0.00583535, 0.204053)),
        new Classifier(new Filter(8, 2, 4, 2), new Quantizer(-0.0771619, -0.00991999, 0.0575406)),
        new Classifier(new Filter(6, 2, 15, 5), new Quantizer(-0.710437, -0.518954, -0.330402)),
        new Classifier(new Filter(9, 2, 16, 1), new Quantizer(-0.353724, -0.0189719, 0.289768)),
        new Classifier(new Filter(4, 2, 10, 3), new Quantizer(-0.128418, -0.0285697, 0.0591791)),
        new Classifier(new Filter(9, 2, 16, 3), new Quantizer(-0.139052, -0.0228468, 0.0879723)),
        new Classifier(new Filter(1, 3, 6, 2), new Quantizer(-0.133562, 0.00669205, 0.155012)),
        new Classifier(new Filter(3, 6, 2, 3), new Quantizer(-0.0267, 0.00804829, 0.0459773)),
        new Classifier(new Filter(8, 1, 10, 2), new Quantizer(-0.0972417, 0.0152227, 0.129003)),
        new Classifier(new Filter(4, 4, 14, 3), new Quantizer(-0.141434, 0.00374515, 0.149935)),
        new Classifier(new Filter(4, 2, 15, 5), new Quantizer(-0.64035, -0.466999, -0.285493)),
        new Classifier(new Filter(9, 2, 3, 5), new Quantizer(-0.322792, -0.254258, -0.174278)),
        new Classifier(new Filter(1, 8, 4, 2), new Quantizer(-0.0741375, -0.00590933, 0.0600357))
    };

    public FingerprinterConfiguration1()
    {
        _classifiers = kClassifiers1;
        _filterCoefficients = kChromaFilterCoefficients;
        _interpolate = false;
    }
}

/// <summary>
/// Pre-defined fingerprinter configuration
/// (trained on eMusic samples)
/// </summary>
public class FingerprinterConfiguration2 : FingerprinterConfiguration
{
    private static Classifier[] kClassifiers2 = {
        new Classifier(new Filter(4, 3, 15, 0), new Quantizer(1.98215, 2.35817, 2.63523)),
        new Classifier(new Filter(4, 6, 15, 4), new Quantizer(-1.03809, -0.651211, -0.282167)),
        new Classifier(new Filter(0, 4, 16, 1), new Quantizer(-0.298702, 0.119262, 0.558497)),
        new Classifier(new Filter(8, 2, 12, 3), new Quantizer(-0.105439, 0.0153946, 0.135898)),
        new Classifier(new Filter(4, 4, 8, 3), new Quantizer(-0.142891, 0.0258736, 0.200632)),
        new Classifier(new Filter(0, 3, 5, 4), new Quantizer(-0.826319, -0.590612, -0.368214)),
        new Classifier(new Filter(2, 2, 9, 1), new Quantizer(-0.557409, -0.233035, 0.0534525)),
        new Classifier(new Filter(7, 3, 4, 2), new Quantizer(-0.0646826, 0.00620476, 0.0784847)),
        new Classifier(new Filter(6, 2, 16, 2), new Quantizer(-0.192387, -0.029699, 0.215855)),
        new Classifier(new Filter(1, 3, 2, 2), new Quantizer(-0.0397818, -0.00568076, 0.0292026)),
        new Classifier(new Filter(10, 1, 15, 5), new Quantizer(-0.53823, -0.369934, -0.190235)),
        new Classifier(new Filter(6, 2, 10, 3), new Quantizer(-0.124877, 0.0296483, 0.139239)),
        new Classifier(new Filter(1, 1, 14, 2), new Quantizer(-0.101475, 0.0225617, 0.231971)),
        new Classifier(new Filter(5, 6, 4, 3), new Quantizer(-0.0799915, -0.00729616, 0.063262)),
        new Classifier(new Filter(9, 2, 12, 1), new Quantizer(-0.272556, 0.019424, 0.302559)),
        new Classifier(new Filter(4, 2, 14, 3), new Quantizer(-0.164292, -0.0321188, 0.0846339)),
    };
    
    public FingerprinterConfiguration2()
    {
        _classifiers = kClassifiers2;
        _filterCoefficients = kChromaFilterCoefficients;
        _interpolate = false;
    }
}

/// <summary>
/// Pre-defined fingerprinter configuration
/// (trained on eMusic samples, with interpolation)
/// </summary>
public class FingerprinterConfiguration3 : FingerprinterConfiguration
{
    private static Classifier[] kClassifiers3 = {
        new Classifier(new Filter(4, 3, 15, 0), new Quantizer(1.98215, 2.35817, 2.63523)),
        new Classifier(new Filter(4, 6, 15, 4), new Quantizer(-1.03809, -0.651211, -0.282167)),
        new Classifier(new Filter(0, 4, 16, 1), new Quantizer(-0.298702, 0.119262, 0.558497)),
        new Classifier(new Filter(8, 2, 12, 3), new Quantizer(-0.105439, 0.0153946, 0.135898)),
        new Classifier(new Filter(4, 4, 8, 3), new Quantizer(-0.142891, 0.0258736, 0.200632)),
        new Classifier(new Filter(0, 3, 5, 4), new Quantizer(-0.826319, -0.590612, -0.368214)),
        new Classifier(new Filter(2, 2, 9, 1), new Quantizer(-0.557409, -0.233035, 0.0534525)),
        new Classifier(new Filter(7, 3, 4, 2), new Quantizer(-0.0646826, 0.00620476, 0.0784847)),
        new Classifier(new Filter(6, 2, 16, 2), new Quantizer(-0.192387, -0.029699, 0.215855)),
        new Classifier(new Filter(1, 3, 2, 2), new Quantizer(-0.0397818, -0.00568076, 0.0292026)),
        new Classifier(new Filter(10, 1, 15, 5), new Quantizer(-0.53823, -0.369934, -0.190235)),
        new Classifier(new Filter(6, 2, 10, 3), new Quantizer(-0.124877, 0.0296483, 0.139239)),
        new Classifier(new Filter(1, 1, 14, 2), new Quantizer(-0.101475, 0.0225617, 0.231971)),
        new Classifier(new Filter(5, 6, 4, 3), new Quantizer(-0.0799915, -0.00729616, 0.063262)),
        new Classifier(new Filter(9, 2, 12, 1), new Quantizer(-0.272556, 0.019424, 0.302559)),
        new Classifier(new Filter(4, 2, 14, 3), new Quantizer(-0.164292, -0.0321188, 0.0846339)),
    };

    public FingerprinterConfiguration3()
    {
        _classifiers = kClassifiers3;
        _filterCoefficients = kChromaFilterCoefficients;
        _interpolate = true;
    }
}

/// <summary>
/// Pre-defined fingerprinter configuration
/// (trained on eMusic samples, with silence removal)
/// </summary>
public class FingerprinterConfiguration4 : FingerprinterConfiguration2
{
    public FingerprinterConfiguration4()
    {
        _removeSilence = true;
        _silenceThreshold = 50;
    }
}
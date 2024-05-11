using System.Text;

namespace Chromaprint.Utilities;

/// <summary>
/// Custom implementation of Base64 encoding
/// </summary>
/// <remarks>
/// Convert.ToBase64String and Convert.FromBase64String
/// cannot be used in current implementation
/// </remarks>
public class ChromaBase64
{
    public static Encoding ByteEncoding = Encoding.GetEncoding("Latin1");
    
    private static byte[] kBase64Chars = ByteEncoding.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_");
    private static byte[] kBase64CharsReversed =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 52,
        53, 54, 55, 56, 57, 58, 59, 60, 61, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5,
        6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
        25, 0, 0, 0, 0, 63, 0, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37,
        38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 0, 0, 0, 0, 0
    };

    public static string Encode(string plain)
    {
        int size = plain.Length;
        int encodedSize = (size * 4 + 2) / 3;
        byte[] encoded = new byte[encodedSize];

        byte[] src = ByteEncoding.GetBytes(plain);
        int srcOffset = 0;
        int dest = 0;
        
        while (size > 0)
        {
            encoded[dest++] = kBase64Chars[(src[0 + srcOffset] >> 2)];
            encoded[dest++] = kBase64Chars[((src[0 + srcOffset] << 4) | ((--size > 0) ? (src[1 + srcOffset] >> 4) : 0)) & 63];
            if (size > 0)
            {
                encoded[dest++] = kBase64Chars[((src[1 + srcOffset] << 2) | ((--size > 0) ? (src[2 + srcOffset] >> 6) : 0)) & 63];
                if (size > 0)
                {
                    encoded[dest++] = kBase64Chars[src[2 + srcOffset] & 63];
                    --size;
                }
            }
            srcOffset += 3;
        }

        return ByteEncoding.GetString(encoded);
    }

    
    public static string Decode(string cipher)
    {
        int size = cipher.Length;
        byte[] str = new byte[(3*size)/4];
        byte[] src = ByteEncoding.GetBytes(cipher);

        int srcOffset = 0;
        int dest = 0;
        while (size > 0)
        {
            int b0 = kBase64CharsReversed[src[srcOffset++]];
            if (--size > 0)
            {
                int b1 = kBase64CharsReversed[src[srcOffset++]];
                int r = (b0 << 2) | (b1 >> 4);
                str[dest++] = (byte)r;
                if (--size > 0)
                {
                    int b2 = kBase64CharsReversed[src[srcOffset++]];
                    r = ((b1 << 4) & 255) | (b2 >> 2);
                    str[dest++] = (byte)r;
                    if (--size > 0)
                    {
                        int b3 = kBase64CharsReversed[src[srcOffset++]];
                        r = ((b2 << 6) & 255) | b3;
                        str[dest++] = (byte)r;
                        --size;
                    }
                }
            }
        }

        return ByteEncoding.GetString(str);
    }
}
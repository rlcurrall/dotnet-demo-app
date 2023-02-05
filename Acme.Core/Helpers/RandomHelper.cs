using System.Security.Cryptography;
using System.Text;

namespace Acme.Core.Helpers;

public class RandomHelper
{
    internal static readonly char[] chars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public static string GetUniqueKey(int size)
    {
        var data = new byte[4 * size];
        using (var crypto = RandomNumberGenerator.Create())
        {
            crypto.GetBytes(data);
        }

        StringBuilder result = new(size);
        for (var i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % chars.Length;

            result.Append(chars[idx]);
        }

        return result.ToString();
    }

    public static string GetUniqueKeyOriginal_BIASED(int size)
    {
        var chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        var data = new byte[size];
        using (RNGCryptoServiceProvider crypto = new())
        {
            crypto.GetBytes(data);
        }

        StringBuilder result = new(size);
        foreach (var b in data) result.Append(chars[b % chars.Length]);
        return result.ToString();
    }
}
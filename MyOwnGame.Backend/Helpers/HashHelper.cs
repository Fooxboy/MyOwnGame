using System.Security.Cryptography;
using System.Text;

namespace MyOwnGame.Backend.Helpers;

public class HashHelper
{
    public static string ComputeHash(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            using (var sha = MD5.Create())
            {
                var hash = sha.ComputeHash(stream);
                return ByteArrayToHexString(hash);
            }
        }
    }

    public static bool IsEquals(string one, string two)
    {
        return one.Equals(two);
    }
    
    private static string ByteArrayToHexString(byte[] bytes)
    {
        var result = new StringBuilder(bytes.Length * 2);
        
        foreach (var b in bytes)
        {
            result.Append(b.ToString("x2"));
        }
        
        return result.ToString();
    }
}
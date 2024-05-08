using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Iface.Oik.Tm.Utils
{
  public static class CompressUtil
  {
    private static readonly byte[] GzipHeaderBytes    = { 0x1f, 0x8b, 0x08 };
    private static readonly int    EfficientThreshold = 8192;
    
    
    public static byte[] CompressWhenEfficient(string str)
    {
      return CompressWhenEfficient(Encoding.UTF8.GetBytes(str));
    }
    
    
    public static byte[] Compress(string str)
    {
      return Compress(Encoding.UTF8.GetBytes(str));
    }


    public static byte[] CompressWhenEfficient(byte[] bytes)
    {
      if (bytes.Length < EfficientThreshold)
      {
        return bytes;
      }
      return Compress(bytes);
    }
    
    
    public static byte[] Compress(byte[] bytes)
    {
      using (var outputStream = new MemoryStream())
      {
        using (var gzip = new GZipStream(outputStream, CompressionMode.Compress))
        {
          gzip.Write(bytes, 0, bytes.Length);
        }
        return outputStream.ToArray();
      }
    }
    
  
    public static byte[] Decompress(byte[] bytes)
    {
      using (var outputStream = new MemoryStream())
      using (var inputStream = new MemoryStream(bytes))
      using (var gzip = new GZipStream(inputStream, CompressionMode.Decompress))
      {
        gzip.CopyTo(outputStream);
        return outputStream.ToArray();
      }
    }


    public static string GetRawOrDecompressedString(byte[] bytes)
    {
      return IsProbablyCompressed(bytes)
               ? Encoding.UTF8.GetString(Decompress(bytes))
               : Encoding.UTF8.GetString(bytes);
    }


    public static bool IsProbablyCompressed(byte[] bytes)
    {
      return bytes.Take(GzipHeaderBytes.Length)
                  .SequenceEqual(GzipHeaderBytes);
    }
  }
}
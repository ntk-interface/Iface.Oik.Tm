using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Iface.Oik.Tm.Utils
{
  public static class CompressUtil
  {
    private static readonly byte[] GzipHeaderBytes    = { 0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 4, 0 };
    private static readonly int    EfficientThreshold = 65535;
    
    
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
        Console.WriteLine("NOT EFFICIENT TO COMRPESS");
        return bytes;
      }

      Console.WriteLine("EFFICIENT TO COMPRESS");
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
      if (IsProbablyCompressed(bytes))
      {
        return Encoding.UTF8.GetString(Decompress(bytes));
      }
      return Encoding.UTF8.GetString(bytes);
      /*return IsProbablyCompressed(bytes)
               ? Encoding.UTF8.GetString(Decompress(bytes))
               : Encoding.UTF8.GetString(bytes);*/
    }


    public static bool IsProbablyCompressed(byte[] bytes)
    {
      return bytes.Take(10).SequenceEqual(GzipHeaderBytes);
    }
  }
}
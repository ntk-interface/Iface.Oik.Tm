using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Iface.Oik.Tm.Utils
{
  public static class CompressUtil
  {
    private static          ReadOnlySpan<byte> GzipHeaderBytes => new byte[] { 0x1f, 0x8b, 0x08 };
    private static readonly int                EfficientThreshold = 8192;
    
    
    public static byte[] CompressWhenEfficient(string str)
    {
      if (string.IsNullOrEmpty(str))
      {
        return Array.Empty<byte>();
      }

      if (GetByteCount(str) < EfficientThreshold)
      {
        return GetBytes(str);
      }
      
      return Compress(GetBytes(str));
    }
    
    
    public static byte[] Compress(string str)
    {
      if (string.IsNullOrEmpty(str))
      {
        return Array.Empty<byte>();
      }
      
      return Compress(GetBytes(str));
    }


    public static byte[] CompressWhenEfficient(byte[] bytes)
    {
      if (bytes == null)
      {
        return Array.Empty<byte>();
      }
      if (bytes.Length < EfficientThreshold)
      {
        return bytes;
      }
      return Compress(bytes);
    }
    
    
    public static byte[] Compress(byte[] bytes)
    {
      if (bytes == null || bytes.Length == 0)
      {
        return Array.Empty<byte>();
      }
      
      using (var outputStream = new MemoryStream())
      {
        using (var gzip = new GZipStream(outputStream, CompressionLevel.Fastest))
        {
          gzip.Write(bytes);
        }
        return outputStream.ToArray();
      }
    }
    
  
    public static byte[] Decompress(byte[] bytes)
    {
      if (bytes == null || bytes.Length == 0)
      {
        return Array.Empty<byte>();
      }
      
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
      if (bytes == null || bytes.Length == 0)
      {
        return string.Empty;
      }
      
      return IsProbablyCompressed(bytes)
               ? GetString(Decompress(bytes))
               : GetString(bytes);
    }


    public static bool IsProbablyCompressed(ReadOnlySpan<byte> bytes)
    {
      return bytes.Length >= GzipHeaderBytes.Length &&
             bytes[..GzipHeaderBytes.Length].SequenceEqual(GzipHeaderBytes);
    }


    private static byte[] GetBytes(string str)
    {
      return Encoding.UTF8.GetBytes(str);
    }


    private static int GetByteCount(string str)
    {
      return Encoding.UTF8.GetByteCount(str);
    }


    private static string GetString(ReadOnlySpan<byte> bytes)
    {
      return Encoding.UTF8.GetString(bytes);
    }
  }
}
using System;
using System.Collections.Generic;
using System.Text;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Helpers
{
	public class IniManager : IDisposable
	{
		private const    uint     BuffSizeStep = 1024;
		private const    uint     BufSizeLimit = 0x200000;
		private          IntPtr   _filePointer;
		private readonly Encoding _fileEncoding;

		public IniManager(string filePath, Encoding encoding = null)
		{
			_fileEncoding = encoding ?? Encoding.UTF8;
			_filePointer  = TmNative.ini_Open(EncodingUtil.StringToBytes(filePath));
		}


		public string GetPrivateString(string section, string key, string defaultResponse = "")
		{
			uint bufSize = 0;
			byte[] buf;
			do
			{
				bufSize += BuffSizeStep;
				buf = new byte[bufSize];
				TmNative.ini_ReadString(_filePointer, 
				                        EncodingUtil.StringToBytes(section), 
				                        EncodingUtil.StringToBytes(key), 
				                        EncodingUtil.StringToBytes(defaultResponse), 
				                        buf, 
				                        bufSize);
			} while (buf[^2] != 0 && bufSize < BufSizeLimit);


			return EncodingUtil.BytesToString(buf);
		}


		public Dictionary<string, string> GetPrivateSection(string section)
		{
			var buf = new byte[BuffSizeStep];

			var returnSize = TmNative.ini_ReadSection(_filePointer, 
			                                          EncodingUtil.StringToBytes(section), 
			                                          buf, 
			                                          BuffSizeStep);

			if (buf.Length < returnSize)
			{
				buf = new byte[returnSize];
				TmNative.ini_ReadSection(_filePointer, 
				                         EncodingUtil.StringToBytes(section), 
				                         buf, 
				                         returnSize);
			}

			var significantBytes = new byte[returnSize];
			Array.Copy(buf, significantBytes, returnSize);

			var content = TmNativeUtil.GetStringsListFromBytes(buf, _fileEncoding);

			var result = new Dictionary<string, string>();
			foreach(var line in content)
			{ 
				var equalSingIndex = line.IndexOf('=');
				if(equalSingIndex != -1)
				{
					result.Add(line[..equalSingIndex], line[(equalSingIndex + 1)..]);
				}
			}
			return result;
		}


		public byte[] GetStruct(string section, string key, uint bufSize = 1024)
		{
			var buf = new byte[bufSize];

			var returnSize = TmNative.ini_ReadStruct(_filePointer, 
			                                         EncodingUtil.StringToBytes(section), 
			                                         EncodingUtil.StringToBytes(key), 
			                                         buf, 
			                                         bufSize);

			if (buf.Length < returnSize)
			{
				buf = new byte[returnSize];
				TmNative.ini_ReadSection(_filePointer, 
				                         EncodingUtil.StringToBytes(section), 
				                         buf, 
				                         returnSize);
			}

			var significantBytes = new byte[returnSize];
			Array.Copy(buf, significantBytes, returnSize);

			return significantBytes;
		}


		public void Dispose()
		{
			TmNative.ini_Close(_filePointer);
			_filePointer = IntPtr.Zero;
		}
	}
}
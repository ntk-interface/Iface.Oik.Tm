using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Helpers
{
	public class IniManager : IDisposable
	{
		private const uint BuffSizeStep = 1024;
		private const uint BufSizeLimit = 0x200000;
		private readonly ITmNative _native;
		private IntPtr _filePointer;

		public IniManager(string filePath)
		{
			_native = new TmNative();

			_filePointer = _native.IniOpen(EncodingUtil.Utf8ToWin1251Bytes(filePath));
		}


		public string GetPrivateString(string section, string key, string defaultResponse = "")
		{
			uint bufSize = 0;
			byte[] buf;
			do
			{
				bufSize += BuffSizeStep;
				buf = new byte[bufSize];
				_native.IniReadString(_filePointer, 
				                      EncodingUtil.Utf8ToWin1251Bytes(section), 
				                      EncodingUtil.Utf8ToWin1251Bytes(key), 
				                      EncodingUtil.Utf8ToWin1251Bytes(defaultResponse), 
				                      buf, 
				                      bufSize);
			} while (buf[buf.Length - 2] != 0 && bufSize < BufSizeLimit);


			return EncodingUtil.Win1251BytesToUtf8(buf);
		}


		public Dictionary<string, string> GetPrivateSection(string section)
		{
			var buf = new byte[BuffSizeStep];

			var returnSize = _native.IniReadSection(_filePointer, 
			                                        EncodingUtil.Utf8ToWin1251Bytes(section), 
			                                        buf, 
			                                        BuffSizeStep);

			if (buf.Length < returnSize)
			{
				buf = new byte[returnSize];
				_native.IniReadSection(_filePointer, 
				                       EncodingUtil.Utf8ToWin1251Bytes(section), 
				                       buf, 
				                       returnSize);
			}

			var significantBytes = new byte[returnSize];
			Array.Copy(buf, significantBytes, returnSize);

			var content = EncodingUtil.Win1251BytesToUtf8(significantBytes)
			 		       .Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

			var result = new Dictionary<string, string>();
			foreach(var line in content)
			{ 
				var equalSingIndex = line.IndexOf('=');
				if(equalSingIndex != -1)
				{
					result.Add(line.Substring(0, equalSingIndex), line.Substring(equalSingIndex+1));
				}
			}
			return result;
		}


		public byte[] GetStruct(string section, string key, uint bufSize = 1024)
		{
			var buf = new byte[bufSize];

			var returnSize = _native.IniReadStruct(_filePointer, 
			                                       EncodingUtil.Utf8ToWin1251Bytes(section), 
			                                       EncodingUtil.Utf8ToWin1251Bytes(key), 
			                                       buf, 
			                                       bufSize);

			if (buf.Length < returnSize)
			{
				buf = new byte[returnSize];
				_native.IniReadSection(_filePointer, 
				                       EncodingUtil.Utf8ToWin1251Bytes(section), 
				                       buf, 
				                       returnSize);
			}

			var significantBytes = new byte[returnSize];
			Array.Copy(buf, significantBytes, returnSize);

			return significantBytes;
		}


		public void Dispose()
		{
			_native.IniClose(_filePointer);
			_filePointer = IntPtr.Zero;
		}
	}
}
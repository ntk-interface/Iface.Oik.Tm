using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Helpers
{
	public static class Cfs
	{

		public const string ServerTypeTms = "tms$";

		public static readonly ITmNative Native = new TmNative();

		public static void InitializeCfsLibrary()
		{
			Native.CfsInitLibrary();
		}

		public static void SetUserCredentials(string user,
											  string password)
		{
			Native.CfsSetUser(EncodingUtil.Utf8ToWin1251Bytes(user), EncodingUtil.Utf8ToWin1251Bytes(password));
		}
		
		public static string MakeInprocCrd(string host, string user, string pwd)
		{
			var ptr = Native.CfsMakeInprocCrd(EncodingUtil.Utf8ToWin1251Bytes(host),
			                                  EncodingUtil.Utf8ToWin1251Bytes(user),
			                                  EncodingUtil.Utf8ToWin1251Bytes(pwd));
			if (ptr == IntPtr.Zero)
			{
				return string.Empty;
			}
			
			var res = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(ptr);
			Native.CfsFreeMemory(ptr);
			return res;

		}

		public static (IntPtr cfId, string errString, int errorCode) ConnectToCfs(string host)
		{
			const int errStringLength = 1000;
			var errBuf = new byte[errStringLength];

			var cfId =
			  Native.CfsConnect(EncodingUtil.Utf8ToWin1251Bytes(host), out uint errCode, ref errBuf, errStringLength);

			if (cfId == IntPtr.Zero)
			{
				Console.WriteLine($"Ошибка соединения с мастер-сервисом: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errBuf)}");
			}

			return (cfId, EncodingUtil.Win1251BytesToUtf8(errBuf), Convert.ToInt32(errCode));
		}

		public static (IntPtr, CfsDefs.InitializeConnectionResult) InitializeConnection(CfsOptions options)
		{
			SetUserCredentials(options.User, options.Password);

			var (cfsCid, errString, errorCode) = ConnectToCfs(options.Host);

			switch (errorCode)
			{
				case 0:
					return (cfsCid, CfsDefs.InitializeConnectionResult.Ok);
				case 87:
					Console.WriteLine("Соединение не установлено. " + errString);
					return (cfsCid, CfsDefs.InitializeConnectionResult.InvalidLoginOrPassword);
				default:
					Console.WriteLine("Соединение не установлено. " + errString);
					return (cfsCid, CfsDefs.InitializeConnectionResult.NonSpecifiedError);
			}
		}

		public static bool IsConnected(IntPtr cfsCid)
		{
			return cfsCid != IntPtr.Zero;
		}

		public class CfsOptions
		{
			public string Host { get; set; }
			public string User { get; set; }
			public string Password { get; set; }
		}

		// todo надо ли вообще здесь такую реализацию
		
		public static TmUserInfo GetUserInfo(IntPtr cfCid,
											 string serverName,
											 string serverType)
		{
			var nativeUserInfoSize = Marshal.SizeOf(typeof(TmNativeDefs.TExtendedUserInfo));
			var nativeUserInfoPtr = Marshal.AllocHGlobal(nativeUserInfoSize);

			var fetchResult = Native.CfsGetExtendedUserData(cfCid,
			                                                EncodingUtil.Utf8ToWin1251Bytes(serverType),
			                                                EncodingUtil.Utf8ToWin1251Bytes(serverName),
															nativeUserInfoPtr,
															(uint)nativeUserInfoSize);
			if (fetchResult == 0)
			{
				Marshal.FreeHGlobal(nativeUserInfoPtr); // не забываем освобождать память из HGlobal
				return null;
			}

			var nativeUserInfo = Marshal.PtrToStructure<TmNativeDefs.TExtendedUserInfo>(nativeUserInfoPtr);
			Marshal.FreeHGlobal(nativeUserInfoPtr); // не забываем освобождать память из HGlobal

			return new TmUserInfo(nativeUserInfo.UserId,
								  Encoding.GetEncoding(1251).GetString(nativeUserInfo.UserName).Trim('\0'),
								  string.Empty, // todo надо ли сделать получать категорию
								  Encoding.GetEncoding(1251).GetString(nativeUserInfo.KeyId).Trim('\0'),
								  nativeUserInfo.Group,
								  nativeUserInfo.Rights);
		}

		public static void CloseCfsConnection(IntPtr cfId)
		{
			Native.CfsDisconnect(cfId);
		}


		public static (bool hasNus, TmNativeDefs.NewUserSystem nusFlags) GetNewUserSystemFlags(IntPtr cfCid)
		{
			const int errBufLength = 1000;
			var       errBuf       = new byte[errBufLength];
			uint      errCode      = 0;

			var hasNus = Native.CfsIfpcNewUserSystemAvaliable(cfCid, out var nusFlags, out errCode, ref errBuf, errBufLength);
			return (hasNus, (TmNativeDefs.NewUserSystem)nusFlags);
		}
	}
}
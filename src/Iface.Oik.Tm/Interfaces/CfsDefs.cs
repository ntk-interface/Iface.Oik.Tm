using System;
using System.Collections.Generic;
using System.ComponentModel;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces
{
	public class CfsDefs
	{
		public enum InitializeConnectionResult
		{
			Ok = 0,

			[Description("Ошибка авторизации!")] InvalidLoginOrPassword = 87,

			[Description("Ошибка соединения!")] NonSpecifiedError = 1000,
		}


		public enum MasterServiceStatus
		{
			[Description("Потеряно соединение!")] LostConnection = 0,

			[Description("Мастер-сервис остановлен!")] Stopped = 1,

			[Description("Мастер-сервис запущен!")] Running = 2
		}


		public enum SoftwareTypes
		{
			Unknown = -1,
			Old = 0,
			Version3 = 1
		}
	}


	public enum LicenseFeature
	{
		Terminator = 0,
		Expiration = 1,
		Teleparms = 2,
		MaxUsers = 3,
		Hotres = 4,
		UpdLimit = 5,
		UseModus = 6,
		UseSpa = 7,
		UseIec101 = 8,
		UseIpc = 9,
		Magazine = 10,
		CrTime = 11,
		PlcDir = 12,
		UseIec103 = 13,
		UseOpc = 14,
		UseSmsGate = 15,
		UseIec61850 = 16,
		UseSqlGate = 17,
		ArisScada = 18,
		WebClient = 19,
		Aan = 20,
		Pin = 21,
		Platform = 22,
		AtiScada = 23,
		UseDnp3 = 24,
		ScadaSec = 25,
		TotParms = 26,
		Version = 27,
		LocalClient = 28,
		Spodes = 29,
		Client10 = 30,
		WebClient10 = 31,
		Client = 32,
		LicNum = 1024,
	}


	public enum LicenseFeatureStatus
	{
		Ok = 1,
		No = 0,
		Wait = -1,
		Error = -2,
	}

	public enum LicenseKeyType
	{
		[Description("Неизвестный тип ключа")] Unknown = 0,
		[Description("???")] TypeFour = 4,
		[Description("Программная лицензия")] Software = 5,
		[Description("Interface USB HID/SSD")] UsbHidSsd = 6,
		[Description("Сетевой ключ Interface")] Network = 7,

	}


	public enum SLogType : uint
	{
		Security = TmNativeDefs.SLogType.Security,
		Administrator = TmNativeDefs.SLogType.Administrator,
	}

	public enum SLogReadDirection
	{
		FromEnd = 0,
		FromStart = 1
	}

	public static class SLogIndex
	{
		public const uint First = 0x00_00_00_00;
		public const uint Last = 0x7f_ff_ff_ff;
	}
	public class AccessMask
	{
		public uint Mask { get; set; }
		public Dictionary<string, string> Description { get; set; } = new Dictionary<string, string>();
	}
	public class AccessDescriptor
	{
		public string NamePrefix = "";
		public Dictionary<string, string> ObjTypeName { get; set; } = new Dictionary<string, string>();
		public List<AccessMask> AccessMasks { get; set; } = new List<AccessMask>();
	}
	public class ExtendedRight
	{
		public bool IsHeader { get; set; }
		public byte ByteIndex { get; set; }
		public Dictionary<string, string> Description { get; set; } = new Dictionary<string, string>();
	}
	public class ExtendedRightsDescriptor
	{
		public byte DoUserID { get; set; }
		public uint MaxUserID { get; set; }
		public byte DoGroup { get; set; }
		public byte DoKeyID { get; set; }
		public byte DoUserNick { get; set; }
		public byte DoUserPwd { get; set; }
		public List<ExtendedRight> Rights { get; set; } = new List<ExtendedRight>();
	}
}
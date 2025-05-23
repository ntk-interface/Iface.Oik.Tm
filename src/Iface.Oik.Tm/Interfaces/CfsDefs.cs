using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
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
		DallasCOM = 0,
		DallasLPT = 1,
		DallasUSB = 2,
		IfaceUSBCOM = 3,
		HDDKey = 4,
		Software = 5,
		UsbHidSsd = 6,
		Network = 7,
		Unknown = -1,
	}


	public enum CfsSLogType : uint
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

	[Flags]
	public enum MS_AccessRights : uint
	{
		ReadConfig = 0x00_00_00_01,
		WriteConfig = 0x00_00_00_02,
		DirectoryAccess = 0x00_00_00_04,
		ServersAccess = 0x00_00_00_08,
		Trace = 0x00_00_00_10,
		ReadSecurityLog = 0x00_00_00_20,
		ReadAdminLog = 0x00_00_00_40,
		EditSecurity = 0x00_00_80_00,
	}
	public class AccessMask
	{
		public uint Mask { get; set; }
		public Dictionary<string, string> Description { get; set; } = new Dictionary<string, string>();
	}
	public class AccessMasksDescriptor
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
	public class ExtendedUserData
	{
		public int UserID{ get; set; }
		public int Group { get; set; }
		public string UserNick { get; set; } = "";
		public string UserPwd { get; set; } = "";
		public string KeyID { get; set; } = "";
		public byte[] Rights { get; set; } = new byte[250];
	}
	public class UserPolicy
	{
		[ReadOnly(true)]
		public bool Predefined { get; set; }

		[ReadOnly(true)]
		public bool PasswordSet { get; set; }

		[ReadOnly(true)]
		public int BadLogonCount { get; set; }

		public string UserTemplate { get; set; } = "";

		public bool IsBlocked { get; set; }

		public bool MustChangePassword { get; set; }

		public DateTime NotBefore { get; set; }

		public DateTime NotAfter { get; set; }

		public int BadLogonLimit { get; set; }

		[XmlArray]
		public string EnabledMACs { get; set; } = "";

		public string UserCategory { get; set; } = "";

	}

	[Flags]
	public enum PWDPOL : uint
	{
		Upper = 0x00001,
		Digits = 0x00002,
		Spec = 0x00004,
		CheckRepeat = 0x00008,
		CheqSeq = 0x00010,
		CheckDict = 0x00020,
		CheckCache = 0x10000,
	}

	public class PasswordPolicy
	{
		public bool AdminPasswordChange { get; set; }
		public int PasswordTTL_Days { get; set; }
		public bool EnforcePasswordCheck { get; set; }
		public int MinPasswordLength { get; set; }
		public bool PwdChars_Upper { get; set; }
		public bool PwdChars_Digits { get; set; }
		public bool PwdChars_Special { get; set; }
		public bool PwdChars_NoRepeat { get; set; }
		public bool PwdChars_NoSequential { get; set; }
		public bool PwdChars_CheckDictonary { get; set; }
		public bool CheckOldPasswords { get; set; }
	}
	public class ComputerInfo
	{
		public string Copyright { get; set; }
		public string CfsVer { get; set; }
		public string BuildDate { get; set; }
		public string InstallDate { get; set; }
		public string SoftwareKeyID { get; set; }
		public string ComputerName { get; set; }
		public string PrimaryDomainName { get; set; }
		public string OS_ProductType { get; set; }
		public string OS_Version { get; set; }
		public string Architecture { get; set; }
		public List<string> IpAddrs { get; set; }
		public UInt32 Acp { get; set; }
		public UInt64 Uptime { get; set; }
		public DateTime ServerTimeGMT { get; set; }
		public string UserName { get; set; }
		public string UserAddr { get; set; }
		public UInt32 AccessMask { get; set; }
	}

	public enum ReserveState : uint
	{
		NotReserved = 0,
		
		MainUndefined       = 1,
		MainConnecting      = 2,
		MainSynchronization = 3,
		MainActive          = 4,
		MainNotConnected        = 5,
		
		ReserveConnectedToMain = 0x1001,
		ReserveActive = 0x1002,
		ReserveUndefined = 0x1003,
	}

	public enum BroadcastServerSignature : uint
	{
		None = 0,
		
		Sbr = 0x524253,
		Smt = 0x544d53
	}

	public enum PasswordDigestState
	{
		NotSupported,
		DoesNotExists,
		Exists,
	}
}
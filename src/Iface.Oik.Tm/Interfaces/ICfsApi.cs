using Iface.Oik.Tm.Native.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iface.Oik.Tm.Interfaces
{
	public interface ICfsApi
	{
		void SetCfIdAndHost(IntPtr cfId, string host);

		Task<(IntPtr, DateTime)> OpenConfigurationTree(string fileName);
		Task<(IntPtr, DateTime)> OpenMasterServiceConfiguration();
		Task<(MSTreeNode, DateTime)> LoadFullMSTree();
		Task SaveConfigurationTree(IntPtr treeHandle, string filename);
		Task SaveMasterServiceConfiguration(IntPtr treeHandle);
		Task SaveFullMSTree(MSTreeNode msRoot);

		Task<List<CfTreeNode>> GetCfTree(IntPtr rootHandle);

		Task<IntPtr> CreateNewMasterServiceTree(MSTreeNode msRoot);

		void FreeConfigurationTreeHandle(IntPtr handle);

		void FreeMasterServiceConfigurationHandle(IntPtr handle);



		Task<CfsDefs.SoftwareTypes> GetSoftwareType();


		Task<CfsDefs.MasterServiceStatus> MasterServiceStatus();


		Task StartMasterService();


		Task StopMasterService();


		Task<bool> IsConnected();


		Task<IReadOnlyCollection<TmServer>> GetTmServersTree();


		Task<IReadOnlyCollection<TmServerLogRecord>> GetTmServersLog();


		Task<IReadOnlyCollection<TmServerLogRecord>> GetTmServersLog(int maxRecords, DateTime? startTime, DateTime? endTime);


		Task<IReadOnlyCollection<TmServerThread>> GetTmServersThreads();


		Task RegisterTmServerTracer(ITmServerTraceable traceTarget, bool debug, int pause);


		Task StopTmServerTrace();


		Task<IReadOnlyCollection<TmServerLogRecord>> TraceTmServerLogRecords();


		Task<TmInstallationInfo> GetTmInstallationInfo();


		Task<TmLicenseInfo> GetLicenseInfo();


		Task<IReadOnlyCollection<LicenseKeyType>> GetAvailableLicenseKeyTypes();


		Task SetLicenseKeyCom(TmLicenseKey newLicenseKey);


		Task<IReadOnlyCollection<string>> GetFilesInDirectory(string path);


		Task PutFile(string localFilePath,
					 string remoteFilePath,
					 uint timeout);


		Task DeleteFile(string remoteFilePath);


		Task<IReadOnlyCollection<string>> GetInstalledLicenseKeyFiles();


		Task PutLicenseKeyFile(string filePath);


		Task DeleteLicenseKeyFile(string fileName);


		Task<IReadOnlyCollection<SLogRecord>> GetSecurityLogFull(SLogReadDirection readDirection = SLogReadDirection.FromEnd);


		Task<IReadOnlyCollection<SLogRecord>> GetAdministratorLogFull(SLogReadDirection readDirection = SLogReadDirection.FromEnd);


		Task<IReadOnlyCollection<SLogRecord>> GetSecurityLog(int               maxRecords,
		                                                     SLogReadDirection readDirection = SLogReadDirection.FromEnd, 
		                                                     uint              startIndex    = SLogIndex.Last,
		                                                     DateTime?         startTime     = null, 
		                                                     DateTime?         endTime       = null);


		Task<IReadOnlyCollection<SLogRecord>> GetAdministratorLog(int maxRecords,
		                                                          SLogReadDirection readDirection = SLogReadDirection.FromEnd, 
		                                                          uint              startIndex = SLogIndex.Last,
		                                                          DateTime?         startTime = null, 
		                                                          DateTime?         endTime = null);


		Task<int> GetRedirectorPort(string pipeName, int portIndex);


		Task<byte[]> GetBin(string uName,
		                    string oName,
		                    string binName);


		Task<bool> SetRedirectorPort(string pipeName, int portIndex, int port);


		Task<bool> SetBin(string uName,
		                  string oName,
		                  string binName,
		                  byte[] binData);

		AccessDescriptor GetAccessDescriptor(string ProgName);
		ExtendedRightsDescriptor GetExtendedRightsDescriptor();
		Task<(bool, string, IReadOnlyCollection<string>)> IfpcEnumUsers();
		Task<(bool, string, IReadOnlyCollection<string>)> IfpcEnumOSUsers();
		Task<(bool, string)> IfpcChangeUserPassword(string username, string password);
		Task<(bool, string)> IfpcDeleteUser(string username);
		Task<(bool, string, uint)> IfpcGetAccessMask(string uName, string oName);
		Task<(bool, string)> IfpcSetAccessMask(string uName, string oName, uint AccessMask);
	}
}
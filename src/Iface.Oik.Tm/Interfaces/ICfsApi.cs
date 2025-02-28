using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iface.Oik.Tm.Interfaces
{
  public interface ICfsApi
  {
    void                     SetCfIdAndHost(IntPtr                           cfId,    string host);
    string                   MakeInprocCrd(string                            machine, string user, string pwd);
    Task<(IntPtr, DateTime)> OpenConfigurationTree(string                    fileName);
    IntPtr                   CreateConfigurationTree(IEnumerable<CfTreeNode> tree);
    Task<(IntPtr, DateTime)> OpenMasterServiceConfiguration();

    Task<IReadOnlyCollection<CfTreeNode>> GetReserveConfiguration();

    Task<ReserveServerState> ReserveServerTypeIsWorking(CfTreeNode reserveConfNode);

    Task OverrideReservePipe(string pipeName, BroadcastServerSignature signature);


    Task<(MSTreeNode, DateTime)> LoadFullMSTree();
    Task                         SaveConfigurationTree(IntPtr                treeHandle, string filename);
    Task                         SaveMasterServiceConfiguration(IntPtr       treeHandle);
    Task                         SaveFullMSTree(MSTreeNode                   msRoot);
    Task<List<CfTreeNode>>       GetCfTree(IntPtr                            rootHandle, CfTreeNode parent = null);
    void                         FreeConfigurationTreeHandle(IntPtr          handle);
    void                         FreeMasterServiceConfigurationHandle(IntPtr handle);


    Task<CfsDefs.SoftwareTypes> GetSoftwareType();


    Task<CfsDefs.MasterServiceStatus> MasterServiceStatus();


    Task StartMasterService();


    Task StopMasterService();


    Task<bool> IsConnected();

    Task<string> GetBasePath();

    Task<IReadOnlyCollection<string>> GetTimezones();

    Task<IReadOnlyCollection<TmServer>> GetTmServersTree();


    Task<IReadOnlyCollection<TmServerLogRecord>> GetTmServersLog();


    Task<IReadOnlyCollection<TmServerLogRecord>>
      GetTmServersLog(int maxRecords, DateTime? startTime, DateTime? endTime);


    Task<IReadOnlyCollection<TmServerThread>> GetTmServersThreads();


    Task RegisterTmServerTracer(ITmServerTraceable traceTarget, bool debug, int pause);


    Task StopTmServerTrace();


    Task<IReadOnlyCollection<TmServerLogRecord>> TraceTmServerLogRecords();


    Task<TmInstallationInfo> GetTmInstallationInfo();


    Task<TmLicenseInfo> GetLicenseInfo();


    Task<IReadOnlyCollection<LicenseKeyType>> GetAvailableLicenseKeyTypes();


    Task SetLicenseKeyCom(TmLicenseKey newLicenseKey);


    Task<IReadOnlyCollection<string>> GetFilesInDirectory(string path);


    Task<(bool, string)> PutFile(string localFilePath,
                                 string remoteFilePath,
                                 uint   timeout);

    Task DeleteFile(string remoteFilePath);


    Task<IReadOnlyCollection<string>> GetInstalledLicenseKeyFiles();


    Task PutLicenseKeyFile(string filePath);


    Task DeleteLicenseKeyFile(string fileName);


    Task<IReadOnlyCollection<SLogRecord>> GetSecurityLogFull(
      SLogReadDirection readDirection = SLogReadDirection.FromEnd);


    Task<IReadOnlyCollection<SLogRecord>> GetAdministratorLogFull(
      SLogReadDirection readDirection = SLogReadDirection.FromEnd);


    Task<IReadOnlyCollection<SLogRecord>> GetSecurityLog(int               maxRecords,
                                                         SLogReadDirection readDirection = SLogReadDirection.FromEnd,
                                                         uint              startIndex    = SLogIndex.Last,
                                                         DateTime?         startTime     = null,
                                                         DateTime?         endTime       = null);


    Task<IReadOnlyCollection<SLogRecord>> GetAdministratorLog(int maxRecords,
                                                              SLogReadDirection readDirection =
                                                                SLogReadDirection.FromEnd,
                                                              uint      startIndex = SLogIndex.Last,
                                                              DateTime? startTime  = null,
                                                              DateTime? endTime    = null);


    Task<int> GetRedirectorPort(string pipeName, int portIndex);


    Task<PasswordDigestState> GetBackupPasswordDigestState();


    Task<byte[]> GetBin(string uName,
                        string oName,
                        string binName);

    Task<(byte[], uint, string)> SecGetBin(string uName,
                                           string oName,
                                           string binName);

    Task<bool> SetRedirectorPort(string pipeName, int portIndex, int port);


    Task<bool> SetBin(string uName,
                      string oName,
                      string binName,
                      byte[] binData);

    Task<(uint, string)> SecSetBin(string uName,
                                   string oName,
                                   string binName,
                                   byte[] binData);

    AccessMasksDescriptor SecGetAccessDescriptor(string sSetupPath, string progName);

    ExtendedRightsDescriptor SecGetExtendedRightsDescriptor(string sSetupPath);

    Task<(IReadOnlyCollection<string>, uint, string)> SecEnumUsers();

    Task<(IReadOnlyCollection<string>, uint, string)> SecEnumOSUsers();

    Task<(uint, string)> SecChangeUserPassword(string username, string password);

    Task<(uint, string)> SecDeleteUser(string username);

    Task<(uint, uint, string)> SecGetAccessMask(string uName, string oName);

    Task<(uint, string)> SecSetAccessMask(string uName, string oName, uint AccessMask);

    Task<(ExtendedUserData, uint, string)>
      SecGetExtendedUserData(string serverType, string serverName, string username);

    Task<(uint, string)> SecSetExtendedUserData(string           serverType, string serverName, string username,
                                                ExtendedUserData extendedUserData);

    Task<(UserPolicy, uint, string)> SecGetUserPolicy(string username);

    Task<(uint, string)> SecSetUserPolicy(string username, UserPolicy userPolicy);

    Task<(PasswordPolicy, uint, string)> SecGetPasswordPolicy();

    Task<(uint, string)> SecSetPasswordPolicy(PasswordPolicy passwordPolicy);

    Task<(IReadOnlyCollection<string>, uint, string)> DirEnum(string path);

    Task<(bool, string)> SaveMachineConfig(string directory, bool full);

    Task<(bool, string)> SaveMachineConfigEx(string           directory,
                                             uint             scope,
                                             TmNativeCallback callback          = null,
                                             IntPtr           callbackParameter = default);

    Task<(bool, string)> RestoreMachineConfig(string filename);

    Task<(bool, string)> CreateBackup(string           progName,
                                      string           pipeName,
                                      string           directory,
                                      bool             withRetro,
                                      TmNativeCallback callback          = null,
                                      IntPtr           callbackParameter = default);

    Task<(bool, string)> RestoreBackup(string           progName,
                                       string           pipeName,
                                       string           filename,
                                       bool             withRetro,
                                       TmNativeCallback callback          = null,
                                       IntPtr           callbackParameter = default);

    Task<(uint, string)> BackupSecurity(string directory, string pwd = "");

    Task<(uint, string)> RestoreSecurity(string filename, string pwd);

    Task<(IReadOnlyCollection<string>, string)> EnumPackedFiles(string pkfName);

    Task<(IReadOnlyCollection<string>, string)> UnPack(string pkfName,
                                                       string dirname);

    Task<(bool, string)> ExtractFile(string pkfName,
                                     string filename,
                                     string dirname);

    Task<(uint, string, UInt64, UInt32)> StartTestTmcalc(string tmsName,
                                                         string clcName,
                                                         UInt32 testWay,
                                                         UInt32 testFlags);

    Task<(uint, string)> StopTestTmcalc(UInt64 handle,
                                        UInt32 pid);

    Task<(bool, string)> PmonCheckProcess(string processNameArgs);

    Task<(bool, string)> PmonStopProcess(string processNameArgs);

    Task<(bool, string)> PmonRestartProcess(string processNameArgs);

    Task<(bool, string)> SwapFnSrvRole(string encodedCredentials, string fnsName, bool dryRun);

    Task<(bool, string)> ReserveRoleSwapIsAvailable(string encodedCredentials, string pipeName);

    Task<(bool, string)> SwapReserveRole(string encodedCredentials, string pipeName);

    Task<(bool, int, string)> AddPasswordToAutoBackupDigest(string password);
  }
}
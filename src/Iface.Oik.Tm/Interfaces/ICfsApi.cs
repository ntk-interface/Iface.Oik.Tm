using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iface.Oik.Tm.Interfaces
{
  public interface ICfsApi
  {
    void SetCfIdAndHost(IntPtr cfId, string host);

    
    Task<(IntPtr, DateTime)> OpenMasterServiceConfiguration();

    
    Task<bool> SaveMasterServiceConfiguration(IntPtr treeHandle, string serverName);

    
    Task<List<MSTreeNode>> GetMasterServiceTree(IntPtr rootHandle);

    
    Task<IntPtr> CreateNewMasterServiceTree(IEnumerable<MSTreeNode> tree);

    
    void FreeMasterServiceConfigurationHandle(IntPtr handle);

    
    Task<CfsDefs.SoftwareTypes> GetSoftwareType();

    
    Task<CfsDefs.MasterServiceStatus> MasterServiceStatus();

    
    Task StartMasterService();

    
    Task StopMasterService();

    
    Task<bool> IsConnected();


    Task<IReadOnlyCollection<TmServer>> GetTmServersTree();
    
    
    Task<IReadOnlyCollection<TmServerLogRecord>> GetTmServersLog();
    
    
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
                 uint   timeout);

    
    Task DeleteFile(string remoteFilePath);


    Task<IReadOnlyCollection<string>> GetInstalledLicenseKeyFiles();


    Task PutLicenseKeyFile(string filePath);


    Task DeleteLicenseKeyFile(string fileName);
  }
}
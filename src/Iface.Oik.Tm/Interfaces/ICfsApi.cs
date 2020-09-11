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

    
    Task<List<CfTreeNode>> GetMasterServiceTree(IntPtr rootHandle);

    
    Task<IntPtr> CreateNewMasterServiceTree(IEnumerable<CfTreeNode> tree);

    
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
  }
}
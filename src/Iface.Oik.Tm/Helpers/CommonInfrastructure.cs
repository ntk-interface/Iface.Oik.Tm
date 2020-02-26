using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Helpers
{
  public class CommonInfrastructure : ICommonInfrastructure
  {
    public IOikDataApi          OikDataApi    { get; }
    public ITmsApi              TmsApi        { get; }
    public IOikSqlApi           OikSqlApi     { get; }
    public ICommonServerService ServerService { get; }

    public int        TmCid      { get; private set; }
    public int        RbCid      { get; private set; }
    public int        RbPort     { get; private set; }
    public TmUserInfo TmUserInfo { get; private set; }


    public CommonInfrastructure(IOikDataApi          api,
                                ITmsApi              tms,
                                IOikSqlApi           sql,
                                ICommonServerService serverService)
    {
      OikDataApi    = api;
      TmsApi        = tms;
      OikSqlApi     = sql;
      ServerService = serverService;

      OikSqlApi.SetCreateOikSqlConnection(CreateOikSqlConnection);
      ServerService.SetCreateOikSqlConnection(CreateOikSqlConnection);
    }


    public void InitializeTm(int tmCid, int rbCid, int rbPort, TmUserInfo userInfo)
    {
      TmCid      = tmCid;
      RbCid      = rbCid;
      RbPort     = rbPort;
      TmUserInfo = userInfo;

      OikDataApi.SetUserInfo(TmUserInfo);
      TmsApi.SetCidAndUserInfo(TmCid, TmUserInfo);
      ServerService.SetTmCid(TmCid);
    }


    public void InitializeTmWithoutSql(int tmCid, TmUserInfo userInfo)
    {
      TmCid      = tmCid;
      TmUserInfo = userInfo;

      OikDataApi.SetUserInfo(TmUserInfo);
      TmsApi.SetCidAndUserInfo(TmCid, TmUserInfo);
      ServerService.SetTmCid(TmCid);

      ServerService.CheckSqlConnection = false;
      OikDataApi.IsSqlAllowed          = false;
    }


    public void TerminateTm()
    {
      TmCid      = 0;
      RbCid      = 0;
      RbPort     = 0;
      TmUserInfo = null;
    }


    public virtual ICommonOikSqlConnection CreateOikSqlConnection()
    {
      return new CommonOikSqlConnection(RbPort);
    }
  }
}
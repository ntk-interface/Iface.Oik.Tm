using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Helpers
{
  public class CommonInfrastructure : ICommonInfrastructure
  {
    public IOikDataApi          OikDataApi    { get; }
    public ITmsApi              TmsApi        { get; }
    public IOikSqlApi           OikSqlApi     { get; }
    public ICommonServerService ServerService { get; }

    public int              TmCid            { get; private set; }
    public int              RbCid            { get; private set; }
    public int              RbPort           { get; private set; }
    public TmUserInfo       TmUserInfo       { get; private set; }
    public TmServerFeatures TmServerFeatures { get; private set; }


    public CommonInfrastructure(IOikDataApi          api,
                                ITmsApi              tms,
                                IOikSqlApi           sql,
                                ICommonServerService serverService)
    {
      OikDataApi    = api;
      TmsApi        = tms;
      OikSqlApi     = sql;
      ServerService = serverService;
    }


    public void InitializeTm(int tmCid, int rbCid, int rbPort, TmUserInfo userInfo, TmServerFeatures features)
    {
      TmCid            = tmCid;
      RbCid            = rbCid;
      RbPort           = rbPort;
      TmUserInfo       = userInfo;
      TmServerFeatures = features;

      OikSqlApi.SetCreateOikSqlConnection(CreateOikSqlConnection);
      ServerService.SetCreateOikSqlConnection(CreateOikSqlConnection);

      OikDataApi.SetUserInfoAndServerFeatures(TmUserInfo, TmServerFeatures);
      TmsApi.SetCidAndUserInfo(TmCid, TmUserInfo);
      ServerService.SetTmCid(TmCid);
    }


    public void InitializeTmWithoutSql(int tmCid, TmUserInfo userInfo, TmServerFeatures features)
    {
      TmCid            = tmCid;
      TmUserInfo       = userInfo;
      TmServerFeatures = features;

      OikDataApi.SetUserInfoAndServerFeatures(TmUserInfo, TmServerFeatures);
      TmsApi.SetCidAndUserInfo(TmCid, TmUserInfo);
      ServerService.SetTmCid(TmCid);

      ServerService.CheckSqlConnection = false;
      OikDataApi.IsSqlAllowed          = false;
    }


    public void TerminateTm()
    {
      TmCid            = 0;
      RbCid            = 0;
      RbPort           = 0;
      TmUserInfo       = null;
      TmServerFeatures = TmServerFeatures.Empty;
    }


    public virtual ICommonOikSqlConnection CreateOikSqlConnection()
    {
      return new CommonOikSqlConnection(RbPort);
    }
  }
}
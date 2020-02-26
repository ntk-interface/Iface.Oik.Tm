namespace Iface.Oik.Tm.Interfaces
{
  public interface ICommonInfrastructure
  {
    IOikDataApi          OikDataApi    { get; }
    ITmsApi              TmsApi        { get; }
    IOikSqlApi           OikSqlApi     { get; }
    ICommonServerService ServerService { get; }

    int        TmCid      { get; }
    int        RbCid      { get; }
    int        RbPort     { get; }
    TmUserInfo TmUserInfo { get; }


    void InitializeTm(int        tmCid,
                      int        rbCid,
                      int        rbPort,
                      TmUserInfo userInfo);


    void InitializeTmWithoutSql(int        tmCid,
                                TmUserInfo userInfo);


    void TerminateTm();


    ICommonOikSqlConnection CreateOikSqlConnection();
  }
}
using System;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.IntegrationTest.Util;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.IntegrationTest.Workers;

public static class TestStaticTms
{
  public static void DoWork()
  {
    TestConnection();
    TestTmMethods();
    TestRbMethods();
  }


  private static void TestConnection()
  {
    var (host, tmServer, _, username, password) = CommonUtil.ParseCommandLineArgs();

    Tms.SetUserCredentials(username, "totally not a password");
    var cid1 = Tms.Connect(host, tmServer, CommonUtil.TaskName, null, IntPtr.Zero, returnCidAnyway: true);
    Log.Condition(!Tms.IsConnectedSimple(cid1),
                        $"Test connect with incorrect password fail {Tms.GetConnectionErrorText(cid1)}");
    Tms.Disconnect(cid1);

    Tms.SetUserCredentials(username, password);
    var cid2 = Tms.Connect(host, tmServer, CommonUtil.TaskName, null, IntPtr.Zero, returnCidAnyway: true);
    Log.Condition(Tms.IsConnectedSimple(cid2), "Test connect succeed");
    Tms.Disconnect(cid2);

    Tms.SetUserCredentials(username, "totally not a password again");
    var cid3 = Tms.Connect(host, tmServer, CommonUtil.TaskName, null, IntPtr.Zero, returnCidAnyway: true);
    Log.Condition(!Tms.IsConnectedSimple(cid3),
                        $"Test connect with incorrect password fail {Tms.GetConnectionErrorText(cid3)}");
    Tms.Disconnect(cid3);

    Tms.ClearUserCredentials();
  }


  private static void TestTmMethods()
  {
    var (host, tmServer, _, username, password) = CommonUtil.ParseCommandLineArgs();

    Tms.SetUserCredentials(username, password);
    var tmCid = Tms.Connect(host, tmServer, CommonUtil.TaskName, null, IntPtr.Zero);
    Log.Condition(Tms.IsConnectedSimple(tmCid), "TM connect");

    TestTime(tmCid);
    TestUpdateConnection(tmCid);
    TestLinkedRbServer(tmCid, tmServer);
    TestSecurity(tmCid);
    TestUserInfo(tmCid, tmServer);

    Tms.Disconnect(tmCid);
    Tms.ClearUserCredentials();
  }


  private static void TestTime(int tmCid)
  {
    var systemTime = Tms.GetSystemTimeString(tmCid);
    Log.Condition(!string.IsNullOrEmpty(systemTime), $"System time: {systemTime}");
  }


  private static void TestUpdateConnection(int tmCid)
  {
    var oldReconnectCount = Tms.GetReconnectCount(tmCid);
    Tms.UpdateConnection(tmCid);
    var newReconnectCount = Tms.GetReconnectCount(tmCid);
    Log.Condition(oldReconnectCount != newReconnectCount,
                        $"Update connection: {oldReconnectCount} -> {newReconnectCount}");
  }


  private static void TestLinkedRbServer(int tmCid, string tmServer)
  {
    var linkedRbServer = Tms.GetLinkedRbServerName(tmCid, tmServer);
    Log.Condition(!string.IsNullOrEmpty(linkedRbServer), $"Linked RB server: {linkedRbServer}");
  }


  private static void TestSecurity(int tmCid)
  {
    var accessFlags = Tms.GetSecurityAccessFlags(tmCid);
    Log.Condition(accessFlags != TmSecurityAccessFlags.None, $"Security: flags = {accessFlags}");
  }


  private static void TestUserInfo(int tmCid, string tmServer)
  {
    var user = Tms.GetUserInfo(tmCid, tmServer);
    Log.Condition(user != null, $"User data: ID = {user?.Id}, GroupId = {user?.GroupId}, Name = {user?.Name}");
  }


  private static void TestRbMethods()
  {
    var (host, _, rbServer, username, password) = CommonUtil.ParseCommandLineArgs();

    Tms.SetUserCredentials(username, password);
    
    var rbCid = Tms.Connect(host, rbServer, CommonUtil.TaskName, null, IntPtr.Zero);
    Log.Condition(Tms.IsConnectedSimple(rbCid), "RB connect");
    
    var sqlPort = Tms.OpenSqlRedirector(rbCid);
    Log.Condition(sqlPort != 0, $"Open redirector port: {sqlPort}");
    var isCloseOk = Tms.CloseSqlRedirector(rbCid);
    Log.Condition(isCloseOk, "Close redirector");
    
    Tms.Disconnect(rbCid);
    Tms.ClearUserCredentials();
  }
}
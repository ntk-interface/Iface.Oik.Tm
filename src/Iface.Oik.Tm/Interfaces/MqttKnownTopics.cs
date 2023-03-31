using System.ComponentModel;

namespace Iface.Oik.Tm.Interfaces
{
  public enum MqttKnownTopic
  {
    [Description("User/Login")]          UserLogin         = 10,
    [Description("User/Logout")]         UserLogout        = 11,
    [Description("User/CustomMessage")]  UserCustomMessage = 12,

    [Description("Settings/GlobalClient/Edit")]        SettingsGlobalClientEdit        = 50,
    [Description("Settings/UserClient/Edit")]          SettingsUserClientEdit          = 51,
    [Description("Settings/UserEventMonitoring/Edit")] SettingsUserEventMonitoringEdit = 52,

    [Description("Model/Change/All")]                       ModelChangeAll                  = 100,
    [Description("Model/Topology/Change/All")]              TopologyChangeAll               = 110,
    [Description("Model/Topology/Change/Some")]             TopologyChangeSome              = 111,
    [Description("Model/Placard/Add")]                      PlacardAdd                      = 120,
    [Description("Model/Placard/Edit")]                     PlacardEdit                     = 121,
    [Description("Model/Placard/Remove")]                   PlacardRemove                   = 122,
    [Description("Model/Placard/PortableGround/Permit")]    PortableGroundPermit            = 123,
    [Description("Model/Placard/PortableGround/Forbid")]    PortableGroundForbid            = 124,
    [Description("Model/Binding/Add")]                      BindingAdd                      = 140,
    [Description("Model/Binding/ChangeType")]               BindingChangeType               = 141,
    [Description("Model/Binding/ChangeTmAddr")]             BindingChangeTmAddr             = 142,
    [Description("Model/Binding/Edit")]                     BindingEdit                     = 143,
    [Description("Model/Binding/Remove")]                   BindingRemove                   = 145,
    [Description("Model/Binding/RemoveAll")]                BindingRemoveAll                = 146,
    [Description("Model/Binding/AddFromModus")]             BindingAddFromModus             = 147,
    [Description("Model/Binding/RemoveBeforeAddFromModus")] BindingRemoveBeforeAddFromModus = 148,

    [Description("TmEvent/Add")]       TmEventAdd       = 200,
    [Description("TmEvent/Ack")]       TmEventAck       = 201,
    [Description("UserActionLog/Add")] UserActionLogAdd = 202,
    [Description("UserActionLog/Ack")] UserActionLogAck = 203,
    [Description("TmAlert/Add")]       TmAlertAdd       = 250,
    [Description("TmAlert/Remove")]    TmAlertRemove    = 251,
    [Description("TmAlert/Change")]    TmAlertChange    = 252,
    [Description("TmAlert/Ack")]       TmAlertAck       = 253,

    [Description("Document/Create")]          DocumentCreate          = 301,
    [Description("Document/UpdateContent")]   DocumentUpdateContent   = 302,
    [Description("Document/Rename")]          DocumentRename          = 303,
    [Description("Document/UpdateOwnership")] DocumentUpdateOwnership = 304,
    [Description("Document/AddTag")]          DocumentAddTag          = 305,
    [Description("Document/RemoveTag")]       DocumentRemoveTag       = 306,
    [Description("Document/Delete")]          DocumentDelete          = 307,
  }
}
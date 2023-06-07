using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Dto
{
  public class MqttUserCustomMessageDto
  {
    public TmEventImportances Importance { get; set; } = TmEventImportances.Imp0;
    public string             Message    { get; set; }
  }


  public class MqttTopologyChangeSomeDto
  {
    public Dictionary<Guid, CimTopologyStatus> Items { get; set; }
  }


  public class MqttUserActionLogDto
  {
    public Guid                 Id         { get; set; }
    public MqttKnownTopic       Action     { get; set; }
    public TmUserActionCategory Category   { get; set; }
    public DateTime             Time       { get; set; }
    public string               State      { get; set; }
    public int                  Importance { get; set; }
    public string               Text       { get; set; }
    public string               Username   { get; set; }
    public Guid?                ExtraId    { get; set; }
  }


  public class MqttCustomMessageDto
  {
    public int    Importance { get; set; }
    public string Message    { get; set; }
  }


  public class MqttDocumentDto
  {
    public Guid   Id        { get; set; }
    public int    DocType   { get; set; }
    public int    Ownership { get; set; }
    public string Name      { get; set; }
  }


  public class MqttDocumentWithNewNameDto : MqttDocumentDto
  {
    public string NewName { get; set; }
  }


  public class MqttDocumentWithTagDto : MqttDocumentDto
  {
    public string Tag { get; set; }
  }


  public abstract class MqttPlacardDto
  {
    public Guid   Id          { get; set; }
    public int    TypeId      { get; set; }
    public string TypeName    { get; set; }
    public Guid   EquipmentId { get; set; }
  }


  public class MqttPlacardAddedDto : MqttPlacardDto
  {
    public int?   Index   { get; set; }
    public string Comment { get; set; }
  }


  public class MqttPlacardEditedDto : MqttPlacardDto
  {
    public DateTime CreatedTime { get; set; }
    public int?     Index       { get; set; }
    public string   Comment     { get; set; }
    public int?     NewIndex    { get; set; }
    public string   NewComment  { get; set; }
  }


  public class MqttPlacardRemovedDto : MqttPlacardDto
  {
    public DateTime CreatedTime { get; set; }
  }


  public class MqttPermittedPortableGroundDto
  {
    public Guid EquipmentId { get; set; }
    public bool IsPermitted { get; set; }
  }


  public class MqttBindingDto
  {
    public Guid           Id            { get; set; }
    public TobBindingType BindingType   { get; set; }
    public Guid           EquipmentId   { get; set; }
    public string         Name          { get; set; }
    public string         Value         { get; set; }
    public string         NameToDisplay { get; set; }
  }


  public class MqttBindingWithNewTypeDto : MqttBindingDto
  {
    public TobBindingType NewBindingType { get; set; }
  }


  public class MqttBindingWithNewTmAddrDto : MqttBindingDto
  {
    public string NewTmAddrString { get; set; }
  }


  public class MqttBindingWithChangesDto : MqttBindingDto
  {
    public string Changes { get; set; }
  }


  public class MqttBindingFromModusDto
  {
    public int    SchemeId { get; set; }
    public string DocName  { get; set; }
  }
}
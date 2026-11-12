using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Dto
{
    public class MqttRpcRequestDto { }

    public class MqttRpcResponseDto { }

    public enum MqttDtoEditAction
    {
        Unknown = 0,
        Add = 1,
        Edit = 2,
        Remove = 3,
    }

    public class MqttTopologyEquipmentStatusRequestDto : MqttRpcRequestDto
    {
        public List<Guid> EquipmentIds { get; set; }
    }

    public class MqttTopologyEquipmentStatusResponseDto : MqttRpcResponseDto
    {
        public List<CimTopologyStatus> Statuses { get; set; }
    }

    public class MqttTopologyEquipmentTerminalsStatusRequestDto : MqttRpcRequestDto
    {
        public Guid EquipmentId { get; set; }
    }

    public class MqttTopologyEquipmentTerminalsStatusResponseDto : MqttRpcResponseDto
    {
        public List<CimTopologyStatus> Statuses { get; set; }
    }

    public class MqttTopologyTraceRequestDto : MqttRpcRequestDto
    {
        public Guid EquipmentId { get; set; }
        public TmTopologyTraceType Type { get; set; }
    }

    public class MqttTopologyTraceResponseDto : MqttRpcResponseDto
    {
        public List<MqttTopologyTraceDto> Traces { get; set; }
    }

    public class MqttTopologyTraceDto
    {
        public List<Guid> EquipmentIds { get; set; }
    }

    public class MqttTopologyVoltageSourcesResponseDto : MqttRpcResponseDto
    {
        public List<MqttTopologySourceDto> Sources { get; set; } = new();
    }

    public class MqttTopologyGroundSourcesResponseDto : MqttRpcResponseDto
    {
        public List<MqttTopologySourceDto> Sources { get; set; } = new();
    }

    public class MqttTopologySourceDto
    {
        public Guid EquipmentId { get; set; }
        public int AffectedEquipmentCount { get; set; }
        public bool IsSourcePortable { get; set; }
    }

    public class MqttUserCustomMessageDto
    {
        public TmEventImportances Importance { get; set; } = TmEventImportances.Imp0;
        public string Message { get; set; }
    }

    public class MqttTopologyChangeSomeDto
    {
        public Dictionary<Guid, CimTopologyStatus> Items { get; set; }
    }

    public class MqttUserActionLogDto
    {
        public Guid Id { get; set; }
        public MqttKnownTopic Action { get; set; }
        public TmUserActionCategory Category { get; set; }
        public DateTime Time { get; set; }
        public string State { get; set; }
        public int Importance { get; set; }
        public string Text { get; set; }
        public string Username { get; set; }
        public long? Tma { get; set; }
        public Guid? ExtraId { get; set; }
        public int? ExtraInt { get; set; }
    }

    public class MqttEventNotesDto
    {
        public DateTime? EventTime { get; set; }

        public string Comment { get; set; }
        public DateTime? Time { get; set; }
        public Guid? TagId { get; set; }
        public string TagName { get; set; }
    }

    public class MqttTmEventNotesDto : MqttEventNotesDto
    {
        public TmEventElix Elix { get; set; }
    }

    public class MqttUserActionNotesDto : MqttEventNotesDto
    {
        public Guid Id { get; set; }
    }

    public class MqttCustomMessageDto
    {
        public int Importance { get; set; }
        public string Message { get; set; }
    }

    public class MqttDocumentDto
    {
        public Guid Id { get; set; }
        public int DocType { get; set; } // DocumentType
        public int Ownership { get; set; } // DocumentOwnership
        public string Name { get; set; }
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
        public Guid Id { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public Guid EquipmentId { get; set; }
        public string EquipmentName { get; set; }
    }

    public class MqttPlacardAddedDto : MqttPlacardDto
    {
        public int? Index { get; set; }
        public string Comment { get; set; }
    }

    public class MqttPlacardEditedDto : MqttPlacardDto
    {
        public DateTime CreatedTime { get; set; }
        public int? Index { get; set; }
        public string Comment { get; set; }
        public int? NewIndex { get; set; }
        public string NewComment { get; set; }
    }

    public class MqttPlacardRemovedDto : MqttPlacardDto
    {
        public DateTime CreatedTime { get; set; }
    }

    public class MqttPermittedPortableGroundDto
    {
        public Guid EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public bool IsPermitted { get; set; }
    }

    public class MqttBindingDto
    {
        public Guid Id { get; set; }
        public int BindingType { get; set; } // TobBindingType
        public Guid EquipmentId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string NameToDisplay { get; set; }
    }

    public class MqttBindingWithNewTypeDto : MqttBindingDto
    {
        public int NewBindingType { get; set; }
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
        public int SchemeId { get; set; }
        public string DocName { get; set; }
    }

    public class MqttTmTagBlockEvents
    {
        public long TagFullTma { get; set; }
        public string TagName { get; set; }
        public DateTime EndBlockTime { get; set; }
        public string Comment { get; set; }
    }

    public class MqttTmTagSetBackdate
    {
        public long TagFullTma { get; set; }
        public string TagName { get; set; }
        public string ValueToDisplay { get; set; }
        public DateTime Time { get; set; }
    }

    public class MqttOperjRecordDto
    {
        public Guid Id { get; set; }
        public Guid? TemplateId { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Operator { get; set; }
        public string FinalText { get; set; }
        public Guid? SupersededById { get; set; }
    }

    public class MqttOperTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class MqttReferenceDataDto
    {
        public MqttDtoEditAction Action { get; set; }
        public string Name { get; set; }
        public string OldName { get; set; }
    }

    public class MqttReferenceDataWithGuidIdDto : MqttReferenceDataDto
    {
        public Guid Id { get; set; }
    }

    public class MqttReferenceDataWithIntIdDto : MqttReferenceDataDto
    {
        public int Id { get; set; }
        public int? OldId { get; set; }
    }

    public class MqttMapPlacemarkDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LayerName { get; set; }
    }

    public class MqttMapPlacemarksImportedDto
    {
        public int Count { get; set; }
    }

    public class MqttMapPlacemarksRemovedAllDto
    {
        public int Count { get; set; }
    }

    public class MqttMapPlacemarkEditedDto : MqttMapPlacemarkDto
    {
        public string OldName { get; set; }
        public string OldLayerName { get; set; }
        public bool IsReplaced { get; set; } // полная замена метки
    }

    public class MqttMapPlacemarkBindingDto
    {
        public Guid PlacemarkId { get; set; }
        public string PlacemarkName { get; set; }
        public int BindingType { get; set; } // MapPlacemarkBindingType
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class MqttMapPlacemarkBindingEditedDto : MqttMapPlacemarkBindingDto
    {
        public string OldName { get; set; }
        public string OldValue { get; set; }
    }
}

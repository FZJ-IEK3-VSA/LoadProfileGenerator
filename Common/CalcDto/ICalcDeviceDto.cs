using System.Collections.Generic;
using Automation;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public interface ICalcDeviceDto {

        string Name { get; }
        //int ID { get; }
        //int DeviceCategoryID { get; }
        //HouseholdKey HouseholdKey { get; }
        //OefcDeviceType DeviceType { get; }
        //string DeviceCategoryName { get; }
        //string AdditionalName { get; }
        StrGuid Guid { get; }
        //int LocationID { get; }
        //string LocationGuid { get; }
        //string LocationName { get; }
        [ItemNotNull][NotNull]
        List<CalcDeviceLoadDto> Loads { get; }
    }
}
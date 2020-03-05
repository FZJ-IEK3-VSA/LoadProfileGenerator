using System.Collections.Generic;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseElements {
    public class CalcTransformationCondition : CalcBase {
        [CanBeNull] private readonly CalcLoadType _dstLoadType;

        private readonly double _maxValue;
        private readonly double _minValue;

        [CanBeNull] private readonly CalcEnergyStorage _storage;

        private readonly CalcTransformationConditionType _type;

        public CalcTransformationCondition([NotNull] string pName, CalcTransformationConditionType type,
            [CanBeNull] CalcEnergyStorage storage, [CanBeNull] CalcLoadType loadType, double minValue,
            double maxValue, [NotNull] string guid) : base(pName, guid) {
            _type = type;
            _dstLoadType = loadType;
            _minValue = minValue;
            _maxValue = maxValue;
            _storage = storage;
        }

        public bool GetResult([NotNull][ItemNotNull] List<OnlineEnergyFileRow> fileRows) {
            if (fileRows.Count == 0) {
                return false;
            }

            switch (_type) {
                case CalcTransformationConditionType.StorageBetweenValues:
                    if (_storage == null) {
                        throw new LPGException("Storage was null");
                    }
                    if (_storage.PreviousFillLevel >= _minValue * _storage.StorageCapacity &&
                        _storage.PreviousFillLevel <= _maxValue * _storage.StorageCapacity) {
                        return true;
                    }
                    return false;
                case CalcTransformationConditionType.LoadtypeBalanceBetweenValues:
                    foreach (var fileRow in fileRows) {
                        if (fileRow.LoadType == _dstLoadType?.ConvertToDto()) {
                            var sumvalue = fileRow.SumFresh;
                            if (sumvalue > _minValue && sumvalue < _maxValue) {
                                return true;
                            }
                            return false;
                        }
                    }
                    throw new DataIntegrityException("The load type " + _dstLoadType?.Name +
                                                     " was not found in the list of load types for this household. The load type is " +
                                                     " required by one of the signals of the Transformation Device " +
                                                     Name);
                default:
                    throw new LPGException("Forgot a CalcTransformationConditionType. This is a bug. Please report. " +
                                           Name);
            }
        }
    }
}
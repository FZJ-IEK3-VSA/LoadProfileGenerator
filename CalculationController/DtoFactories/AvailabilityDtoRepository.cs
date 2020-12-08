using System;
using System.Collections;
using System.Collections.Generic;
using Automation;
using Common;
using Common.CalcDto;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class AvailabilityDtoRepository
    {
        public class Entry {
            public Entry([JetBrains.Annotations.NotNull] string name, StrGuid guid, [JetBrains.Annotations.NotNull][ItemNotNull] BitArray array)
            {
                Name = name;
                Guid = guid;
                Array = array;
            }

            [JetBrains.Annotations.NotNull]
            public string Name { get; }
            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public BitArray Array { get; }
            public StrGuid Guid { get; }
        }

        [JetBrains.Annotations.NotNull]
        private Dictionary<StrGuid, Entry> Entries { get; } = new Dictionary<StrGuid, Entry>();

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public BitArray GetByGuid(StrGuid guid)
        {
            return Entries[guid].Array;
        }

        [JetBrains.Annotations.NotNull]
        public AvailabilityDataReferenceDto MakeNewReference([JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull][ItemNotNull] BitArray timearray)
        {
            StrGuid guid = Guid.NewGuid().ToStrGuid();
            Entry e = new Entry(name,guid,timearray);
            Entries.Add(e.Guid,e);
            return new AvailabilityDataReferenceDto(name,guid);
        }
    }
}

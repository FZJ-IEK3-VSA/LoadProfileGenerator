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
            public Entry([NotNull] string name, [NotNull] StrGuid guid, [NotNull][ItemNotNull] BitArray array)
            {
                Name = name;
                Guid = guid;
                Array = array;
            }

            [NotNull]
            public string Name { get; }
            [NotNull]
            [ItemNotNull]
            public BitArray Array { get; }
            [NotNull]
            public StrGuid Guid { get; }
        }

        [NotNull]
        private Dictionary<StrGuid, Entry> Entries { get; } = new Dictionary<StrGuid, Entry>();

        [NotNull]
        [ItemNotNull]
        public BitArray GetByGuid([NotNull] StrGuid guid)
        {
            return Entries[guid].Array;
        }

        [NotNull]
        public AvailabilityDataReferenceDto MakeNewReference([NotNull] string name, [NotNull][ItemNotNull] BitArray timearray)
        {
            StrGuid guid = Guid.NewGuid().ToStrGuid();
            Entry e = new Entry(name,guid,timearray);
            Entries.Add(e.Guid,e);
            return new AvailabilityDataReferenceDto(name,guid);
        }
    }
}

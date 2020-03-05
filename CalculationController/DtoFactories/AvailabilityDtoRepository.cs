using System;
using System.Collections;
using System.Collections.Generic;
using Common.CalcDto;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class AvailabilityDtoRepository
    {
        public class Entry {
            public Entry([NotNull] string name, [NotNull] string guid, [NotNull][ItemNotNull] BitArray array)
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
            public string Guid { get; }
        }

        [NotNull]
        private Dictionary<string, Entry> Entries { get; } = new Dictionary<string, Entry>();

        [NotNull]
        [ItemNotNull]
        public BitArray GetByGuid([NotNull] string guid)
        {
            return Entries[guid].Array;
        }

        [NotNull]
        public AvailabilityDataReferenceDto MakeNewReference([NotNull] string name, [NotNull][ItemNotNull] BitArray timearray)
        {
            string guid = Guid.NewGuid().ToString();
            Entry e = new Entry(name,guid,timearray);
            Entries.Add(e.Guid,e);
            return new AvailabilityDataReferenceDto(name,guid);
        }
    }
}

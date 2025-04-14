using System;
using System.Collections;
using System.Collections.Generic;
using Automation;
using Common.CalcDto;
using Common.Extensions;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class AvailabilityDtoRepository
    {
        public class Entry {
            public Entry([NotNull] string name, StrGuid guid, [NotNull][ItemNotNull] BitArray array)
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

        /// <summary>
        /// Returns the matching BitArray for the guid. The same as GetByGuid, but also
        /// allows null as parameter, in which case null is returned. Useful when TimeLimits
        /// are optional.
        /// </summary>
        /// <param name="guid">the guid of the availability entry</param>
        /// <returns>the BitArray of the matching availability entry</returns>
        public BitArray? GetByGuidOptional([NotNull] StrGuid? guid)
        {
            if (guid is null)
                return null;
            return GetByGuid(guid);
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

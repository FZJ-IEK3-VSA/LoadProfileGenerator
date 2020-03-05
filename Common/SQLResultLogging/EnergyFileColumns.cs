using System.Collections.Generic;
using System.Linq;
using System.Text;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

namespace Common.SQLResultLogging
{
    public class EnergyFileColumns
    {
        [NotNull]
        public HouseholdKey Key { get; }
        [NotNull] private readonly CalcParameters _calcParameters;
        [ItemNotNull] [NotNull] private readonly List<ColumnEntry> _columnEntries;
        public EnergyFileColumns([NotNull] SqlResultLoggingService srls, [NotNull] HouseholdKey key, [NotNull] CalcParameters calcParameters)
        {
            Key = key;
            _calcParameters = calcParameters;
            ColumnEntryLogger cel = new ColumnEntryLogger(srls);
            _columnEntries = cel.Read(key);
            foreach (ColumnEntry entry in _columnEntries) {
                if(entry.HouseholdKey!= key && key != Constants.GeneralHouseholdKey ) {
                    continue;
                }

                if (!_columnEntriesByColumn.ContainsKey(entry.LoadType)) {
                    _columnEntriesByColumn.Add(entry.LoadType, new Dictionary<int, ColumnEntry>());
                    ColumnCountByLoadType.Add(entry.LoadType,0);
                }
                _columnEntriesByColumn[entry.LoadType].Add(entry.Column,entry);
                ColumnCountByLoadType[entry.LoadType]++;
            }
        }
        [NotNull]
        public string GetTotalHeaderString([NotNull] CalcLoadTypeDto lt, [CanBeNull] List<int> usedColumns)
        {
            var headerstring = new StringBuilder();
            var columns = _columnEntries.Where(x=> x.LoadType == lt);
            if (usedColumns == null || usedColumns.Count == 0)
            {
                foreach (var entry in columns)
                {
                    headerstring.Append(entry.HeaderString(lt, _calcParameters));
                }
            }
            else
            {
                foreach (var entry in columns)
                {
                    if (usedColumns.Contains(entry.Column))
                    {
                        headerstring.Append(entry.HeaderString(lt, _calcParameters));
                    }
                }
            }
            if(headerstring[headerstring.Length-1] == _calcParameters.CSVCharacter[0]) {
                headerstring.Length--;
            }
            return headerstring.ToString();
        }

        [NotNull]
        private readonly Dictionary<CalcLoadTypeDto, Dictionary<int, ColumnEntry>> _columnEntriesByColumn = new Dictionary<CalcLoadTypeDto, Dictionary<int, ColumnEntry>>();
        [NotNull]
        public Dictionary<CalcLoadTypeDto, Dictionary<int, ColumnEntry>> ColumnEntriesByColumn => _columnEntriesByColumn;
        [NotNull]
        public Dictionary<CalcLoadTypeDto, int> ColumnCountByLoadType { get; } = new Dictionary<CalcLoadTypeDto, int>();
    }
}

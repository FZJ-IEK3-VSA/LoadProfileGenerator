using System.Collections.Generic;
using Automation;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables

{
    public interface IImportFromOtherItem
    {
        [NotNull]
        DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim);
    }
    public abstract class DBBaseElement : DBBase, IImportFromOtherItem
    {
        [NotNull]
        public abstract DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim);
        [ItemNotNull]
        [NotNull]
        public abstract List<UsedIn> CalculateUsedIns([NotNull] Simulator sim);

        protected DBBaseElement([NotNull] string pName, [NotNull] string tableName, [NotNull] string connectionString, [NotNull] StrGuid guid) : base(pName, tableName, connectionString, guid)
        {
        }

        protected DBBaseElement([NotNull]string pName,[CanBeNull] int? pID, [NotNull] string tableName, [NotNull] string connectionString, [NotNull] StrGuid guid) : base(pName,pID, tableName, connectionString, guid)
        {
        }
        /*
                protected DBBaseElement(string pName, int? pID, string tableName, string connectionString) : base(pName, pID, tableName, connectionString)
                {
                }*/
    }
}

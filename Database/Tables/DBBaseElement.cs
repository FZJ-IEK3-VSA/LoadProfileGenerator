using System.Collections.Generic;
using Automation;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables

{
    public interface IImportFromOtherItem
    {
        [JetBrains.Annotations.NotNull]
        DBBase ImportFromGenericItem([JetBrains.Annotations.NotNull] DBBase toImport, [JetBrains.Annotations.NotNull] Simulator dstSim);
    }
    public abstract class DBBaseElement : DBBase, IImportFromOtherItem
    {
        public abstract DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim);
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public abstract List<UsedIn> CalculateUsedIns([JetBrains.Annotations.NotNull] Simulator sim);

        protected DBBaseElement([JetBrains.Annotations.NotNull] string pName, [JetBrains.Annotations.NotNull] string tableName, [JetBrains.Annotations.NotNull] string connectionString,[NotNull]  StrGuid guid) : base(pName, tableName, connectionString, guid)
        {
        }

        protected DBBaseElement([JetBrains.Annotations.NotNull]string pName,[CanBeNull] int? pID, [JetBrains.Annotations.NotNull] string tableName, [JetBrains.Annotations.NotNull] string connectionString, [NotNull] StrGuid guid) : base(pName,pID, tableName, connectionString, guid)
        {
        }
        /*
                protected DBBaseElement(string pName, int? pID, string tableName, string connectionString) : base(pName, pID, tableName, connectionString)
                {
                }*/
    }
}

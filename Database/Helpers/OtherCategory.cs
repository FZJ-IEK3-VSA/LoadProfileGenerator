using System.Collections.Generic;
using Common;
using Database.Tables;
using JetBrains.Annotations;

namespace Database.Helpers {
    public class OtherCategory : BasicCategory {
        public OtherCategory([NotNull] string pName) : base(pName) => LoadingNumber = 0;

        public override List<DBBase> CollectAllDBBaseItems() => throw new LPGNotImplementedException();
    }
}
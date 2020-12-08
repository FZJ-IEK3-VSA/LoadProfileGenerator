using System.Collections.Generic;
using Common;
using Database.Tables;

namespace Database.Helpers {
    public class OtherCategory : BasicCategory {
        public OtherCategory([JetBrains.Annotations.NotNull] string pName) : base(pName) => LoadingNumber = 0;

        public override List<DBBase> CollectAllDBBaseItems() => throw new LPGNotImplementedException();
    }
}
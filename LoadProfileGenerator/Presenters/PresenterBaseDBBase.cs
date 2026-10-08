using System.Diagnostics.CodeAnalysis;
using Database.Tables;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Presenters {
    public abstract class PresenterBaseDBBase<T> : PresenterBaseWithAppPresenter<T> where T : class {
        [JetBrains.Annotations.NotNull] private readonly ApplicationPresenter _applicationPresenter;

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        protected PresenterBaseDBBase([JetBrains.Annotations.NotNull] T view, [JetBrains.Annotations.NotNull] string tabHeaderPath, [JetBrains.Annotations.NotNull] DBBase item,
            [JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter) : base(view, tabHeaderPath, applicationPresenter) {
            Item = item;
            _applicationPresenter = applicationPresenter;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public DBBase Item { get; }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string ItemName => Item.TypeDescription;

        public override void Close(bool saveToDB, bool removeLast = false) {
            if (saveToDB) {
                Item.SaveToDB();
            }
            _applicationPresenter.CloseTab(this, removeLast);
        }
    }
}
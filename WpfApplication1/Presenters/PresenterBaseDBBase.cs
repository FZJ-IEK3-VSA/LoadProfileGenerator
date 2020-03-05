using System.Diagnostics.CodeAnalysis;
using Database.Tables;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Presenters {
    public abstract class PresenterBaseDBBase<T> : PresenterBaseWithAppPresenter<T> where T : class {
        [NotNull] private readonly ApplicationPresenter _applicationPresenter;

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        protected PresenterBaseDBBase([NotNull] T view, [NotNull] string tabHeaderPath, [NotNull] DBBase item,
            [NotNull] ApplicationPresenter applicationPresenter) : base(view, tabHeaderPath, applicationPresenter) {
            Item = item;
            _applicationPresenter = applicationPresenter;
        }

        [NotNull]
        [UsedImplicitly]
        public DBBase Item { get; }

        [NotNull]
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
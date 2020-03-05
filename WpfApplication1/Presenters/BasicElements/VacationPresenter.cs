using System.Collections.Generic;
using Common.Enums;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Views.BasicElements;

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class VacationPresenter : PresenterBaseDBBase<VacationView> {
        [NotNull] private readonly ApplicationPresenter _applicationPresenter;
        [NotNull] private readonly Vacation _thisVacation;

        public VacationPresenter([NotNull] ApplicationPresenter applicationPresenter,
                                 [NotNull] VacationView view,
                                 [NotNull] Vacation vacation)
            : base(view, "ThisVacation.HeaderString", vacation, applicationPresenter)
        {
            _applicationPresenter = applicationPresenter;
            _thisVacation = vacation;
        }
        [NotNull]
        [UsedImplicitly]

        public Dictionary<CreationType, string> CreationTypes => CreationTypeHelper.CreationTypeDictionary;

        [UsedImplicitly]
        public VacationType SelectedVacationType { get; set; }

        [NotNull]
        public Vacation ThisVacation => _thisVacation;
        [NotNull]
        [UsedImplicitly]
        public Dictionary<VacationType, string> VacationTypes => VacationTypeHelper.VacationTypeDictionary;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _thisVacation.SaveToDB();
            }
            _applicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete()
        {
            Sim.Vacations.DeleteItem(_thisVacation);
            Close(false);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            return obj is VacationPresenter presenter && presenter.ThisVacation.Equals(_thisVacation);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                const int hash = 17;
                // Suitable nullity checks etc, of course :)
                return hash * 23 + TabHeaderPath.GetHashCode();
            }
        }
    }
}
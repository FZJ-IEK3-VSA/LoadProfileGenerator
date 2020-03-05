using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik für HouseholdGenerator.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class HouseholdTemplateView {
        public HouseholdTemplateView()
        {
            InitializeComponent();
        }

        [NotNull]
        private HouseholdTemplatePresenter Presenter => (HouseholdTemplatePresenter) DataContext;

        private void BtnAddAllVacation_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            foreach (var allVacation in Presenter.AllVacations) {
                Presenter.ThisTemplate.AddVacation(allVacation);
            }
        }

        private void BtnAddEntry_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedTag == null) {
                return;
            }

            if (Presenter.MaxCount == 0) {
                return;
            }

            if (!Presenter.Persons.Any(x => x.IsChecked)) {
                return;
            }

            var p = Presenter.Persons.Where(x => x.IsChecked).Select(x => x.Person).ToList();
            Presenter.AddEntry(Presenter.SelectedTag, Presenter.MinCount, Presenter.MaxCount, p);
            Presenter.RefreshFilteredEntries(Presenter.FilterText);
        }

        private void BtnAddPerson_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedPerson == null) {
                return;
            }

            TraitTag tag = null;
            if (CmbLivingPatterns.SelectedItem != null) {
                tag = (TraitTag) CmbLivingPatterns.SelectedItem;
            }

            Presenter.ThisTemplate.AddPerson(Presenter.SelectedPerson, tag);
            Presenter.RefreshPersons();
        }

        private void BtnAddTemplateTag_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedHouseholdTag == null) {
                return;
            }

            Presenter.ThisTemplate.AddTemplateTag(Presenter.SelectedHouseholdTag);
            LstTemplateTags.ResizeColummns();
        }

        private void BtnAddVacation_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedVacation == null) {
                return;
            }

            Presenter.ThisTemplate.AddVacation(Presenter.SelectedVacation);
        }

        private void BtnCreateNewTag_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var tag =
                Presenter.Sim.HouseholdTags.CreateNewItem(
                    Presenter.Sim.ConnectionString);
            Presenter.ThisTemplate.AddTemplateTag(tag);
            Presenter.ApplicationPresenter.OpenItem(tag);
            LstTemplateTags.ResizeColummns();
        }

        private void BtnRemoveAllEntry_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var count = Presenter.ThisTemplate.Entries.Count;
            var mbr = MessageWindows.ShowYesNoMessage(
                "Are you really sure you want to delete these " + count + " entries?", "Delete?");
            if (mbr == MessageBoxResult.Yes) {
                Presenter.DeleteAllEntries();
            }

            Presenter.RefreshFilteredEntries(Presenter.FilterText);
        }

        private void BtnRemoveEntry_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstEntries.SelectedItem == null) {
                return;
            }

            var entry = (HHTemplateEntry) LstEntries.SelectedItem;
            Presenter.ThisTemplate.DeleteEntryFromDB(entry);
            Presenter.RefreshFilteredEntries(Presenter.FilterText);
        }

        private void BtnRemovePerson_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstPersons.SelectedItem == null) {
                return;
            }

            var hhTemplatePerson = (HHTemplatePerson) LstPersons.SelectedItem;
            Presenter.ThisTemplate.DeletePersonFromDB(hhTemplatePerson);
            Presenter.RefreshPersons();
        }

        private void BtnRemoveTag_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstTemplateTags.SelectedItem == null) {
                return;
            }

            Presenter.ThisTemplate.DeleteTagFromDB((HHTemplateTag) LstTemplateTags.SelectedItem);
            LstTemplateTags.ResizeColummns();
        }

        private void BtnRemoveVacation_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstVacations.SelectedItem == null) {
                return;
            }

            var hhTemplateVacation = (HHTemplateVacation) LstVacations.SelectedItem;
            Presenter.ThisTemplate.DeleteVacationFromDB(hhTemplateVacation);
        }

        private void BtnRemoveWrongAge_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var householdTemplate = Presenter.ThisTemplate;
            if (!householdTemplate.Persons.Any()) {
                Logger.Warning("No persons were defined, and thus noone can be removed.");
                return;
            }

            var ages = householdTemplate.Persons.Select(x => x.Person.Age).ToList();

            var minAge = ages.Min();
            var maxAge = ages.Max();
            var items2Delete = new List<HHTemplateVacation>();
            foreach (var templateVacation in householdTemplate.Vacations) {
                if (templateVacation.Vacation.MinimumAge >= minAge || templateVacation.Vacation.MaximumAge <= maxAge) {
                    items2Delete.Add(templateVacation);
                }
            }

            foreach (var hhTemplateVacation in items2Delete) {
                householdTemplate.DeleteVacationFromDB(hhTemplateVacation);
            }
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisTemplate.HeaderString, Presenter.Delete);

        private void DeleteAllClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var count = Presenter.GeneratedHouseholds.Count;
            var mbr =
                MessageWindows.ShowYesNoMessage(
                    "Are you really sure you want to delete these " + count + " households?", "Delete?");
            if (mbr == MessageBoxResult.Yes) {
                Presenter.DeleteAllHouseholds();
            }
        }

        private void Generate_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (ChkSettlementCreation.IsChecked == null) {
                ChkSettlementCreation.IsChecked = true;
            }

            Presenter.GenerateHouseholds((bool) ChkSettlementCreation.IsChecked);
        }

        private void ImportClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedCHH == null) {
                return;
            }

            Presenter.ImportFromCHH();
            Presenter.RefreshFilteredEntries(TxtFilter.Text);
        }

        private void LstEntries_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var entry = (HHTemplateEntry) LstEntries.SelectedItem;
            if (entry?.TraitTag != null) {
                Presenter.ApplicationPresenter.OpenItem(entry.TraitTag);
            }
        }

        private void LstGeneratedHouseholds_OnMouseDoubleClick([CanBeNull] object sender,
            [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstGeneratedHouseholds.SelectedItem == null) {
                return;
            }

            var mhh = (ModularHousehold) LstGeneratedHouseholds.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(mhh);
        }

        private void LstTemplate_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstTemplateTags.SelectedItem == null) {
                return;
            }

            Presenter.SelectedHouseholdTag = ((HHTemplateTag) LstTemplateTags.SelectedItem).Tag;
        }

        private void LstTemplateTags_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstTemplateTags.SelectedItem == null) {
                return;
            }

            var hht = (HHTemplateTag) LstTemplateTags.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(hht.Tag);
            LstTemplateTags.ResizeColummns();
        }

        private void LstVacationsOnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstVacations.SelectedItem == null) {
                return;
            }

            var hhTemplateVacation = (HHTemplateVacation) LstVacations.SelectedItem;
            Presenter.SelectedVacation = hhTemplateVacation.Vacation;
        }

        private void RefreshHouseholdsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshHouseholds();

        private void Selector_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstPersons.SelectedItem == null) {
                return;
            }

            var hhTemplatePerson = (HHTemplatePerson) LstPersons.SelectedItem;
            Presenter.SelectedPerson = hhTemplatePerson.Person;
        }

        private void UIElement_OnKeyUp([NotNull] object sender, [NotNull] KeyEventArgs e) =>
            Presenter.RefreshFilteredEntries(TxtFilter.Text);
    }
}
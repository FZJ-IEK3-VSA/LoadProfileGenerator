//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Common;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace LoadProfileGenerator.Views.Households {
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class ModularHouseholdView {
        public ModularHouseholdView()
        {
            InitializeComponent();
        }

        [NotNull]
        private ModularHouseholdPresenter Presenter => (ModularHouseholdPresenter) DataContext;

        private void Apply([NotNull] string s, [CanBeNull] Person p = null)
        {
            Presenter.FilterText = s;
            Presenter.UseFilter = true;
            Presenter.FilterTraits(s, (TraitTag) CmbFilterTag.SelectedItem);
            if (p != null) {
                Presenter.SelectedPerson = p;
            }
        }

        private void ApplyTag([NotNull] string s, [CanBeNull] Person p = null)
        {
            var tag = Presenter.Sim.TraitTags.FindByName(s, FindMode.Partial);
            Presenter.UseTags = true;
            if (tag != null) {
                Presenter.SelectedFilterTag = tag;
            }

            Presenter.FilterTraits(TxtFilter.Text, (TraitTag) CmbFilterTag.SelectedItem);
            if (p != null) {
                Presenter.SelectedPerson = p;
            }
        }

        private void BtnAddHouseholdTag_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedHouseholdTag == null) {
                return;
            }

            Presenter.ThisModularHousehold.AddHouseholdTag(Presenter.SelectedHouseholdTag);
            LstTags.ResizeColummns();
        }

        private void BtnAddPersonClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbChhPersons.SelectedItem == null) {
                Logger.Error("Please first select a person.");
                return;
            }

            var p = (Person) CmbChhPersons.SelectedItem;
            var tag = (TraitTag) CmbLivingPatterns.SelectedItem;
            Presenter.AddPerson(p, tag);
        }

        private void BtnAddTrait_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbTraits.SelectedItem == null) {
                return;
            }

            var hht = (HouseholdTrait) CmbTraits.SelectedItem;
            var type =
                (ModularHouseholdTrait.ModularHouseholdTraitAssignType) CmbAssignType.SelectedItem;
            Person p = null;
            if (CmbPersons.SelectedItem != null) {
                p = (Person) CmbPersons.SelectedItem;
            }

            if (type == ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name && p == null) {
                Logger.Error("Please select a person first!");
                return;
            }

            Presenter.AddHouseholdTrait(hht, type, p);
            CmbPersons.SelectedItem = p;
            CmbAssignType.SelectedItem = type;
            CmbTraits.SelectedItem = hht;

            Logger.Info("Added new Trait to " + p?.Name + ": " + hht.Name);
        }

        private void BtnCreateNewTag_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var tag =
                Presenter.Sim.HouseholdTags.CreateNewItem(Presenter.Sim.ConnectionString);
            Presenter.ThisModularHousehold.AddHouseholdTag(tag);
            Presenter.ApplicationPresenter.OpenItem(tag);
            LstTags.ResizeColummns();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("ReSharper", "ReplaceWithSingleAssignment.True")]
        private void BtnNextRequirement_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var chh = Presenter.ThisModularHousehold;
            var laundry = chh.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("LAUNDRY"));
            if (!laundry) {
                Apply("Laundry");
                return;
            }

            var dishwashing = chh.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("DISHWASH"));
            if (!dishwashing) {
                Apply("dishwash");
                return;
            }

            var cleanBathroom = chh.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("CLEAN BATHROOM"));
            if (!cleanBathroom) {
                Apply("bathroom");
                return;
            }

            var foodshopping = chh.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("FOOD SHOPPING"));
            if (!foodshopping) {
                Apply("food");
                return;
            }

            var vacuum = chh.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("VACUUM"));
            if (!vacuum) {
                Apply("vacuum");
                return;
            }

            foreach (var modularHouseholdPerson in chh.Persons) {
                var requireLiving = true;
                if (modularHouseholdPerson.Person.Description.ToUpperInvariant() == "MAID") {
                    requireLiving = false;
                }

                var traits = chh.Traits.Where(x => x.DstPerson == modularHouseholdPerson.Person)
                    .ToList();
                var shower = traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("SHOWER"));
                if (requireLiving && !shower && modularHouseholdPerson.Person.Age > 10) {
                    Apply("shower", modularHouseholdPerson.Person);
                    return;
                }

                var sleep = traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("SLEEP"));
                if (requireLiving && !sleep) {
                    Apply("sleep", modularHouseholdPerson.Person);
                    return;
                }

                var cooking = traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("COOKING"));
                var unhungry = traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("UNHUNGRY"));
                if (requireLiving && !cooking && !unhungry) {
                    Apply("unhungry", modularHouseholdPerson.Person);
                    return;
                }

                var sickness = traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("SICKNESS"));
                if (requireLiving && !sickness) {
                    Apply("sickness", modularHouseholdPerson.Person);
                    return;
                }

                var toilet = traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("TOILET"));
                if (requireLiving && !toilet) {
                    Apply("toilet", modularHouseholdPerson.Person);
                    return;
                }

                var getready = traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("READY"));
                if (requireLiving && !getready) {
                    Apply("get ready", modularHouseholdPerson.Person);
                    return;
                }

                var work = traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("WORK"));
                var school = traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("SCHOOL"));
                var outsideafternoonTraits =
                    traits.Where(x => x.HouseholdTrait.Tags.Any(y => y.Name.ToUpperInvariant()
                        .Contains("OUTSIDE AFTERNOON ENTERTAINMENT"))).ToList();

                if (requireLiving && outsideafternoonTraits.Count == 0 && !work && !school &&
                    modularHouseholdPerson.Person.Age > 10) {
                    ApplyTag("afternoon entertain", modularHouseholdPerson.Person);
                    return;
                }

                var outsideEveningTraits =
                    traits.Where(x => x.HouseholdTrait.Tags.Any(y => y.Name.ToUpperInvariant()
                        .Contains("OUTSIDE EVENING ENTERTAINMENT"))).ToList();

                if (requireLiving && outsideEveningTraits.Count == 0 && modularHouseholdPerson.Person.Age > 15) {
                    ApplyTag("evening entertain", modularHouseholdPerson.Person);
                    return;
                }

                var hobby = traits
                    .Where(x => x.HouseholdTrait.Tags.Any(y => y.Name.ToUpperInvariant().Contains("HOBBY"))).ToList();

                if (requireLiving && hobby.Count == 0 && modularHouseholdPerson.Person.Age > 15) {
                    ApplyTag("hobby", modularHouseholdPerson.Person);
                    return;
                }
            }

            Logger.Info("No more requirements missing!");
        }

        private void BtnRefreshHouseholds_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.RefreshUsedIn();
            LstUsedIn.ResizeColummns();
        }

        private void BtnRemovePersonClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstChhPersons.SelectedItem == null) {
                Logger.Info("Please select a person first.");
                return;
            }

            var chp = (ModularHouseholdPerson) LstChhPersons.SelectedItem;
            var s = "Are you sure you want to delete the person" + Environment.NewLine + chp.Person.PrettyName +
                    Environment.NewLine + "from this household?";
            var dr = MessageWindowHandler.Mw.ShowYesNoMessage(s, "Delete?");
            if (dr == LPGMsgBoxResult.Yes) {
                Presenter.RemovePerson(chp);
            }
        }

        private void BtnRemoveTag_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstTags.SelectedItem == null) {
                return;
            }

            Presenter.ThisModularHousehold.DeleteTag((ModularHouseholdTag) LstTags.SelectedItem);
            LstTags.ResizeColummns();
        }

        private void BtnRemoveTrait_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstHouseholdTraits.SelectedItem == null) {
                return;
            }

            var hht = (ModularHouseholdTrait) LstHouseholdTraits.SelectedItem;
            var name = hht.Name;
            Logger.Info("Deleted " + hht.Name + " from " + name);
            Presenter.RemoveHouseholdTrait(hht);
        }

        private void BtnSwapPersonClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstChhPersons.SelectedItem == null) {
                return;
            }

            if (CmbChhPersons.SelectedItem == null) {
                return;
            }

            var srcPerson = (ModularHouseholdPerson) LstChhPersons.SelectedItem;
            var dstPerson = (Person) CmbChhPersons.SelectedItem;
            var traitTag = (TraitTag) CmbLivingPatterns.SelectedItem;
            Presenter.SwapPersons(srcPerson, dstPerson, traitTag);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void CmbFilterTag_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            Presenter.UseFilter = false;
            Presenter.UseTags = true;
            Presenter.FilterTraits(TxtFilter.Text, (TraitTag) CmbFilterTag.SelectedItem);
        }

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisModularHousehold.HeaderString, Presenter.Delete);

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ExportToCsvClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            using (var sfd = new SaveFileDialog()) {
                sfd.OverwritePrompt = true;
                sfd.Filter = "CSV file (*.csv)|*.csv";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.AddExtension = true;
                sfd.DefaultExt = ".csv";
                sfd.Title = "Please choose the path to save the exported CSV file.";
                sfd.CheckPathExists = true;
                if (sfd.ShowDialog() == DialogResult.OK) {
                    try {
                        ModularHouseholdSerializer.ExportAsCSV(Presenter.ThisModularHousehold,
                            Presenter.Sim, sfd.FileName);
                        Logger.Info("Finished writing the file.");
                        MessageWindowHandler.Mw.ShowInfoMessage("Finished exporting to " + sfd.FileName, "Finished Export");
                    }
                    catch (Exception ex) {
                        Logger.Exception(ex);
                        MessageWindowHandler.Mw.ShowDebugMessage(ex);
                    }
                }
            }
        }

        private void HHPersons_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstChhPersons.SelectedItem == null) {
                return;
            }

            var chp = (ModularHouseholdPerson) LstChhPersons.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(chp.Person);
        }

        private void LstHouseholdTraits_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (Presenter.SelectedChTrait == null) {
                return;
            }

            Presenter.ApplicationPresenter.OpenItem(Presenter.SelectedTrait);
        }

        private void LstHouseholdTraits_OnSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstHouseholdTraits.SelectedItem == null) {
                return;
            }

            var trait = (ModularHouseholdTrait) LstHouseholdTraits.SelectedItem;
            Presenter.SelectedAssigningType = trait.AssignType;
            Presenter.SelectedPerson = trait.DstPerson;
            Presenter.SelectedTrait = trait.HouseholdTrait;
            Presenter.SelectedChTrait = trait;
        }

        private void LstTags_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstTags.SelectedItem == null) {
                return;
            }

            var hht = (ModularHouseholdTag) LstTags.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(hht.Tag);
            LstTags.ResizeColummns();
        }

        private void LstTags_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstTags.SelectedItem == null) {
                return;
            }

            Presenter.SelectedHouseholdTag = ((ModularHouseholdTag) LstTags.SelectedItem).Tag;
        }

        private void LstUsedByMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var ui = LstUsedIn.SelectedItem as UsedIn;
            if (ui?.Item != null) {
                Presenter.ApplicationPresenter.OpenItem(ui.Item);
            }
        }

        private void MakeTraitCopyOnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.CreateNewModularHousehold();

        private void RefreshTimeEstimatesClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.RefreshTimeEstimates();
        }

        private void TxtFilter_OnKeyUp([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Presenter.UseFilter = true;
            Presenter.UseTags = false;
            Presenter.FilterTraits(TxtFilter.Text, (TraitTag) CmbFilterTag.SelectedItem);
        }
    }
}
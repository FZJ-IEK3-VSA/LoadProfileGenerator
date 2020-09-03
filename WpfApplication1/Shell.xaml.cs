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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Automation.ResultFiles;
using CalculationController.Integrity;
using Common;
using Database;
using Database.Database;
using Database.Helpers;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters;
using LoadProfileGenerator.Presenters.BasicElements;
using Microsoft.Win32;

namespace LoadProfileGenerator {
    /// <summary>
    ///     Interaktionslogik f�r MainWindow.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class Shell : IWindowWithDialog {
        // CommonDataWPF.WindowWithDialog
        private const string LastFileFileName = "LastDatabase.txt";

        [NotNull] private readonly string _version;

        [NotNull] private string _connectionString;


        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void SafeExecuteWithWait([NotNull] Action action)
        {
            if (Dispatcher != null && Thread.CurrentThread != Dispatcher.Thread)
            {
                var finished = false;

                void CallActionWithErrorHandling()
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception(ex);
                    }
                    finished = true;
                }

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)CallActionWithErrorHandling);
                while (!finished)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                action();
            }
        }
        public void SafeExecuteForLogger([NotNull] Action a)
        {
            if (Dispatcher != null && Thread.CurrentThread != Dispatcher.Thread)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, a);
            }
            else
            {
                a();
            }
        }
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
        public Shell()
        {
            Logger.Get().SaveExecutionFunction = SafeExecuteForLogger;
            Logger.Get().SaveExecutionFunctionWithWait = SafeExecuteWithWait;
             try {
                Logger.Debug("Initializing BindingErrorListener");
                BindingErrorListener.Listen(m => {
                    if (!m.Contains("FindAncestor")) {
                        var s = m;
                        Logger.Error(s);
                        var t = new Thread(() => MessageBox.Show(s));
                        t.Start();
                    }
                });
            }
            catch (Exception e) {
                Logger.Error(e.Message);
                MessageWindowHandler.Mw.ShowDebugMessage(e);
                Logger.Exception(e);
            }

            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
            DataContext = new ApplicationPresenter(this, Sim, this.Dispatcher);
            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Config.LPGVersion = _version;
            MessageWindowHandler.SetMainWindow(new WpfMsgWindows(this));
            FindDefaultDB3File();

            if (!string.IsNullOrEmpty(DB3Path) && (DB3Path.ToUpperInvariant().Contains("APPDATA") ||
                                                   (!DB3Path.Contains("\\") && Environment.CurrentDirectory
                                                       .ToUpperInvariant().Contains("APPDATA")))) {
                Config.WarningString = "It seems you are storing your database in the appdata folder. " +
                                       "This happens automatically if you installed by setup. " +
                                       "To avoid loosing all changes on an upgrade, it it strongly recommended " +
                                       "to save your database elsewhere. To do that just select \"save as\" from the menu, " +
                                       "choose a location and save the database there.";
                Presenter.WelcomePresenter?.Refresh();
            }
            else {
                Config.WarningString = string.Empty;
            }

            UpdateTitle();
            if (!string.IsNullOrWhiteSpace(DB3Path)) {
                OpenDatabase(DB3Path);
            }
            else {
                try {
                    Logger.Error("No database was found. If nothing else works, please" +
                                 " download the zip file from the website and use " + "the db3-file from there!");
                }
                catch (Exception ex) {
                    Logger.Error(ex.Message);
                    Logger.Exception(ex);
                }
            }

            DotNetVersionCheck.CheckForVersion();
            var uc = new UpdateChecker();
            uc.GetLatestVersion(out var question);
            if (question != null) {
                MessageWindowHandler.Mw.ShowInfoMessage(question, "Update recommended!");
            }
        }

        [CanBeNull]
        public static string DB3Path { get; private set; }

        [NotNull]
        private ApplicationPresenter Presenter => (ApplicationPresenter) DataContext;

        [CanBeNull]
        private static Simulator Sim { get; set; }

        //helper function for the MessageWindowHandler.Mw class to get around the dispatcher issue
        public MessageBoxResult ShowMessageWindow(string txt, string caption,
            MessageBoxButton button,
            MessageBoxImage image, MessageBoxResult defaultresult)
        {
            var result = MessageBoxResult.Yes;
            if (Dispatcher == null) {
                throw new Exception("Dispatcher was null");
            }
            if (txt.Length < 500) {
                if (Dispatcher.CheckAccess()) {
                    result = MessageBox.Show(this, txt, caption, button, image, defaultresult);
                }
                else {
                    Dispatcher.Invoke(DispatcherPriority.Normal,
                        new Action(() => result = MessageBox.Show(this, txt, caption, button, image, defaultresult)));
                }
            }
            else {
                if (Dispatcher.CheckAccess()) {
                    var sew = new ScrollingErrorWindow(txt);
                    sew.ShowDialog();
                    result = MessageBoxResult.OK;
                }

                // Invokation required
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                    result = MessageBoxResult.OK;
                    var sew = new ScrollingErrorWindow(txt);
                    sew.ShowDialog();
                }));
            }

            return result;
        }

        public void AddTab<T>([NotNull] PresenterBase<T> presenter) where T : class
        {
            TabItem newTab = null;

            for (var i = 0; i < Tabs.Items.Count; i++) {
                var existingTab = (TabItem) Tabs.Items[i];

                if (existingTab.DataContext.Equals(presenter)) {
                    Tabs.Items.Remove(existingTab);
                    newTab = existingTab;
                    break;
                }
            }

            if (newTab == null) {
                newTab = new TabItem();

                var headerBinding = new Binding(presenter.TabHeaderPath);
                BindingOperations.SetBinding(newTab, HeaderedContentControl.HeaderProperty, headerBinding);
                newTab.DataContext = presenter;
                newTab.Content = presenter.View;
            }

            Tabs.Items.Insert(0, newTab);
            Tabs.SelectedIndex = 0;
        }

        public void CloseDB()
        {
            CloseAllTabs();
            DB3Path = string.Empty;
            UpdateTitle();
            Sim = null;
            //Presenter.Simulator = null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void OpenDatabase([NotNull] string fileName)
        {
            if (!File.Exists(fileName)) {
                return;
            }

            CloseDB();
            DB3Path = fileName;
            UpdateTitle();
            try {
                using (var sw = new StreamWriter(LastFileFileName)) {
                    sw.WriteLine(fileName);
                    sw.Close();
                }
            }
            catch (Exception e) {
                Logger.Error("Error while opening the database:" + e.Message + ":" + fileName);
            }

            var t = new Thread(() => {
                Logger.Get().SafeExecuteWithWait(() => Presenter.IsMenuEnabled = false);
                _connectionString = "Data Source=" + fileName;
                try {
                    Sim = new Simulator(_connectionString);
                }
                catch (Exception e) {
                    Logger.Error("Could not load the selected file. The error was this:" + Environment.NewLine +
                                 Environment.NewLine + e.Message);

                    MessageBox.Show("Could not load the selected file. The error was this:" + Environment.NewLine +
                                    Environment.NewLine + e.Message);
                    DB3Path = string.Empty;
                    Logger.Get().SafeExecute(UpdateTitle);
                    try {
                        if (File.Exists(LastFileFileName)) {
                            File.Delete(LastFileFileName);
                        }
                    }
                    catch (Exception ex) {
                        Logger.Error(ex.Message);
                    }

                    return;
                }

                Logger.Get().SafeExecute(() => Presenter.Simulator = Sim);
                Logger.Get().SafeExecute(() => Presenter.IsMenuEnabled = true);
            });
            t.Start();
        }

        public void RemoveTab<T>([NotNull] PresenterBase<T> presenter, bool closelast = false) where T : class
        {
            if (Tabs.Items.Count < 2 && !closelast) {
                return;
            }

            for (var i = 0; i < Tabs.Items.Count; i++) {
                var item = (TabItem) Tabs.Items[i];

                if (item.DataContext.Equals(presenter)) {
                    Tabs.Items.Remove(item);
                    break;
                }
            }
        }

        public static void UpdateVacationsInHouseholdTemplates1([NotNull] Simulator sim)
        {
            var forFamiliesWithChildren =
                sim.DateBasedProfiles.Items.First(x => x.Name == "School Holidays Saxony, Germany, 2015, 1 = vacation");
            var noChildren =
                sim.DateBasedProfiles.Items.First(x => x.Name == "School Holidays Saxony, Germany, 2015, 1 = no vacation");

            foreach (var template in sim.HouseholdTemplates.Items) {
                template.TemplateVacationType = TemplateVacationType.RandomlyGenerated;
                template.MinNumberOfVacations = 1;
                template.MaxNumberOfVacations = 3;
                template.MinTotalVacationDays = 7;
                template.MaxTotalVacationDays = 21;
                template.AverageVacationDuration = 7;
                var minAge = template.Persons.Select(x => x.Person.Age).Min();

                if (minAge > 18) {
                    template.TimeProfileForVacations = noChildren;
                }
                else {
                    template.TimeProfileForVacations = forFamiliesWithChildren;
                }

                template.SaveToDB();
                Logger.Info("Updated " + template.PrettyName);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void About_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var aw = new AboutWindow();
                aw.ShowDialog();
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddAffordance_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddAffordance();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddAffordanceTaggingSet_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddAffordanceTaggingSet();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddDateBasedProfile_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var dbp = Presenter.AddDateBasedProfile();
                MyGlobalTree.SelectItem(dbp, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddDesire_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddDesire();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddDevice_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddDevice();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddDeviceAction_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var a = Presenter.AddDeviceAction();
                MyGlobalTree.SelectItem(a, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddDeviceActionGroup_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var g = Presenter.AddDeviceActionGroup();
                MyGlobalTree.SelectItem(g, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddDeviceCategory_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var dc = Presenter.AddDeviceCategory();
                MyGlobalTree.SelectItem(dc, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddDeviceSelection_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var ds = Presenter.AddDeviceSelection();
                MyGlobalTree.SelectItem(ds, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddDeviceTaggingSet_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddDeviceTaggingSet();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddEnergyStorageDevice_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var es = Presenter.AddEnergyStorageDevice();
                MyGlobalTree.SelectItem(es, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddGenerator_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var gen = Presenter.AddGenerator();
                MyGlobalTree.SelectItem(gen, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddGeographicLocation_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var gl = Presenter.AddGeographicLocation();
                MyGlobalTree.SelectItem(gl, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddHoliday_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var holiday = Presenter.AddHoliday();
                MyGlobalTree.SelectItem(holiday, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddHouse_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var h = Presenter.AddHouse();
                MyGlobalTree.SelectItem(h, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddHouseholdPlan_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddHouseholdPlan();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddHouseholdTemplate_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var g = Presenter.AddHouseholdTemplate();
                MyGlobalTree.SelectItem(g, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddHouseholdTrait_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var ht = Presenter.AddHouseholdTrait();
                MyGlobalTree.SelectItem(ht, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddHouseholdTraitTag_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var g = Presenter.AddTraitTag();
                MyGlobalTree.SelectItem(g, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddHouseType_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var ht = Presenter.AddHouseType();
                MyGlobalTree.SelectItem(ht, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddLoadType_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var dt = Presenter.AddLoadType();
                MyGlobalTree.SelectItem(dt, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddLocation_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddLocation();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddModularHousehold_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var ch = Presenter.AddModularHousehold();
                MyGlobalTree.SelectItem(ch, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddPerson_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddPerson();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddProfile_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var p = Presenter.AddTimeProfile();
                MyGlobalTree.SelectItem(p, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddSettlement_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var sett = Presenter.AddSettlement();
                MyGlobalTree.SelectItem(sett, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddSettlementTemplateClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var v = Presenter.AddSettlementTemplate();
                MyGlobalTree.SelectItem(v, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddSiteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddSite();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddSubAffordance_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var s = Presenter.AddSubAffordance();
                MyGlobalTree.SelectItem(s, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddTemperatureProfile_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var tp = Presenter.AddTemperatureProfile();
                MyGlobalTree.SelectItem(tp, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddTemplatePerson_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var v = Presenter.AddTemplatePerson();
                MyGlobalTree.SelectItem(v, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddTemplateTag_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddTemplateTag();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddTestSettlement_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var mbr =
                MessageWindowHandler.Mw.ShowYesNoMessage(
                    "Are you sure that you want to create a test settlement with all households?", "Create?");
            if (mbr == LPGMsgBoxResult.Yes) {
                try {
                    var sett = Presenter.AddTestSettlement();
                    MyGlobalTree.SelectItem(sett, false);
                }
                catch (Exception ex) {
                    MessageWindowHandler.Mw.ShowDebugMessage(ex);
                }
            }
            else {
                Logger.Warning("canceled.");
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddTimeLimit_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var dt = Presenter.AddTimeLimit();
                MyGlobalTree.SelectItem(dt, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddTransformDevice_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var td = Presenter.AddTransformationDevice();
                MyGlobalTree.SelectItem(td, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddTransportationDeviceCategoryClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddTransportationDeviceCategory();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddTravelRouteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddTravelRoute();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddTravelRouteSetClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var d = Presenter.AddTravelRouteSet();
                MyGlobalTree.SelectItem(d, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddVacation_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var v = Presenter.AddVacation();
                MyGlobalTree.SelectItem(v, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddVariable_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                var v = Presenter.AddVariable();
                MyGlobalTree.SelectItem(v, false);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AffordanceColorViewClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                Presenter.OpenItem("Affordance Color View");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AffordancesRealDevices([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                Presenter.OpenItem("Affordances with real devices");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AffordanceVariableClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                Presenter.OpenItem("Affordances Variable Overview");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        private void AllScreenshotter_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Sim == null) {
                return;
            }

            var mbr =
                MessageWindowHandler.Mw.ShowYesNoMessage("Are you sure that you want to take a full set of screenshots?",
                    "Create?");
            if (mbr == LPGMsgBoxResult.Yes) {
                Width = 1500;
                WindowGrid.ColumnDefinitions[2].Width = new GridLength(1000);
                var ssh = new ScreenshotHelper(Sim, Presenter, Tabs);
                ssh.Run();
                ssh.RunOthers();
            }
            else {
                Logger.Warning("canceled.");
            }
        }

        private void CalcRealisticTraitTimeEstimates_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var mbr =
                MessageWindowHandler.Mw.ShowYesNoMessage(
                    "Are you sure that you want to update the time estimates for all traits?", "Update?");
            var sim = Presenter.Simulator;

            if (mbr == LPGMsgBoxResult.Yes&&sim!=null) {
                var t = new Thread(() => RealisticTraitEstimator.Run(sim));
                t.Start();
            }
            else {
                Logger.Warning("canceled.");
            }
        }

        private void CloseAllTabs()
        {
            while (Tabs.Items.Count > 1) {
                var count = Tabs.Items.Count;
                var item = (TabItem) Tabs.Items[0];
                CloseTab(item, true);
                if (count == Tabs.Items.Count) {
                    throw new LPGException("Tab could not be closed?");
                }
            }
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => CloseDB();

        private void CloseTab([CanBeNull] TabItem item, bool closelast)
        {
            if (item == null) {
                return;
            }

            if (Tabs.Items.Count < 2 && !closelast) {
                return;
            }

            var thisType = item.DataContext.GetType();
            var theMethod = thisType.GetMethod("Close");
            if (theMethod == null) {
                return;
            }

            var parameters = new object[2];
            parameters[0] = true;
            parameters[1] = true;
            theMethod.Invoke(item.DataContext, parameters);
        }

        private void CmdCloseAllClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => CloseAllTabs();

        private void CmdTabItemCloseButtonClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (e == null) {
                return;
            }

            var b = (Button) e.OriginalSource;
            if (Tabs.Items.Count < 2) {
                return;
            }

            if (!(b.CommandParameter is TabItem ti))
            {
                return;
            }

            CloseTab(ti, false);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CompactDatabaseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (DB3Path == null) {
                return;
            }

            try {
                MessageWindowHandler.Mw.ShowInfoMessage(
                    "This will close the database, perform cleanup work and open it again. This might take a minute.",
                    "Please wait.");
                var oldpath = DB3Path;
                CloseDB();
                using (var con = new Connection(_connectionString)) {
                    using (var cmd = new Command(con)) {
                        con.Open();
                        cmd.ExecuteNonQuery("vacuum;");
                    }
                }

                OpenDatabase(oldpath);
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        private void ComprehensiveAddClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var caa = new CompleteAffordanceAdd(Presenter);
            caa.ShowDialog();
        }

        private void DeleteAllHouseholdTemplates_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Sim == null) {
                return;
            }

            try {
                var result =
                    MessageWindowHandler.Mw.ShowYesNoMessage("Delete all household templates now?",
                        "Delete Household Templates?");
                if (result == LPGMsgBoxResult.Yes) {
                    var i = 1;
                    while (Sim.HouseholdTemplates.Items.Count > 0) {
                        Logger.Warning("Deleting template " + i + ": " + Sim.HouseholdTemplates.Items[0]);
                        Sim.HouseholdTemplates.DeleteItem(Sim.HouseholdTemplates.Items[0]);
                        i++;
                    }
                }
            }
            catch (Exception ex) {
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void DeviceOverviewClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                Presenter.OpenItem("Device Overview");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        private void ExitClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Close();

        private static void FindDefaultDB3File()
        {
#pragma warning disable S1075 // URIs should not be hardcoded
            if (File.Exists(@"c:\work\profilegenerator.db3")) {
#pragma warning restore S1075 // URIs should not be hardcoded
                DB3Path = @"c:\work\profilegenerator.db3";
                return;
            }

            if (File.Exists(LastFileFileName)) {
                using (var sr = new StreamReader(LastFileFileName)) {
                    var filename = sr.ReadLine();
                    if (File.Exists(filename)) {
                        DB3Path = filename;
                        return;
                    }
                }
            }
            else {
                Logger.ImportantInfo("Didn't find any information from the previous start.");
            }

            if (File.Exists("profilegenerator.db3")) {
                DB3Path = "profilegenerator.db3";
                return;
            }

            Logger.Warning("Didn't find any DB3 in the program folder.");
            var apppath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            apppath = Path.Combine(apppath, "LoadProfileGenerator", "profilegenerator.db3");
            if (File.Exists(apppath)) {
                DB3Path = apppath;
                return;
            }

            Logger.Warning("Didn't find any DB3 in the app data folder: " + apppath);
            DB3Path = string.Empty;
        }

        private void HHTemplateCreatorClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Sim == null) {
                return;
            }

            var mbr =
                MessageWindowHandler.Mw.ShowYesNoMessage("Are you sure that you want to create the household templates?",
                    "Create?");
            if (mbr == LPGMsgBoxResult.Yes) {
                var hhtc = new HouseholdTemplateCreator(Sim);
                hhtc.Run(true, Sim);
            }
            else {
                Logger.Warning("canceled.");
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void HouseholdsRealDevicesClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                Presenter.OpenItem("Households with real devices");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        private void HouseholdTemplateVacationUpdate_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Sim == null) {
                return;
            }

            UpdateVacationsInHouseholdTemplates1(Sim);
            MessageWindowHandler.Mw.ShowInfoMessage("Finished updating...", "LPG");
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ImportClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                Presenter.OpenItem("Import");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void IntegrityCheck_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Sim == null) {
                return;
            }

            var sim = Sim;
            var presenter = Presenter;
            var task1 = new Task(() => {
                try {
                    SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                }
                catch (Exception ex) {
                    if (!Config.IsInUnitTesting) {
                        if (ex is DataIntegrityException exception) {
                            MessageWindowHandler.Mw.ShowDataIntegrityMessage(exception);
                            if (exception.Element != null) {
                                Logger.Get().SafeExecute(() => presenter.OpenItem(exception.Element));
                            }

                            if (exception.Elements != null) {
                                foreach (var basicElement in exception.Elements) {
                                    Logger.Get().SafeExecute(() => presenter.OpenItem(basicElement));
                                }
                            }
                        }
                        else {
                            MessageWindowHandler.Mw.ShowDebugMessage(ex);
                        }
                    }
                    else {
                        Logger.Exception(ex);
                        throw;
                    }
                }

                Logger.Info("Finished integrity check.");
            });
            task1.Start();
        }

        private void LstProgress_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstProgress.SelectedItem == null) {
                return;
            }

            var lm = (Logger.LogMessage) LstProgress.SelectedItem;
            var sew = new ScrollingErrorWindow(lm.Message, "Message");
            sew.Show();
        }

        private void OpenClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog {
                DefaultExt = ".db3",
                Filter = "Database Files (*.db3)|*.db3|All files (*.*)|*.*"
            };
            var result = ofd.ShowDialog();
            if (result == true) {
                var filename = ofd.FileName;
                if (File.Exists(filename)) {
                    OpenDatabase(filename);
                }
            }
        }

        private void SaveACopyClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => SaveAs();

        [NotNull]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private string SaveAs()
        {
            if (Sim != null && DB3Path != null) {
                CloseAllTabs();
                var ofd = new SaveFileDialog {
                    DefaultExt = ".db3",
                    Filter = "Database Files (*.db3)|*.db3|All files (*.*)|*.*"
                };
                var result = ofd.ShowDialog();
                if (result == true) {
                    var filename = ofd.FileName;
                    if (!File.Exists(filename)) {
                        try {
                            File.Copy(DB3Path, filename);
                            return filename;
                        }
                        catch (Exception) {
                            return string.Empty;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private void SaveAsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var s = SaveAs();
            if (!string.IsNullOrWhiteSpace(s)) {
                OpenDatabase(s);
            }
        }

        private void Screenshotter_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Sim == null) {
                return;
            }

            var idx = Tabs.SelectedIndex;
            var ti = (TabItem) Tabs.Items[idx];
            var screenshotHelper = new ScreenshotHelper(Sim, Presenter, Tabs);
            screenshotHelper.SnapshotExpanders(ti);
        }

        private void TemplatedDeleteorClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Sim == null) {
                return;
            }

            Sim.DeleteAllTemplatedItems(false);
        }

        private void TemplatePersonHouseholds_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Sim == null) {
                return;
            }

            var mbr =
                MessageWindowHandler.Mw.ShowYesNoMessage(
                    "Are you sure that you want to create the test households for the templated persons?", "Create?");
            if (mbr == LPGMsgBoxResult.Yes) {
                TemplatePersonCreator.RunCalculationTests(Sim);
            }
            else {
                Logger.Warning("canceled.");
            }
        }

        private void ThrowTestException_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
                throw new NotImplementedException("Test error to crash the program");
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
            }
            catch (Exception ex) {
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void TimeLimitsAffordancesViewClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                Presenter.OpenItem("Affordance Time Limit Overview");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void UnusedAffordancesClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                Presenter.OpenItem("Unused Affordances");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void UnusedTimeLimitsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try {
                Presenter.OpenItem("Unused Time Limits");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
            }
        }

        private void UpdateTemplatePersonDescriptions_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Sim == null) {
                return;
            }

            var mbr =
                MessageWindowHandler.Mw.ShowYesNoMessage("Are you sure that you want to create or update the templated persons?",
                    "Create?");
            if (mbr == LPGMsgBoxResult.Yes) {
                var t = new Thread(() => {
                    PersonDescriptionFixer.FillPersonDescriptions(Sim);
                    TemplatePersonCreator.CreateTemplatePersons(Sim);
                });
                t.Start();
            }
            else {
                Logger.Warning("canceled.");
            }
        }

        private void UpdateTitle()
        {
            var filename = "(no database selected)";
            if (!string.IsNullOrEmpty(DB3Path)) {
                filename = DB3Path;
            }

            Title = "LoadProfileGenerator " + _version + " (" + filename + ")";
        }

        private void Window_Closing([CanBeNull] object sender, [CanBeNull] CancelEventArgs e)
        {
            while (Tabs.Items.Count > 0) {
                var count = Tabs.Items.Count;
                var item = (TabItem) Tabs.Items[0];
                Logger.Info("Closing Tab " + item.Name);
                CloseTab(item, true);
                if (count == Tabs.Items.Count) {
                    throw new LPGException("A tab closing event is missing.");
                }
            }
        }

        private void AddTransportationDeviceClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            try
            {
                var ds = Presenter.AddTransportationDevice();
                MyGlobalTree.SelectItem(ds, false);
            }
            catch (Exception ex)
            {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        private void AddTransportationDeviceSetClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            try
            {
                var ds = Presenter.AddTransportationDeviceSet();
                MyGlobalTree.SelectItem(ds, false);
            }
            catch (Exception ex)
            {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        private void AddChargingStationSet([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            try
            {
                var ds = Presenter.AddChargingStationSet();
                MyGlobalTree.SelectItem(ds, false);
            }
            catch (Exception ex)
            {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        private void Shell_OnClosed([NotNull] object sender, [NotNull] EventArgs e)
        {

            Application.Current.Shutdown();
        }


        private void AddLivingPatternTag_Click(object sender, RoutedEventArgs e)
        {
                try
                {
                    var d = Presenter.AddLivingPatternTag();
                    MyGlobalTree.SelectItem(d, false);
                }
                catch (Exception ex)
                {
                    MessageWindowHandler.Mw.ShowDebugMessage(ex);
                }
        }

        private void BtnCopyLivingPatterns(object sender, RoutedEventArgs e)
        {
            //RunLivingPatternCopy(Sim??throw new LPGException("was null"));
            //ConvertAllPersonTagsToLivingPatternTags(Sim ?? throw new LPGException("was null"));
        }

        public static void RunLivingPatternCopy([NotNull] Simulator sim)
        {
            foreach (var tag in sim.TraitTags.Items) {
                if (tag.Name.StartsWith("Living Pattern")) {
                    var newtag= sim.LivingPatternTags.CreateNewItem(sim.ConnectionString);
                    newtag.Name = tag.Name;
                    newtag.SaveToDB();
                }
            }
        }

        //public static void ConvertAllPersonTagsToLivingPatternTags([NotNull] Simulator sim)
        //{
        //    foreach (var household in sim.ModularHouseholds.Items) {
        //        foreach (var person in household.Persons) {
        //            var srcTag = sim.LivingPatternTags.FindFirstByNameNotNull(person.TraitTag?.Name);
        //            person.LivingPatternTag = srcTag;
        //            person.SaveToDB();
        //        }
        //    }

        //    foreach (var template in sim.HouseholdTemplates.Items) {
        //        foreach (var person in template.Persons)
        //        {
        //            var srcTag = sim.LivingPatternTags.FindFirstByNameNotNull(person.LivingPatternTag?.Name);
        //            person.LivingPatternTag = srcTag;
        //            person.SaveToDB();
        //        }
        //        template.SaveToDB();
        //    }
        //}

        public static void ConvertAllTraitTagsToLivingPatternTags([NotNull] Simulator sim)
        {

            foreach (var trait in sim.HouseholdTraits.Items) {
                var tagstoremove = new List<HHTTag>();
                foreach (var tag in trait.Tags) {
                    if (tag.Tag.Name.StartsWith("Living Pattern")) {
                        var lpTag = sim.LivingPatternTags.Items.Single(x => x.Name == tag.Tag.Name);
                        Logger.Info("Converting tag " + tag.Tag.Name);
                        trait.AddLivingPatternTag(lpTag);
                        tagstoremove.Add(tag);
                    }
                }

                foreach (var tag in tagstoremove) {
                    trait.DeleteHHTTag(tag);
                }
            }
        }
    }
}
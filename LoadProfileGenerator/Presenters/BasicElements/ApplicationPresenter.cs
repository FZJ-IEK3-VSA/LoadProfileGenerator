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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;
using LoadProfileGenerator.Presenters.Houses;
using LoadProfileGenerator.Presenters.SpecialViews;
using LoadProfileGenerator.Views.BasicElements;
using LoadProfileGenerator.Views.Households;
using LoadProfileGenerator.Views.Houses;
using LoadProfileGenerator.Views.SpecialViews;
using LoadProfileGenerator.Views.Transportation;

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class ApplicationPresenter : PresenterBase<Shell> {
        [JetBrains.Annotations.NotNull] private readonly Dictionary<string, Action<object>> _openItemDict = new Dictionary<string, Action<object>>();

        [CanBeNull] private readonly WelcomePresenter _welcomePresenter;

        [CanBeNull] private string _filterString;

        private bool _isMenuEnabled;
        [CanBeNull] private Simulator _simulator;

        public ApplicationPresenter([JetBrains.Annotations.NotNull] Shell view, [CanBeNull] Simulator simulator, Dispatcher mainDispatcher)
            : base(view, "ApplicationPresenter")
        {
            Shell = view;
            MainDispatcher = mainDispatcher;
            _simulator = simulator;
            if (_simulator != null) {
                CurrentCategories = _simulator.Categories;
            }

            if (!Config.IsInUnitTesting) {
                _welcomePresenter = new WelcomePresenter(this, new WelcomeView());
                View.AddTab(_welcomePresenter);
                InitOpenItemDict();
            }
        }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<object> CurrentCategories { get; private set; }

        [CanBeNull]
        [UsedImplicitly]
        public string FilterString {
            get => _filterString;
            set {
                _filterString = value;
                if (CurrentCategories != null) {
                    foreach (var currentcategory in CurrentCategories) {
                        var bc = currentcategory as BasicCategory;
                        bc?.ApplyFilter(value);
                    }
                }
            }
        }

        [UsedImplicitly]
        public bool IsMenuEnabled {
            get => _isMenuEnabled;
            set {
                _isMenuEnabled = value;
                OnPropertyChanged(nameof(IsMenuEnabled));
            }
        }

        // the log at the bottom of the screen
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Logger.LogMessage> LogMessages => Logger.Get().LogCol;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public Shell Shell { get; }

        public Dispatcher MainDispatcher { get; }

        [CanBeNull]
        public Simulator Simulator {
            get => _simulator;
            set {
                _simulator = value;
                if (_simulator != null) {
                    CurrentCategories = _simulator.Categories;
                    IsMenuEnabled = true;
                }
                else {
                    CurrentCategories = null;
                    IsMenuEnabled = false;
                }

                OnPropertyChanged(nameof(CurrentCategories));
                OnPropertyChanged(nameof(Simulator));
                OnPropertyChanged(nameof(IsMenuEnabled));
            }
        }

        [CanBeNull]
        public WelcomePresenter WelcomePresenter => _welcomePresenter;

        [JetBrains.Annotations.NotNull]
        public Affordance AddAffordance()
        {
            if(Simulator == null) {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.Affordances.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new AffordancePresenter(this, new AffordanceView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public AffordanceTaggingSet AddAffordanceTaggingSet()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.AffordanceTaggingSets.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new AffordanceTaggingSetPresenter(this, new AffordanceTaggingSetView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public DateBasedProfile AddDateBasedProfile()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var dateBasedProfile = Simulator.DateBasedProfiles.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new DateBasedProfilePresenter(this, new DateBasedProfileView(), dateBasedProfile));
            dateBasedProfile.SaveToDB();
            return dateBasedProfile;
        }

        [JetBrains.Annotations.NotNull]
        public Desire AddDesire()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.Desires.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new DesirePresenter(this, new DesireView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public RealDevice AddDevice()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.RealDevices.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new DevicePresenter(this, new DeviceView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public DeviceAction AddDeviceAction()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.DeviceActions.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new DeviceActionPresenter(this, new DeviceActionView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public DeviceActionGroup AddDeviceActionGroup()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.DeviceActionGroups.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new DeviceActionGroupPresenter(this, new DeviceActionGroupView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public DeviceCategory AddDeviceCategory()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.DeviceCategories.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new DeviceCategoryPresenter(this, new DeviceCategoryView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public DeviceSelection AddDeviceSelection()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var p = Simulator.DeviceSelections.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new DeviceSelectionPresenter(this, new DeviceSelectionView(), p));
            p.SaveToDB();
            return p;
        }

        [JetBrains.Annotations.NotNull]
        public DeviceTaggingSet AddDeviceTaggingSet()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.DeviceTaggingSets.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new DeviceTaggingSetPresenter(this, new DeviceTaggingSetView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public EnergyStorage AddEnergyStorageDevice()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.EnergyStorages.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new EnergyStoragePresenter(this, new EnergyStorageView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public Generator AddGenerator()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var gen = Simulator.Generators.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new GeneratorPresenter(this, new GeneratorView(), gen));
            gen.SaveToDB();
            return gen;
        }

        [JetBrains.Annotations.NotNull]
        public GeographicLocation AddGeographicLocation()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var p = Simulator.GeographicLocations.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new GeographicLocationPresenter(this, new GeographicLocationView(), p));
            p.SaveToDB();
            return p;
        }

        [JetBrains.Annotations.NotNull]
        public Holiday AddHoliday()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var holiday = Simulator.Holidays.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new HolidayPresenter(this, new HolidayView(), holiday));
            holiday.SaveToDB();
            return holiday;
        }

        [JetBrains.Annotations.NotNull]
        public House AddHouse()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.Houses.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new HousePresenter(this, new HouseView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public HouseholdPlan AddHouseholdPlan()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.HouseholdPlans.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new HouseholdPlanPresenter(this, new HouseholdPlanView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public HouseholdTemplate AddHouseholdTemplate()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.HouseholdTemplates.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new HouseholdTemplatePresenter(this, new HouseholdTemplateView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public HouseholdTrait AddHouseholdTrait()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var ht = Simulator.HouseholdTraits.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new HouseholdTraitPresenter(this, new HouseholdTraitView(), ht));
            ht.SaveToDB();
            return ht;
        }

        [JetBrains.Annotations.NotNull]
        public HouseType AddHouseType()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.HouseTypes.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new HouseTypePresenter(this, new HouseTypeView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public VLoadType AddLoadType()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.LoadTypes.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new LoadTypePresenter(this, new LoadTypeView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public Location AddLocation()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var p = Simulator.Locations.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new LocationPresenter(this, new LocationView(), p));
            p.SaveToDB();
            return p;
        }

        [JetBrains.Annotations.NotNull]
        public ModularHousehold AddModularHousehold()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var chh = Simulator.ModularHouseholds.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new ModularHouseholdPresenter(this, new ModularHouseholdView(), chh));
            chh.SaveToDB();
            return chh;
        }

        [JetBrains.Annotations.NotNull]
        public Person AddPerson()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var p = Simulator.Persons.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new PersonPresenter(this, new PersonView(), p));
            p.SaveToDB();
            return p;
        }

        [JetBrains.Annotations.NotNull]
        public Settlement AddSettlement()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var sett = Simulator.Settlements.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new SettlementPresenter(this, new SettlementView(), sett));
            sett.SaveToDB();
            return sett;
        }

        [JetBrains.Annotations.NotNull]
        public SettlementTemplate AddSettlementTemplate()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.SettlementTemplates.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new SettlementTemplatePresenter(this, new SettlementTemplateView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public Site AddSite()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.Sites.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new SitePresenter(this, new SiteView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public SubAffordance AddSubAffordance()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.SubAffordances.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new SubAffordancePresenter(this, new SubAffordanceView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public TemperatureProfile AddTemperatureProfile()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.TemperatureProfiles.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TemperatureProfilePresenter(this, new TemperatureProfileView(), d));
            d.SaveToDB();
            return d;
        }

        //[JetBrains.Annotations.NotNull]
        //public object AddTemplatePerson()
        //{
        //    if (Simulator == null)
        //    {
        //        throw new LPGException("Simulator was null");
        //    }

        //    var d = Simulator.TemplatePersons.CreateNewItem(Simulator.ConnectionString);
        //    View.AddTab(new TemplatePersonPresenter(this, new TemplatePersonView(), d));
        //    d.SaveToDB();
        //    return d;
        //}

        [JetBrains.Annotations.NotNull]
        public HouseholdTag AddTemplateTag()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.HouseholdTags.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TemplateTagPresenter(this, new TemplateTagView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public Settlement AddTestSettlement()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var sett = Simulator.Settlements.CreateNewItem(Simulator.ConnectionString);
            foreach (var household in Simulator.ModularHouseholds.Items) {
                sett.AddHousehold(household, 1);
            }

            foreach (var household in Simulator.Houses.Items) {
                sett.AddHousehold(household, 1);
            }

            View.AddTab(new SettlementPresenter(this, new SettlementView(), sett));
            sett.SaveToDB();
            return sett;
        }

        [JetBrains.Annotations.NotNull]
        public TimeLimit AddTimeLimit()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.TimeLimits.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TimeLimitPresenter(this, new TimeLimitView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public TimeBasedProfile AddTimeProfile()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.Timeprofiles.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TimeProfilePresenter(this, new TimeProfileView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public TraitTag AddTraitTag()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var t = Simulator.TraitTags.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TraitTagPresenter(this, new TraitTagView(), t));
            t.SaveToDB();
            return t;
        }

        [JetBrains.Annotations.NotNull]
        public TransformationDevice AddTransformationDevice()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.TransformationDevices.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TransformationDevicePresenter(this, new TransformationDeviceView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public TransportationDeviceCategory AddTransportationDeviceCategory()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.TransportationDeviceCategories.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TransportationDeviceCategoryPresenter(this, new TransportationDeviceCategoryView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public TravelRoute AddTravelRoute()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.TravelRoutes.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TravelRoutePresenter(this, new TravelRouteView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public TravelRouteSet AddTravelRouteSet()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.TravelRouteSets.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TravelRouteSetPresenter(this, new TravelRouteSetView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public Vacation AddVacation()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.Vacations.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new VacationPresenter(this, new VacationView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public object AddVariable()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.Variables.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new VariablePresenter(this, new VariableView(), d));
            d.SaveToDB();
            return d;
        }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            // needed from interface
        }

        public void CloseTab<T>([JetBrains.Annotations.NotNull] PresenterBase<T> presenter, bool removeLast) where T : class
        {
            presenter.UnloadingStarted = true;
            View.RemoveTab(presenter, removeLast);
        }

        public bool OpenItem([CanBeNull] object o)
        {
            if (o == null) {
                return false;
            }

            Logger.Debug("Opening new tab: " + o);
            string key;
            if (o is string s) {
                key = s;
            }
            else {
                if (o is OtherCategory category) {
                    key = "OtherCategory." + category.Name;
                }
                else {
                    key = o.GetType().FullName;
                }
            }

            if (key == null) {
                throw new LPGException("Key was null. Please fix.");
            }

            if (_openItemDict.ContainsKey(key)) {
                //var f = Logger.Get().SaveExecutionFunctionWithWait;
                //if (f != null) {
                //f(() => _openItemDict[key].Invoke(o));
                //}
                //else
                //{
                //  _openItemDict[key].Invoke(o);
                //}
                var a = new Action(() => _openItemDict[key].Invoke(o));
                if(MainDispatcher!=null) {
                    MainDispatcher.BeginInvoke(a);
                }
                else {
                    a();
                }
            }
            else {
                if (!key.StartsWith("Database.Helpers.CategoryDBBase", StringComparison.Ordinal) &&
                    key != "Database.Helpers.CategoryDeviceCategory" &&
                    key != "Database.Helpers.CategoryAffordance" && key != "Database.Helpers.CategorySettlement") {
                    throw new LPGException("Missing item for opening: " + key);
                }
            }

            return true;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        private void InitOpenItemDict()
        {
            _openItemDict.Add(typeof(ResultPresenter).FullName, x => View.AddTab((ResultPresenter) x));
            //_openItemDict.Add(typeof(SettlementResultsPresenter).FullName,x => View.AddTab((SettlementResultsPresenter) x));
            _openItemDict.Add(typeof(Desire).FullName,
                x => View.AddTab(new DesirePresenter(this, new DesireView(), (Desire) x)));
            _openItemDict.Add(typeof(Location).FullName,
                x => View.AddTab(new LocationPresenter(this, new LocationView(), (Location) x)));
            _openItemDict.Add(typeof(DeviceCategory).FullName,
                x => View.AddTab(new DeviceCategoryPresenter(this, new DeviceCategoryView(), (DeviceCategory) x)));
            _openItemDict.Add(typeof(Person).FullName,
                x => View.AddTab(new PersonPresenter(this, new PersonView(), (Person) x)));
            _openItemDict.Add(typeof(RealDevice).FullName,
                x => View.AddTab(new DevicePresenter(this, new DeviceView(), (RealDevice) x)));
            _openItemDict.Add(typeof(TimeBasedProfile).FullName,
                x => View.AddTab(new TimeProfilePresenter(this, new TimeProfileView(), (TimeBasedProfile) x)));
            _openItemDict.Add(typeof(SubAffordance).FullName,
                x => View.AddTab(new SubAffordancePresenter(this, new SubAffordanceView(), (SubAffordance) x)));
            _openItemDict.Add(typeof(Settlement).FullName,
                x => View.AddTab(new SettlementPresenter(this, new SettlementView(), (Settlement) x)));
            _openItemDict.Add(typeof(TemperatureProfile).FullName,
                x =>
                    View.AddTab(new TemperatureProfilePresenter(this, new TemperatureProfileView(),
                        (TemperatureProfile) x)));
            _openItemDict.Add(typeof(House).FullName,
                x => View.AddTab(new HousePresenter(this, new HouseView(), (House) x)));
            _openItemDict.Add(typeof(TimeLimit).FullName,
                x => View.AddTab(new TimeLimitPresenter(this, new TimeLimitView(), (TimeLimit) x)));
            _openItemDict.Add(typeof(VLoadType).FullName,
                x => View.AddTab(new LoadTypePresenter(this, new LoadTypeView(), (VLoadType) x)));
            _openItemDict.Add(typeof(TransformationDevice).FullName,
                x =>
                    View.AddTab(new TransformationDevicePresenter(this, new TransformationDeviceView(),
                        (TransformationDevice) x)));
            _openItemDict.Add(typeof(EnergyStorage).FullName,
                x => View.AddTab(new EnergyStoragePresenter(this, new EnergyStorageView(), (EnergyStorage) x)));
            _openItemDict.Add(typeof(GeographicLocation).FullName,
                x =>
                    View.AddTab(new GeographicLocationPresenter(this, new GeographicLocationView(),
                        (GeographicLocation) x)));
            _openItemDict.Add(typeof(DateBasedProfile).FullName,
                x => View.AddTab(
                    new DateBasedProfilePresenter(this, new DateBasedProfileView(), (DateBasedProfile) x)));
            _openItemDict.Add(typeof(Generator).FullName,
                x => View.AddTab(new GeneratorPresenter(this, new GeneratorView(), (Generator) x)));
            _openItemDict.Add(typeof(Holiday).FullName,
                x => View.AddTab(new HolidayPresenter(this, new HolidayView(), (Holiday) x)));
            _openItemDict.Add(typeof(HouseType).FullName,
                x => View.AddTab(new HouseTypePresenter(this, new HouseTypeView(), (HouseType) x)));
            _openItemDict.Add(typeof(HouseholdTrait).FullName,
                x => View.AddTab(new HouseholdTraitPresenter(this, new HouseholdTraitView(), (HouseholdTrait) x)));
            _openItemDict.Add(typeof(ModularHousehold).FullName,
                x => View.AddTab(
                    new ModularHouseholdPresenter(this, new ModularHouseholdView(), (ModularHousehold) x)));
            _openItemDict.Add(typeof(DeviceSelection).FullName,
                x => View.AddTab(new DeviceSelectionPresenter(this, new DeviceSelectionView(), (DeviceSelection) x)));
            _openItemDict.Add(typeof(AffordanceTaggingSet).FullName,
                x =>
                    View.AddTab(new AffordanceTaggingSetPresenter(this, new AffordanceTaggingSetView(),
                        (AffordanceTaggingSet) x)));
            _openItemDict.Add(typeof(DeviceTaggingSet).FullName,
                x => View.AddTab(
                    new DeviceTaggingSetPresenter(this, new DeviceTaggingSetView(), (DeviceTaggingSet) x)));
            _openItemDict.Add(typeof(HouseholdPlan).FullName,
                x => View.AddTab(new HouseholdPlanPresenter(this, new HouseholdPlanView(), (HouseholdPlan) x)));
            _openItemDict.Add(typeof(DeviceActionGroup).FullName,
                x =>
                    View.AddTab(new DeviceActionGroupPresenter(this, new DeviceActionGroupView(),
                        (DeviceActionGroup) x)));
            _openItemDict.Add(typeof(DeviceAction).FullName,
                x => View.AddTab(new DeviceActionPresenter(this, new DeviceActionView(), (DeviceAction) x)));
            _openItemDict.Add(typeof(TraitTag).FullName,
                x => View.AddTab(new TraitTagPresenter(this, new TraitTagView(), (TraitTag) x)));
            _openItemDict.Add(typeof(LivingPatternTag).FullName,
                x => View.AddTab(new LivingPatternTagPresenter(this, new LivingPatternTagView(), (LivingPatternTag)x)));
#pragma warning restore RCS1163 // Unused parameter.
            _openItemDict.Add(typeof(HouseholdTemplate).FullName,
                x =>
                    View.AddTab(new HouseholdTemplatePresenter(this, new HouseholdTemplateView(),
                        (HouseholdTemplate) x)));
            _openItemDict.Add(typeof(Vacation).FullName,
                x => View.AddTab(new VacationPresenter(this, new VacationView(), (Vacation) x)));
            _openItemDict.Add(typeof(SettlementTemplate).FullName,
                x =>
                    View.AddTab(new SettlementTemplatePresenter(this, new SettlementTemplateView(),
                        (SettlementTemplate) x)));
#pragma warning disable RCS1163 // Unused parameter.
            _openItemDict.Add(typeof(CategoryOutcome).FullName, _ => View.AddTab(new CalculationOutcomesPresenter(this, new CalculationOutcomesView())));
#pragma warning restore RCS1163 // Unused parameter.
            _openItemDict.Add(typeof(Variable).FullName,
                x => View.AddTab(new VariablePresenter(this, new VariableView(), (Variable) x)));
            _openItemDict.Add(typeof(HouseholdTag).FullName,
                x => View.AddTab(new TemplateTagPresenter(this, new TemplateTagView(), (HouseholdTag) x)));
            //_openItemDict.Add(typeof(TemplatePerson).FullName,
            //    x => View.AddTab(new TemplatePersonPresenter(this, new TemplatePersonView(), (TemplatePerson) x)));
            _openItemDict.Add(typeof(Affordance).FullName,
                x => View.AddTab(new AffordancePresenter(this, new AffordanceView(), (Affordance) x)));

            _openItemDict.Add(typeof(Site).FullName,
                x => View.AddTab(new SitePresenter(this, new SiteView(), (Site) x)));

            _openItemDict.Add(typeof(TransportationDeviceCategory).FullName,
                x => View.AddTab(new TransportationDeviceCategoryPresenter(this, new TransportationDeviceCategoryView(),
                    (TransportationDeviceCategory) x)));

            _openItemDict.Add(typeof(TransportationDeviceSet).FullName,
                x => View.AddTab(new TransportationDeviceSetPresenter(this, new TransportationDeviceSetView(),
                    (TransportationDeviceSet)x)));

            _openItemDict.Add(typeof(TransportationDevice).FullName,
                x => View.AddTab(new TransportationDevicePresenter(this, new TransportationDeviceView(),
                    (TransportationDevice)x)));

            _openItemDict.Add(typeof(TravelRoute).FullName,
                x => View.AddTab(new TravelRoutePresenter(this, new TravelRouteView(), (TravelRoute) x)));

            _openItemDict.Add(typeof(TravelRouteSet).FullName,
                x => View.AddTab(new TravelRouteSetPresenter(this, new TravelRouteSetView(), (TravelRouteSet) x)));

            _openItemDict.Add(typeof(ChargingStationSet).FullName,
                x => View.AddTab(new ChargingStationSetPresenter(this,
                    new ChargingStationSetView(), (ChargingStationSet)x)));

            _openItemDict.Add("OtherCategory.Calculation",
                _=> View.AddTab(new CalculationPresenter(this, new CalculateView())));
            _openItemDict.Add("OtherCategory.Settings",
                _ => View.AddTab(new SettingPresenter(this, new SettingsView())));
            _openItemDict.Add("Import", _ => View.AddTab(new ImportPresenter(this, new ImportView())));
            _openItemDict.Add("Affordance Color View",
                _ => View.AddTab(new AffordanceColorPresenter(this, new AffordanceColorView())));
            _openItemDict.Add("Affordance Time Limit Overview",
                _ => View.AddTab(new AffordancesTimeLimitsPresenter(this, new AffordancesTimeLimitView())));
            _openItemDict.Add("Affordances with real devices",
                _ => View.AddTab(new AffordancesWithRealDevicesPresenter(this, new AffordancesWithRealDevicesView())));
            _openItemDict.Add("Households with real devices",
                _ => View.AddTab(new HouseholdsWithRealDevicesPresenter(this, new HouseholdsWithRealDevicesView())));
            _openItemDict.Add("Unused Affordances",
                _ => View.AddTab(new AffordanceUnusedPresenter(this, new AffordancesUnusedView())));
            _openItemDict.Add("Unused Time Limits",
                _ => View.AddTab(new TimeLimitUnusedPresenter(this, new TimeLimitUnusedView())));
            _openItemDict.Add("Device Overview",
                _ => View.AddTab(new DeviceOverviewPresenter(this, new DeviceOverviewView())));
            _openItemDict.Add("Affordances Variable Overview",
                _ => View.AddTab(new AffordanceVariablePresenter(this, new AffordanceVariableView())));
        }

        [JetBrains.Annotations.NotNull]
        public object AddTransportationDevice()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.TransportationDevices.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TransportationDevicePresenter(this, new TransportationDeviceView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public object AddTransportationDeviceSet()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var d = Simulator.TransportationDeviceSets.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new TransportationDeviceSetPresenter(this, new TransportationDeviceSetView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public object AddChargingStationSet()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }
            var d = Simulator.ChargingStationSets.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new ChargingStationSetPresenter(this, new ChargingStationSetView(), d));
            d.SaveToDB();
            return d;
        }

        [JetBrains.Annotations.NotNull]
        public object AddLivingPatternTag()
        {
            if (Simulator == null)
            {
                throw new LPGException("Simulator was null");
            }

            var t = Simulator.LivingPatternTags.CreateNewItem(Simulator.ConnectionString);
            View.AddTab(new LivingPatternTagPresenter(this, new LivingPatternTagView(), t));
            t.SaveToDB();
            return t;
        }
    }
}
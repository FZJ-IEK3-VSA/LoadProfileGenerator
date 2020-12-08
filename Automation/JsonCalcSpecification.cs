using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Automation {
    public class JsonCalcSpecification {
        public JsonCalcSpecification()
        {
        }
        //[CanBeNull] JsonReference calcObject,
        public JsonCalcSpecification( bool deleteAllButPDF, [CanBeNull] JsonReference? deviceSelection,
                                     [CanBeNull]DateTime? endDate, [CanBeNull] string? externalTimeResolution, [CanBeNull] string? internalTimeResolution,
                                     [CanBeNull] JsonReference? geographicLocation, LoadTypePriority loadTypePriorityEnum,
                                      [CanBeNull] string? outputDirectory, bool showSettlingPeriod,
                                     [CanBeNull] DateTime? startDate, [CanBeNull] JsonReference? temperatureProfile, bool enableTransportation
                                      )
        {
            //CalcObject = calcObject;
            DeleteAllButPDF = deleteAllButPDF;
            DeviceSelection = deviceSelection;
            EndDate = endDate;
            ExternalTimeResolution = externalTimeResolution;
            InternalTimeResolution = internalTimeResolution;
            GeographicLocation = geographicLocation;
            LoadTypePriority = loadTypePriorityEnum;
            OutputDirectory = outputDirectory;
            ShowSettlingPeriod = showSettlingPeriod;
            StartDate = startDate;
            TemperatureProfile = temperatureProfile;
            EnableTransportation = enableTransportation;
        }

        public JsonCalcSpecification([JetBrains.Annotations.NotNull] JsonCalcSpecification o)
        {
            //CalcObject = o.CalcObject;
            DefaultForOutputFiles = o.DefaultForOutputFiles;
            if (CalcOptions == null) {
                CalcOptions = new List<CalcOption>();
            }

            if (o.CalcOptions != null) {
                CalcOptions.AddRange(o.CalcOptions);
            }

            DeleteAllButPDF = o.DeleteAllButPDF;
            DeleteDAT = o.DeleteDAT;
            DeviceSelection = o.DeviceSelection;
            StartDate = o.StartDate;
            EndDate = o.EndDate;
            EnergyIntensityType = o.EnergyIntensityType;
            ExternalTimeResolution = o.ExternalTimeResolution;
            GeographicLocation = o.GeographicLocation;
            LoadTypePriority = o.LoadTypePriority;
            OutputDirectory = o.OutputDirectory;
            RandomSeed = o.RandomSeed;
            ShowSettlingPeriod = o.ShowSettlingPeriod;
            SkipExisting = o.SkipExisting;
            TemperatureProfile = o.TemperatureProfile;
            InternalTimeResolution = o.InternalTimeResolution;
            LoadtypesForPostprocessing = new List<string>();
            if (o.LoadtypesForPostprocessing != null)
            {
                LoadtypesForPostprocessing.AddRange(o.LoadtypesForPostprocessing);
            }
            DeleteSqlite = o.DeleteSqlite;
            EnableTransportation = o.EnableTransportation;
        }

        [Comment(
            "List of all load types to process in postprocessing. Internally if you calculate a house, the LPG needs to calculate the warm water needs to correctly calculate the electricity demand from the heat pump. " +
            "But maybe you don't need the warm water profiles and only want the electricity files. Then you can put Electricity here (case is important!) and the LPG will skip everything " +
            "in postprocessing that is not in this list. Leave this blank or delete the option entirely if you want all the result files.")]
        [ItemCanBeNull]
        public List<string>? LoadtypesForPostprocessing { get; set; } = new List<string>();


        [Comment(
            "Name for the calculation. This is not used in the calculation and is intended for the user to store comments or something like that.")]
        [CanBeNull]
        public string? CalculationName { get; set; } = null;

        [Comment(
            "List of all calculation output options to enable. This is ADDITIONALLY to the output files enabled by the DefaultForOutputFiles option!",
            ListPossibleOptions.ListCalcOptions)]
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<CalcOption>? CalcOptions { get; set; } = new List<CalcOption>();


        [Comment("This sets which output files are generated. You need to use one of the defaults. " +
                 "If you want some additional individual output, you can use the calc options list of individual settings to enable additional things.",
            ListPossibleOptions.ListOutputFileDefaults)]
        [JsonConverter(typeof(StringEnumConverter))]
        public OutputFileDefault DefaultForOutputFiles { get; set; } = OutputFileDefault.ReasonableWithChartsAndPDF;

        [Comment(
            "This option makes the LPG delete everything but the resulting PDF. This is pretty much only useful if you want " +
            "to generate a full set of PDFs for all households to get a detailed view of the results for each household. Default: false")]
        public bool DeleteAllButPDF { get; set; }

        [Comment("This option make the LPG delete all the DAT-files after the calculation. Default=true")]
        public bool DeleteDAT { get; set; } = true;

        [Comment("If you want the people to use certain devices, for example if you want to make sure that " +
                 "the people really use incandescent light bulbs, then you can set up a device selection to ensure that " +
                 "this type of device will always be selected.")]
        [CanBeNull]
        public JsonReference? DeviceSelection { get; set; }

        [Comment(
            "End date of the simulation. Defaults to the 31.12. of the current year if not set. One year maximum.")]
        [CanBeNull]
        public DateTime? EndDate { get; set; }

        [Comment(
            "How devices are picked for the households, for example if the household gets an old fridge or a new fridge.",
            ListPossibleOptions.EnergyIntensityTypes)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnergyIntensityType EnergyIntensityType { get; set; } = EnergyIntensityType.Random;

        [Comment(
            "If you need result files in 15 min resolution instead of 1 minute, then this option will help you. Set it to 00:15:00 to get 15 minute files. Needs to be a multiple of the internal time resolution, which is normally 1 minute.")]
        [CanBeNull]
        public string? ExternalTimeResolution { get; set; }

        [Comment(
            "If you need result files in 30 sekunds resolution instead of 1 minute, then this option will help you. Set it to 00:00:30 to get 30 second resolution files. " +
            "Note that the predefined device profiles are measured with a resolution of 1 minute, so you won't gain any accuracy, but it will save you the effort of interpolating the results yourself.")]
        [CanBeNull]
        public string? InternalTimeResolution { get; set; }
        [Comment("The guid of the geographic location to use. This determines holidays and sunrise/sunset times.")]
        [CanBeNull]
        public JsonReference? GeographicLocation { get; set; }

        [Comment(
            "Which load types should be included in the calculation. If you want to calculate a house, it is required to use at least the house-setting.",
            ListPossibleOptions.LoadTypePriorities)]
        [JsonConverter(typeof(StringEnumConverter))]
        public LoadTypePriority LoadTypePriority { get; set; } = LoadTypePriority.Undefined;

        [Comment("Path to the output directory where all the files will be put. Defaults to the current path.")]
        [CanBeNull]
        public string? OutputDirectory { get; set; }

        [Comment(
            "Sets the random seed. If two calculations with the same random seed are run, then the results will be identical. Defaults to -1, which means that it will be randomly selected.")]
        public int RandomSeed { get; set; } = -1;

        [Comment(
            "The LPG runs a 3-day period before the simulation start to initialize the people. For debugging purposes it is possible to include this in the result files. Defaults to false.")]
        public bool ShowSettlingPeriod { get; set; }

        [Comment(
            "If you enable this, the LPG will check in the result directory if this household/house was already calculated and if so, will quit quietly. Defaults to true.")]
        public bool SkipExisting { get; set; } = true;

        [Comment("Start date of the simulation. Defaults to the 01.01. of the current year if not set.")]
        [CanBeNull]
        public DateTime? StartDate { get; set; }

        [Comment(
            "Reference of the temperature profile to use. Defaults to the first temperature profile in the database if not set, which is probably not what you want. " +
            "Only the GUID is used to search the database. The name is ignored and only for human readability.")]
        [CanBeNull]
        public JsonReference? TemperatureProfile { get; set; }

        [Comment("This option make the LPG delete all the SQLite result files after the calculation. Only enable this if you really only want the load profiles and no further processing. Default=false")]
        public bool DeleteSqlite { get; set; }
        [Comment("When using household templates, sometimes random households are generated that don't work. With this option you can force the LPG to simulate at least some of the cases anyway. Default=false")]
        public bool IgnorePreviousActivitiesWhenNeeded { get; [UsedImplicitly] set; }

        public bool EnableTransportation { get; set; }
        [Comment("When using household templates, sometimes random households are generated that don't work. With this option you can force the LPG force to simulate all cases, no matter how messed up the definition is. Basically this enables a special activity \"Idle\" that always gets activated whenever the person can't find something to do. Default=false")]
        public bool EnableIdlemode { get; set; }


        [JetBrains.Annotations.NotNull]
        public static JsonCalcSpecification MakeDefaultsForTesting()
        {
            return new JsonCalcSpecification(false,null,new DateTime(2019,1,1),
                "00:15:00","00:01:00",null,LoadTypePriority.All,null,false,new DateTime(2019,1,1),null,false);
        }

        [JetBrains.Annotations.NotNull]
        public static JsonCalcSpecification MakeDefaultsForProduction()
        {
            return new JsonCalcSpecification( false,  null, new DateTime(2019, 12, 31),
                null, null, null, LoadTypePriority.All, null, false, new DateTime(2019, 1, 1),null, true);
        }
    }
}
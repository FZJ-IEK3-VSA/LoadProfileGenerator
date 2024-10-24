﻿using System.Diagnostics.CodeAnalysis;
using Autofac;
using CalcPostProcessor.GeneralHouseholdSteps;
using CalcPostProcessor.GeneralSteps;
using CalcPostProcessor.LoadTypeHouseholdSteps;
using CalcPostProcessor.LoadTypeProcessingSteps;
using CalcPostProcessor.Steps;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;

namespace CalcPostProcessor
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class PostProcessingManager
    {
        [JetBrains.Annotations.NotNull]
        private readonly ICalculationProfiler _calculationProfiler;
        [JetBrains.Annotations.NotNull]
        private readonly FileFactoryAndTracker _fft;

        public PostProcessingManager([JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler, [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft)
        {
            _calculationProfiler = calculationProfiler;
            _fft = fft;
        }

        public void Run([JetBrains.Annotations.NotNull] string resultPath)
        {
            try {
                _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Post Processing");
                var container = RegisterEverything(resultPath, _calculationProfiler, _fft);
                using (var scope = container.BeginLifetimeScope()) {
                    Postprocessor ps = scope.Resolve<Postprocessor>();
                    ps.RunPostProcessing();
                }
            }
            finally {
                _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Post Processing");
            }
        }

        [JetBrains.Annotations.NotNull]
        public static  IContainer RegisterEverything(string resultPath, [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler, [JetBrains.Annotations.CanBeNull] IFileFactoryAndTracker fft)
        {
            var builder = new ContainerBuilder();
            builder.Register(_ => new SqlResultLoggingService(resultPath)).As<SqlResultLoggingService>().SingleInstance();
            //builder.Register(c =>_logFile).As<ILogFile>().SingleInstance();
            builder.Register(_ => calculationProfiler).As<ICalculationProfiler>().SingleInstance();
            builder.Register(_ => fft).As<IFileFactoryAndTracker>().SingleInstance();
            builder.RegisterType<Postprocessor>().As<Postprocessor>().SingleInstance();
            builder.RegisterType<CalcDataRepository>().As<CalcDataRepository>().SingleInstance();
            builder.RegisterType<OptionDependencyManager>().SingleInstance();

            //general processing steps
            builder.RegisterType<AffordanceTagsWriter>().As<IGeneralStep>().SingleInstance();
            builder.RegisterType<DatFileDeletor>().As<IGeneralStep>().SingleInstance();

            //general household steps
            builder.RegisterType<MakeActivationsPerFrequencies>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<MakeActivationsPerHour>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<BodilyActivityLevelsStatistics>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<MakeHouseholdPlanResult>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<MakeBodilyActivityLevelStatistics>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<MakeFlexJsonFiles>().As<IGeneralHouseholdStep>().SingleInstance();
            //builder.RegisterType<ActionCarpetPlot>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<MakeActionsEachTimestep>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<LocationStatisticsMaker>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<AffordanceStatisticsWriter>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<TransportationDeviceJson>().As<IGeneralHouseholdStep>().SingleInstance();
            //builder.RegisterType<LocationCarpetPlot>().As<IGeneralHouseholdStep>().SingleInstance();
            //builder.RegisterType<TransportationDeviceCarpetPlot>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<TransportationStatisticsWriter>().As<IGeneralHouseholdStep>().SingleInstance();
            //builder.RegisterType<AffordanceStatisticsWriter>().As<IGeneralPostProcessingStep>().SingleInstance();
            //loadtypesteps
            builder.RegisterType<HouseSumProfileFromOverallDatProcessor>().As<ILoadTypeSumStep>().SingleInstance();
            builder.RegisterType<HouseJsonSumProfileProcessor>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<HouseSumProfilesFromDetailedDatsProcessor>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<IndividualHouseholdSumProfileProcessor>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<IndividualHouseholdDeviceProfileProcessor>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<IndividualHouseholdNoFlexSumProfileProcessor>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<IndividualHouseholdJsonSumProfileProcessor>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<IndividualHouseholdFlexJsonSumProfileProcessor>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<IndividualHouseholdDeviceProfileJsonProcessor>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<HouseDeviceProfileFileProcessor>().As<ILoadTypeStep>().SingleInstance();
            //builder.RegisterType<EnergyCarpetPlotMaker>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<ExternalTimeResolutionMaker>().As<ILoadTypeStep>().SingleInstance();
            //builder.RegisterType<ImportFileCreatorSMA>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<ImportFileCreatorPolysun>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<TimeOfUseMaker>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<MakeTotalsPerLoadtype>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<DurationCurveMaker>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<MakeTotalsPerDevice>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<WeekdayLoadProfileMaker>().As<ILoadTypeStep>().SingleInstance();

            //logger
            builder.RegisterType<InputDataLogger>().As<IInputDataLogger>().SingleInstance();
            builder.RegisterType<TotalsPerLoadtypeEntryLogger>().As<IDataSaverBase>().SingleInstance();
            builder.RegisterType<TotalsPerDeviceLogger>().As<IDataSaverBase>().SingleInstance();
            builder.RegisterType<BodilyActivityLevelStatisticsLogger>().As<IDataSaverBase>().SingleInstance();
            builder.RegisterType<PersonAffordanceInformationLogger>().As<IDataSaverBase>().SingleInstance();
            builder.RegisterType<SingleTimestepActionEntryLogger>().As<IDataSaverBase>().SingleInstance();
            builder.RegisterType<AffordanceEnergyUseLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationDeviceStatisticsLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationRouteStatisticsLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationDeviceEventStatisticsLogger>().As<IDataSaverBase>();
            var container = builder.Build();
            return container;
        }
    }
}
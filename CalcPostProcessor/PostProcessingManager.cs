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
using JetBrains.Annotations;

namespace CalcPostProcessor
{
    public class PostProcessingManager
    {
        [NotNull]
        private readonly ICalculationProfiler _calculationProfiler;
        [NotNull]
        private readonly FileFactoryAndTracker _fft;

        public PostProcessingManager([NotNull] ICalculationProfiler calculationProfiler, [NotNull] FileFactoryAndTracker fft)
        {
            _calculationProfiler = calculationProfiler;
            _fft = fft;
        }

        public void Run([NotNull] string resultPath)
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Post Processing");
            var builder = new ContainerBuilder();
            builder.Register(_ => new SqlResultLoggingService(resultPath)).As<SqlResultLoggingService>().SingleInstance();
            //builder.Register(c =>_logFile).As<ILogFile>().SingleInstance();
            builder.Register(_ => _calculationProfiler).As<ICalculationProfiler>().SingleInstance();
            builder.Register(_ => _fft).As<FileFactoryAndTracker>().SingleInstance();
            builder.RegisterType<Postprocessor>().As<Postprocessor>().SingleInstance();
            builder.RegisterType<CalcDataRepository>().As<CalcDataRepository>().SingleInstance();

            //general processing steps
            builder.RegisterType<AffordanceTagsWriter>().As<IGeneralStep>().SingleInstance();

            //general household steps
            builder.RegisterType<MakeActivationsPerFrequencies>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<MakeActivationsPerHour>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<MakeHouseholdPlanResult>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<ActionCarpetPlot>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<MakeActionsEachTimestep>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<LocationStatisticsMaker>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<AffordanceStatisticsWriter>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<LocationCarpetPlot>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<TransportationDeviceCarpetPlot>().As<IGeneralHouseholdStep>().SingleInstance();
            builder.RegisterType<TransportationStatisticsWriter>().As<IGeneralHouseholdStep>().SingleInstance();
            //builder.RegisterType<AffordanceStatisticsWriter>().As<IGeneralPostProcessingStep>().SingleInstance();

            //loadtypesteps
            builder.RegisterType<SumProfileProcessor>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<IndividualHouseholdSumProfileProcessor>().As<IHouseholdLoadTypeStep>().SingleInstance();
            builder.RegisterType<DeviceProfileFileProcessor>().As<ILoadTypeStep>().SingleInstance();
            builder.RegisterType<EnergyCarpetPlotMaker>().As<ILoadTypeStep>().SingleInstance();
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
            builder.RegisterType<TotalsEntryLogger>().As<IDataSaverBase>().SingleInstance();
            builder.RegisterType<PersonAffordanceInformationLogger>().As<IDataSaverBase>().SingleInstance();
            builder.RegisterType<SingleTimestepActionEntryLogger>().As<IDataSaverBase>().SingleInstance();
            builder.RegisterType<AffordanceEnergyUseLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationDeviceStatisticsLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationRouteStatisticsLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationDeviceEventStatisticsLogger>().As<IDataSaverBase>();

                        var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                Postprocessor ps = scope.Resolve<Postprocessor>();
                ps.RunPostProcessing();
            }
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Post Processing");
        }
    }
}
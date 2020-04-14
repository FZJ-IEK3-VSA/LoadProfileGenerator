using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation.ResultFiles;
using CalculationController.Integrity;
using Common;
using Common.Enums;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using PowerArgs;

namespace SimulationEngineLib.WebRunner {
    [UsedImplicitly]
    public static class WebRun {
        [NotNull]
        private static ModularHousehold CreateHousehold([NotNull] FileReader.AspHousehold hh, [NotNull] Simulator sim) {
            var chh = sim.ModularHouseholds.CreateNewItem(sim.ConnectionString);
            if (hh.Name == null) {
                throw new LPGException("Household was null");
            }
            chh.Name = hh.Name;
            chh.Description = hh.AspID.ToString(CultureInfo.CurrentCulture);
            var vac = sim.Vacations.CreateNewItem(sim.ConnectionString);
            vac.AddVacationTime(hh.VacationStart, hh.VacationEnd, VacationType.GoAway);
            vac.SaveToDB();
            chh.Vacation = vac;
            chh.SaveToDB();
            foreach (var aspPerson in hh.Persons) {
                var p = sim.Persons.CreateNewItem(sim.ConnectionString);
                if (aspPerson.Name == null) {
                    throw new LPGException("Name was null");
                }
                p.Name = aspPerson.Name;
                p.Age = aspPerson.Age;
                if (aspPerson.Gender == 0) {
                    p.Gender = PermittedGender.Male;
                }
                else {
                    p.Gender = PermittedGender.Female;
                }
                p.SickDays = aspPerson.Sickdays;
                p.SaveToDB();
                TraitTag tag = sim.TraitTags.FindFirstByName(aspPerson.TraitTag);
                if(tag == null) {
                    throw new LPGException("Trait tag " + aspPerson.TraitTag + " was not found.");
                }
                chh.AddPerson(p, tag);
                foreach (var aspTrait in aspPerson.Traits) {
                    var trait = sim.HouseholdTraits.It.FirstOrDefault(x => x.PrettyName == aspTrait.Name);
                    if (trait == null) {
                        throw new LPGException("Trait not found:" + aspTrait.Name);
                    }
                    chh.AddTrait(trait, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name, p);
                }
            }
            sim.MyGeneralConfig.PerformCleanUpChecks = "False";
            SimIntegrityChecker.Run(sim);
            return chh;
        }

        [NotNull]
        private static Simulator MakeDatabase() {
            const string connectionString = "Data Source=profilegenerator.db3";
            var sim = new Simulator(connectionString);
            return sim;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void Run([NotNull] WebRunOptions o) {
            if(o.Directory == null) { throw new LPGException("Directory was null");}
            var calculationStartTime = DateTime.Now;
            try {
                Logger.LogFileIndex = "";
                var di = new DirectoryInfo(o.Directory);
                var fis = di.GetFiles("calcjob.txt");
                if (fis.Length > 1) {
                    throw new LPGException("more than one calc job file found.");
                }
                if (fis.Length == 0) {
                    throw new LPGException("no calc job file found.");
                }
                var hh = FileReader.ReadMyTextFile(fis[0].FullName);
                var sim = MakeDatabase();
                var chh = CreateHousehold(hh, sim);
                WebCalculationStarter.StartHousehold(sim, chh, di.FullName, hh);
            }
            catch (DataIntegrityException dix) {
                Logger.Error(dix.Message);
                Logger.LogFileIndex = "DataIntegrityException";
                Logger.Error(dix.Message);
                Logger.LogFileIndex = "DetailedError";
                Logger.Exception(dix);
                Logger.LogFileIndex = "";
            }
            catch (Exception ex) {
                Logger.Error(ex.Message);
                Logger.LogFileIndex = "Error";
                Logger.Error(ex.Message);
                Logger.LogFileIndex = "DetailedError";
                Logger.Exception(ex);
            }
            finally {
                var duration = DateTime.Now - calculationStartTime;
                Logger.Info("Calculation duration:" + duration);

                var finishedFile = Path.Combine(o.Directory, Constants.FinishedFileFlag);
                using (var sw = new StreamWriter(finishedFile)) {
                    sw.WriteLine("Finished at "+ Environment.NewLine + DateTime.Now + Environment.NewLine+" after "+ Environment.NewLine + duration);
                }
            }
        }

        [UsedImplicitly]
        public class WebRunOptions {
            [CanBeNull]
            [ArgDescription("Sets the directory to use")]
            [ArgShortcut(null)]
            public string Directory { get; set; }
        }
    }
}
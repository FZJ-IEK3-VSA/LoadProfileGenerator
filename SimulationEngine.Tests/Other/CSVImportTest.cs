using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Tables.BasicElements;
using Database.Tests;
using NUnit.Framework;
using SimulationEngineLib.Other;

namespace SimulationEngine.Tests.Other
{
    public class CSVImportTest
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            //make dummy csv file
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                string csvfn = wd.Combine("mycsv.csv");
                StreamWriter sw = new StreamWriter(csvfn);
                sw.WriteLine("Date;Value");
                sw.WriteLine("01.01.2019 00:00;1");
                sw.WriteLine("01.01.2019 00:01;2");
                sw.Close();
                CsvImportOptions cio = new CsvImportOptions();
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    cio.Input = csvfn;
                    cio.Delimiter = ";";
                    CsvTimeProfileImporter ctpi = new CsvTimeProfileImporter(db.ConnectionString);
                    if (!ctpi.Import(cio, out var dbp))
                    {
                        throw new LPGCommandlineException("Option not set. Cannnot proceed.");
                    }
                    if (dbp == null)
                    {
                        throw new LPGException("failed to import");
                    }

                    foreach (DateProfileDataPoint dataPoint in dbp.Datapoints)
                    {
                        Logger.Info(dataPoint.DateAndTimeString + " - " + dataPoint.Value);
                    }
                }
            }

        }
    }
}

using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace ReleaseMaker {
    public class SimEngineCopier : CopierBase {
        [NotNull]
        public static List<string> CopySimEngineFiles([NotNull] string src, [NotNull] string dst)
        {
            List<string> programFiles = new List<string>();
            Copy(programFiles, src, dst, "Autofac.dll");
            Copy(programFiles, src, dst, "Automation.dll");
            Copy(programFiles, src, dst, "CalcPostProcessor.dll");
            Copy(programFiles, src, dst, "CalculationController.dll");
            Copy(programFiles, src, dst, "CalculationEngine.dll");
            Copy(programFiles, src, dst, "ChartCreator2.dll");
            Copy(programFiles, src, dst, "ChartPDFCreator.dll");
            Copy(programFiles, src, dst, "Common.dll");
            Copy(programFiles, src, dst, "Database.dll");
            Copy(programFiles, src, dst, "SimulationEngineLib.dll");
            Copy(programFiles, src, dst, "System.Buffers.dll");
            Copy(programFiles, src, dst, "SimulationEngine.exe.config");
            Copy(programFiles, src, dst, "System.Memory.dll");
            Copy(programFiles, src, dst, "System.Numerics.Vectors.dll");
            Copy(programFiles, src, dst, "System.Resources.Extensions.dll");
            Copy(programFiles, src, dst, "System.Runtime.CompilerServices.Unsafe.dll");
            Copy(programFiles, src, dst, "MigraDoc.DocumentObjectModel-gdi.dll");
            string desrc = Path.Combine(src, "de");
            Copy(programFiles, desrc, dst, "MigraDoc.DocumentObjectModel-gdi.resources.dll");
            Copy(programFiles, desrc, dst, "MigraDoc.Rendering-gdi.resources.dll");
            Copy(programFiles, desrc, dst, "MigraDoc.RtfRendering-gdi.resources.dll");
            Copy(programFiles, desrc, dst, "PdfSharp-gdi.resources.dll");
            Copy(programFiles, desrc, dst, "PdfSharp.Charting-gdi.resources.dll");
            Copy(programFiles, src, dst, "MigraDoc.Rendering-gdi.dll");
            Copy(programFiles, src, dst, "MigraDoc.RtfRendering-gdi.dll");
            Copy(programFiles, src, dst, "PdfSharp-gdi.dll");
            Copy(programFiles, src, dst, "PdfSharp.Charting-gdi.dll");
            Copy(programFiles, src, dst, "Newtonsoft.Json.dll");
            Copy(programFiles, src, dst, "OxyPlot.dll");
            Copy(programFiles, src, dst, "OxyPlot.Pdf.dll");
            Copy(programFiles, src, dst, "OxyPlot.Wpf.dll");
            Copy(programFiles, src, dst, "SQLite.Interop.dll");
            Copy(programFiles, src, dst, "System.Data.SQLite.dll");
            Copy(programFiles, src, dst, "SimulationEngine.exe");
            Copy(programFiles, src, dst, "PowerArgs.dll");
            Copy(programFiles, src, dst, "EntityFramework.dll");
            Copy(programFiles, src, dst, "EntityFramework.SqlServer.dll");
            Copy(programFiles, src, dst, "JetBrains.Annotations.dll");
            Copy(programFiles, src, dst, "System.Data.SQLite.EF6.dll");
            Copy(programFiles, src, dst, "System.Data.SQLite.Linq.dll");
            Copy(programFiles, src, dst, "Microsoft.Bcl.AsyncInterfaces.dll");
            Copy(programFiles, src, dst, "System.Threading.Tasks.Extensions.dll");
            Copy(programFiles, src, dst, "xunit.abstractions.dll");
            Copy(programFiles, src, dst, "System.ValueTuple.dll");
            Copy(programFiles, src, dst, "Utf8Json.dll");
            CheckIfFilesAreCompletelyCopied(src, programFiles);
            return programFiles;
        }
    }
}
using Automation.ResultFiles;
using JetBrains.Annotations;
using OxyPlot;

namespace ChartCreator2
{
    public static class OxyPDFCreator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1712:DoNotPrefixEnumValuesWithTypeName")]
        public enum HeightWidth
        {
            HeightWidth1610,
            HeightWidth167,
            HeightWidth165
        }

        public static void Run([NotNull] PlotModel plotModel1, [NotNull] string pdfChartName, HeightWidth hw = HeightWidth.HeightWidth1610)
        {
            switch (hw)
            {
                case HeightWidth.HeightWidth1610:
                    OxyPlot.SkiaSharp.PdfExporter.Export(plotModel1, pdfChartName, 1600, 1000);
                    break;
                case HeightWidth.HeightWidth167:
                    OxyPlot.SkiaSharp.PdfExporter.Export(plotModel1, pdfChartName, 1600, 700);
                    break;
                case HeightWidth.HeightWidth165:
                    OxyPlot.SkiaSharp.PdfExporter.Export(plotModel1, pdfChartName, 1600, 500);
                    break;
                default:
                    throw new LPGException("Forgotten HeightWidth");
            }
        }
    }
}
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

//#region

//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using Automation;
//using Automation.ResultFiles;
//using CalcPostProcessor.Helper;
//using CalcPostProcessor.Steps;
//using Common;
//using Common.CalcDto;
//using Common.JSON;
//using Common.SQLResultLogging;
//using JetBrains.Annotations;

//#endregion

//namespace CalcPostProcessor.LoadTypeProcessingSteps {
//    public class EnergyCarpetPlotMaker: LoadTypeStepBase
//    {
//        [JetBrains.Annotations.NotNull]
//        private readonly CalcParameters _calcParameters;
//        [JetBrains.Annotations.NotNull]
//        private readonly FileFactoryAndTracker _fft;

//        public EnergyCarpetPlotMaker([JetBrains.Annotations.NotNull] CalcDataRepository repository,
//                                     [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
//                                     [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft): base(repository,
//            AutomationUtili.GetOptionList(CalcOption.EnergyCarpetPlot),calculationProfiler,"Energy Carpet Plots")
//        {
//            _calcParameters = Repository.CalcParameters;
//            _fft = fft;
//        }

//        [JetBrains.Annotations.NotNull]
//        private RenderTargetBitmap AddAxisTexts([JetBrains.Annotations.NotNull] BitmapSource bmp, int numberofStepsPerDay, int colWidth,
//            [JetBrains.Annotations.NotNull] BitmapPreparer legendBmp, double min, double max, [JetBrains.Annotations.NotNull] string unit) {
//            var bmpSource = bmp;
//            var drawingVisual = new DrawingVisual();
//            var drawingContext = drawingVisual.RenderOpen();
//            const int xoffset = 80;
//            const int yspace = 250;
//            var targetwidth = (int) bmp.Width + xoffset;
//            if (targetwidth < legendBmp.Width + xoffset) {
//                targetwidth = legendBmp.Width + xoffset + 100;
//            }
//            var targetheight = (int) bmp.Height + yspace;

//            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, targetwidth, targetheight));
//            drawingContext.DrawRectangle(Brushes.Black, null, new Rect(xoffset - 1, 0, bmp.Width + 2, bmp.Height + 2));
//            drawingContext.DrawImage(bmpSource, new Rect(xoffset, 1, bmp.Width, bmp.Height));

//            var legendYStart = targetheight - 100;
//            const int legendXStart = xoffset;
//            drawingContext.DrawRectangle(Brushes.Black, null,
//                new Rect(legendXStart, targetheight - 100, legendBmp.Width + 1, legendBmp.Height + 1));
//            drawingContext.DrawImage(legendBmp.GetBitmap(),
//                new Rect(legendXStart + 1, targetheight - 100, legendBmp.Width, legendBmp.Height));
//            const int fontsize = 24;
//            var c = Color.FromRgb(0, 0, 0);
//            Brush b = new SolidColorBrush(c);
//            var tfarr = new Typeface[Fonts.SystemTypefaces.Count];
//            Fonts.SystemTypefaces.CopyTo(tfarr, 0);
//            var tf = tfarr[0];
//            // legend
//            const int legendSteps = 5;
//            var legendStep = (max - min) / legendSteps;
//            var legendCurr = min;
//#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
//            var legendPositionStep = legendBmp.Width / legendSteps;
//#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
//            var legendCurPosition = legendXStart;
//            var formatString = "N1";
//            if (max - min > 100) {
//                formatString = "N0";
//            }
//            for (var i = 0; i < legendSteps + 1; i++) {
//                var text =
//                    new FormattedText("|" + legendCurr.ToString(formatString, Config.CultureInfo) + " " + unit,
//                        new CultureInfo("en-us"), FlowDirection.LeftToRight, tf, fontsize, b,1);
//                drawingContext.DrawText(text, new Point(legendCurPosition - 5, legendYStart + 30));
//                legendCurr += legendStep;
//                legendCurPosition += legendPositionStep;
//            }

//            // time
//            var oneHour = (int) (numberofStepsPerDay / 24.0);
//            for (var i = 0; i < 24; i += 2) {
//                var text = new FormattedText(i + ":00", new CultureInfo("en-us"), FlowDirection.LeftToRight,
//                    tf, fontsize, b,1);
//                drawingContext.DrawText(text, new Point(2, oneHour * i));
//            }
//            var starttime = _calcParameters.OfficialStartTime;
//            var endTime = _calcParameters.OfficialEndTime;
//            var rotateTransform = new RotateTransform
//            {
//                Angle = -90
//            };
//            drawingContext.PushTransform(rotateTransform);
//            var dateCount = 0;
//            while (starttime < endTime) {
//                var text = new FormattedText(starttime.ToShortDateString(), new CultureInfo("en-us"),
//                    FlowDirection.LeftToRight, tf, fontsize, b,1);
//                double normalxPos = xoffset + dateCount * colWidth;
//                var normalyPos = bmp.Height + text.Width + 10;
//                var newxPos = normalyPos * -1;
//                var newypos = normalxPos;
//                drawingContext.DrawText(text, new Point(newxPos, newypos));
//                dateCount += 10;
//                starttime = starttime.AddDays(10);
//            }
//            drawingContext.Pop();
//            drawingContext.Close();
//            var rendertargetbmp = new RenderTargetBitmap(targetwidth, targetheight, 96, 96,
//                PixelFormats.Pbgra32);
//            rendertargetbmp.Render(drawingVisual);
//            return rendertargetbmp;
//        }

//        private TimeSpan CalculateTime(bool displayNegativeTime) {
//            TimeSpan ts;
//#pragma warning disable RCS1179 // Use return instead of assignment.
//            if (displayNegativeTime) {
//                ts = _calcParameters.InternalEndTime -
//                     _calcParameters.InternalStartTime;
//            }
//            else {
//                ts = _calcParameters.InternalEndTime -
//                     _calcParameters.OfficialStartTime;
//            }
//#pragma warning restore RCS1179 // Use return instead of assignment.
//            return ts;
//        }

//        private void MakeCarpet([JetBrains.Annotations.NotNull] FileStream fs, [JetBrains.Annotations.NotNull] double[] energySumValues,
//            TimeSpan totalDuration, [JetBrains.Annotations.NotNull] string ltName, int colwidth, [JetBrains.Annotations.NotNull] string unit) {
//            if (Math.Abs(_calcParameters.InternalStepsize.TotalSeconds) < Constants.Ebsilon) {
//                throw new LPGException("InnerStepSize was not initialized");
//            }
//            var numberofStepsPerDay =
//                (int)
//                (new TimeSpan(1, 0, 0, 0).TotalSeconds / _calcParameters.InternalStepsize
//                     .TotalSeconds);
//            if (numberofStepsPerDay > 4000) {
//                Logger.Error(
//                    "Couldn't create the carpet plot because the time resolution had more than 4000 steps per day.");
//                return;
//            }
//            var row = 0;
//            var col = 0;
//            Logger.Info("Setting the pixels for " + ltName + "...");
//            using (var bmp = new BitmapPreparer((int) (totalDuration.TotalDays +1)* colwidth, numberofStepsPerDay)) {
//                var max = double.MinValue;
//                var min = double.MaxValue;
//                for (var i = 0; i < energySumValues.Length; i++) {
//                    if (max < energySumValues[i]) {
//                        max = energySumValues[i];
//                    }
//                    if (min > energySumValues[i]) {
//                        min = energySumValues[i];
//                    }
//                }
//                //var trgb = new ColorRGB {R = 255, B = 0, G = 0};
//                //RGBHSL.RGB2HSL(trgb, out _, out _, out _);
//                //trgb.B = 255;
//                //RGBHSL.RGB2HSL(trgb, out _, out _, out _);
//                //trgb.G = 255;
//                //RGBHSL.RGB2HSL(trgb, out _, out _, out _);
//                var offset = min;
//                // ordinarly 0 should be the minium
//                if (offset > 0) {
//                    offset = 0;
//                }
//                // if the values are negative, shift everything to above zero
//                if (offset < 0) {
//                    offset *= -1;
//                }
//                var range = max - min;
//                for (var idx = 0; idx < energySumValues.Length; idx++) {
//                    row++;
//                    if (row == numberofStepsPerDay) {
//                        row = 0;
//                        col++;
//                    }
//                    var hval = (offset + energySumValues[idx]) / range * 0.9;
//                    var crgb = RGBHSL.HSL2RGB(hval, 0.5, 0.5);
//                    Color c;
//                    if (Math.Abs(energySumValues[idx]) < Constants.Ebsilon) {
//                        c = Color.FromRgb(255, 255, 255);
//                    }
//                    else {
//                        c = Color.FromRgb(crgb.R, crgb.G, crgb.B);
//                    }
//                    for (var colwidthIdx = 0; colwidthIdx < colwidth; colwidthIdx++) {
//                        bmp.SetPixel(col * colwidth + colwidthIdx, row, c);
//                    }
//                }
//                Logger.Info("Saving the carpet plot for " + ltName + "...");
//                using (var legendbmp = new BitmapPreparer(255 * 5 + 1, 30)) {
//                    MakeCarpetLegend(legendbmp, energySumValues, 5);
//                    var renderbmp = AddAxisTexts(bmp.GetBitmap(), numberofStepsPerDay, colwidth,
//                        legendbmp, min, max, unit);
//                    var encoder = new PngBitmapEncoder();
//                    encoder.Frames.Add(BitmapFrame.Create(renderbmp));
//                    encoder.Save(fs);
//                }
//            }
//            GC.Collect();
//        }

//        private void MakeCarpetLegend([JetBrains.Annotations.NotNull] BitmapPreparer bmp, [JetBrains.Annotations.NotNull] double[] energySumValues, int repeaterLength) {
//            var max = energySumValues.Max();
//            var min = energySumValues.Min();
//            //var trgb = new ColorRGB {R = 255, B = 0, G = 0};
//            //RGBHSL.RGB2HSL(trgb, out _, out _, out _);
//            //trgb.B = 255;
//            //RGBHSL.RGB2HSL(trgb, out _, out _, out _);
//            //trgb.G = 255;
//            //RGBHSL.RGB2HSL(trgb, out _, out _, out _);
//            var range = max - min;
//            var step = range / 255;
//            var currVal = min;
//            var colorCol = 0;
//            for (var idx = 0; idx < 255; idx++) {
//                var hval = (currVal - min) / range * 0.9;
//                var crgb = RGBHSL.HSL2RGB(hval, 0.5, 0.5);
//                Color c;
//                if (Math.Abs(currVal) < Constants.Ebsilon && Math.Abs(min) < Constants.Ebsilon) {
//                    c = Color.FromRgb(255, 255, 255);
//                }
//                else {
//                    c = Color.FromRgb(crgb.R, crgb.G, crgb.B);
//                }
//                currVal += step;
//                for (var colwidthIdx = 1; colwidthIdx < bmp.Height; colwidthIdx++) {
//                    for (var i = 0; i < repeaterLength; i++) {
//                        bmp.SetPixel(colorCol + i, colwidthIdx, c);
//                    }
//                }
//                colorCol += repeaterLength;
//            }
//            Logger.Info("Finished the carpet plot legend.");
//        }

//        private void Run([JetBrains.Annotations.NotNull] CalcLoadTypeDto dstLoadType, [JetBrains.Annotations.NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
//            [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, bool displayNegativeTime, int columnWidth) {
//            if (!dstLoadType.ShowInCharts) {
//                return;
//            }
//            var energySumValues = new double[_calcParameters.OfficalTimesteps];
//            if (_calcParameters.ShowSettlingPeriodTime) {
//                energySumValues = new double[ _calcParameters.InternalTimesteps];
//            }

//            int idx = 0;
//            foreach (var efr in energyFileRows) {
//                if (efr.Timestep.DisplayThisStep) {
//                    energySumValues[idx++] += efr.SumCached;
//                }
//            }
//            var ts = CalculateTime(displayNegativeTime);
//            var carpetplotcolumnWidth = columnWidth;
//            using (
//                var fs = fft.MakeFile<FileStream>(
//                    "EnergyCarpetplot." + dstLoadType.Name + "." + carpetplotcolumnWidth + ".png",
//                    "Carpet plot of " + dstLoadType.Name + " " + 15 + " pixel column width", true,
//                    ResultFileID.CarpetPlotsEnergy + 0, Constants.GeneralHouseholdKey, TargetDirectory.Charts, _calcParameters.InternalStepsize,
//                    dstLoadType.ConvertToLoadTypeInformation())) {
//                MakeCarpet(fs, energySumValues, ts, dstLoadType.Name, carpetplotcolumnWidth, dstLoadType.UnitOfPower);
//            }
//        }

//        protected override void PerformActualStep(IStepParameters parameters)
//        {
//            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
//            Run(p.LoadType,p.EnergyFileRows,_fft,false,Repository.CarpetPlotColumnWidth);
//        }
//    }
//}
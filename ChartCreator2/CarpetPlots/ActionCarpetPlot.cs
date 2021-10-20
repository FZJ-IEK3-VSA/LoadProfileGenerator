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

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Automation.ResultFiles;
using ChartCreator2.Steps;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using SkiaSharp;

#endregion

namespace ChartCreator2.CarpetPlots
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class ActionCarpetPlot : HouseholdStepBase
    {

        [JetBrains.Annotations.NotNull] private readonly ICalculationProfiler _calculationProfiler;

        private readonly int _columnWidth;

        [JetBrains.Annotations.NotNull] private readonly IFileFactoryAndTracker _fft;

        [JetBrains.Annotations.NotNull] private readonly CalcDataRepository _repository;

        public ActionCarpetPlot([JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                [JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                [JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft
        ) : base(repository, AutomationUtili.GetOptionList(CalcOption.ActionCarpetPlot),
            calculationProfiler,
            "Action Carpet Plot",1)
        {
            _calculationProfiler = calculationProfiler;
            _columnWidth = repository.CarpetPlotColumnWidth;
            _repository = repository;
            _fft = fft;
        }

        protected override void PerformActualStep([JetBrains.Annotations.NotNull] IStepParameters parameters)
        {
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            var entry = hhp.Key;
            if (entry.KeyType != HouseholdKeyType.Household)
            {
                return;
            }

            var householdKey = entry.HHKey;

            Dictionary<string, SKColor> affordanceColorDict = new Dictionary<string, SKColor>();
            var affordances = _repository.LoadAffordances(householdKey);
            if (affordances.Count == 0)
            {
                throw new LPGException("Not a single affordance was found");
            }

            foreach (CalcAffordanceDto affordance in affordances)
            {
                string name = affordance.Name;
                SkiaSharp.SKColor c =
                    new SKColor(affordance.ColorR, affordance.ColorG, affordance.ColorB);
                if (affordanceColorDict.ContainsKey(name))
                {
                    continue;
                }

                affordanceColorDict.Add(name, c);
                foreach (CalcSubAffordanceDto subAffordanceDto in affordance.SubAffordance)
                {
                    if (!affordanceColorDict.ContainsKey(subAffordanceDto.Name))
                    {
                        affordanceColorDict.Add(subAffordanceDto.Name, c);
                    }
                }
            }

            // action carpet plot
            // var affordanceColorDict = GetAffordanceColorDict();
            if (!affordanceColorDict.ContainsKey(Constants.TakingAVacationString))
            {
                affordanceColorDict.Add(Constants.TakingAVacationString,new  SKColor(0, 255, 0)); }

            //ActionCarpetPlot ap = new ActionCarpetPlot(_calculationProfiler, Repository);
            Run(_fft, affordanceColorDict, householdKey, Repository.AffordanceTaggingSets,
                Repository.GetPersons(householdKey));
        }

        public override List<CalcOption> NeededOptions { get; } = new List<CalcOption> {
            CalcOption.DeviceActivations, CalcOption.HouseholdContents, CalcOption.AffordanceDefinitions,
            CalcOption.ActionEntries
        };

        /*[JetBrains.Annotations.NotNull]
        private RenderTargetBitmap AddAxisTexts([JetBrains.Annotations.NotNull] BitmapSource bmp, int numberofStepsPerDay, int colWidth,
                                                [CanBeNull] CalcAffordanceTaggingSetDto taggingSet)
        {
            const int fontsize = 22;
            var bmpSource = bmp;
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            const int xOffset = 70;
            var ySpace = 150;
            if (taggingSet != null)
            {
                ySpace += 30;
            }

            var targetwidth = (int)bmp.Width + xOffset;
            var targetheight = (int)bmp.Height + ySpace;
            var whiteBrush = new SolidColorBrush(Color.FromRgb(255,255,255));
            drawingContext.DrawRectangle(whiteBrush, null, new Rect(0, 0, targetwidth, targetheight));
            drawingContext.DrawImage(bmpSource, new Rect(xOffset, 0, bmp.Width, bmp.Height));
            var c = Color.FromArgb(0,0, 0, 0);
            Brush b = new SolidColorBrush(c);
            var tfarr = new Typeface[Fonts.SystemTypefaces.Count];
            Fonts.SystemTypefaces.CopyTo(tfarr, 0);
            var typeFace = tfarr.First(x => x.FontFamily.Source == "Arial");
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
            var oneHour = numberofStepsPerDay / 24;
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
            for (var i = 0; i < 24; i += 2)
            {
                var text = new System.Windows.Media.FormattedText(i + ":00", new CultureInfo("en-us"), FlowDirection.LeftToRight,
                    typeFace, fontsize, b, 1);
                drawingContext.DrawText(text, new Point(2, oneHour * i));
            }

            var starttime = _calcParameters.OfficialStartTime;
            var endTime = _calcParameters.OfficialEndTime;
            var rotateTransform = new RotateTransform
            {
                Angle = -90
            };
            drawingContext.PushTransform(rotateTransform);
            var dateCount = 0;
            double textwidth = 0;
            while (starttime < endTime)
            {
                var text = new System.Windows.Media.FormattedText(starttime.ToShortDateString(), new CultureInfo("en-us"),
                    FlowDirection.LeftToRight, typeFace, fontsize, b, 1);
                textwidth = text.Width;
                double normalxPos = xOffset + dateCount * colWidth;
                var normalyPos = bmp.Height + text.Width + 10;
                var newxPos = normalyPos * -1;
                var newypos = normalxPos;
                drawingContext.DrawText(text, new Point(newxPos, newypos));
                dateCount += (int)(starttime.AddMonths(1) - starttime).TotalDays;
                starttime = starttime.AddMonths(1);
            }

            drawingContext.Pop();
            var square = Convert.ToChar(9632).ToString();
            if (taggingSet != null)
            {
                double xpos = xOffset;
                var ypos = bmp.Height + textwidth + 15;
                if (!taggingSet.Colors.ContainsKey("Vacation"))
                {
                    taggingSet.Colors.Add("Vacation", new ColorRGB(2, 200, 200));
                }

                var colorList = taggingSet.Colors.Where(x => x.Key != "none").Select(x =>
                {
                    var transText = ChartLocalizer.Get().GetTranslation(x.Key);
                    return new Tuple<string, ColorRGB>(transText, x.Value);
                }).ToList();
                colorList.Sort((x, y) => string.Compare(x.Item1, y.Item1, StringComparison.Ordinal));
                foreach (var color in colorList)
                {
                    Color thisColor = Color.FromArgb(0,color.Item2.R, color.Item2.G, color.Item2.B);
                    Brush colorBrush =new SolidColorBrush(thisColor);
                    var squaretext = new FormattedText(square, new CultureInfo("en-us"),
                        FlowDirection.LeftToRight, typeFace, 75, colorBrush, 1);
                    drawingContext.DrawText(squaretext, new Point(xpos, ypos - 25));
                    xpos += squaretext.Width + 5;
                    var text = new FormattedText(color.Item1, new CultureInfo("en-us"),
                        FlowDirection.LeftToRight, typeFace, fontsize, b, 1);
                    drawingContext.DrawText(text, new Point(xpos, ypos + 15));
                    xpos += text.Width + 15;
                }
            }

            drawingContext.Close();
            var rendertargetbmp = new RenderTargetBitmap(targetwidth, targetheight, 96, 96,
                PixelFormats.Pbgra32);
            rendertargetbmp.Render(drawingVisual);
            return rendertargetbmp;
        }*/

        /*
        [JetBrains.Annotations.NotNull]
        private static RenderTargetBitmap AddDirectTexts([JetBrains.Annotations.NotNull] BitmapSource bmp,
                                                         [JetBrains.Annotations.NotNull] Dictionary<Point, string> descriptionDict,
                                                         int colwidth)
        {
            var bmpSource = bmp;
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            const int factor = 2;
            var targetwidth = (int)bmp.Width * factor;
            var targetheight = (int)bmp.Height * factor;
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, targetwidth, targetheight));
            drawingContext.DrawImage(bmpSource, new Rect(0, 0, targetwidth, targetheight));

            var c = Color.FromArgb(0,255, 255, 255);
            Brush b = new SolidColorBrush(c);
            var tfarr = new Typeface[Fonts.SystemTypefaces.Count];
            Fonts.SystemTypefaces.CopyTo(tfarr, 0);
            var tf = tfarr[0];
            var rotateTransform = new RotateTransform
            {
                Angle = 90
            };
            drawingContext.PushTransform(rotateTransform);
            var position = 0;
            foreach (var keyValuePair in descriptionDict)
            {
                var text = new FormattedText(keyValuePair.Value, new CultureInfo("en-us"),
                    FlowDirection.LeftToRight, tf, 8.0, b, 1);
                var newxPos = keyValuePair.Key.Y * factor;
                double shift = 4 * position;
                position++;
                if (position == 3)
                {
                    position = 0;
                }

                int newypos = (int)((keyValuePair.Key.X - shift + colwidth - 2) * factor * -1);

                drawingContext.DrawText(text, new System.Windows.Point(newxPos, newypos));
            }

            drawingContext.Pop();
            drawingContext.Close();
            var rendertargetbmp = new RenderTargetBitmap(targetwidth * 2, targetheight * 2, 150, 150,
                PixelFormats.Pbgra32);
            rendertargetbmp.Render(drawingVisual);
            return rendertargetbmp;
        }*/

        private TimeSpan CalculateTime(bool displayNegativeTime)
        {
            if (displayNegativeTime)
            {
                return _repository.CalcParameters.InternalEndTime -
                       _repository.CalcParameters.InternalStartTime;
            }

            return _repository.CalcParameters.InternalEndTime -
                   _repository.CalcParameters.OfficialStartTime;
        }

        //private static int Comparison([JetBrains.Annotations.NotNull] Tuple<string, Color> t1, [JetBrains.Annotations.NotNull] Tuple<string, Color> t2)
        //{
        //    var b1 = 0.2126 * t1.Item2.R + 0.7152 * t1.Item2.G + 0.0722 * t1.Item2.B;
        //    var b2 = 0.2126 * t2.Item2.R + 0.7152 * t2.Item2.G + 0.0722 * t2.Item2.B;
        //    return b1.CompareTo(b2);
        //}

        private void MakeCarpet([JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft,
                                [JetBrains.Annotations.NotNull] Dictionary<string, SKColor> affordanceColorDict,
                                [JetBrains.Annotations.NotNull][ItemNotNull] TimeActionTuple[] times,
                                [JetBrains.Annotations.NotNull] CalcPersonDto person, TimeSpan ts, [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                int colwidth, [CanBeNull] CalcAffordanceTaggingSetDto taggingSet)
        {
            var numberofStepsPerDay =
                (int)
                (new TimeSpan(1, 0, 0, 0).TotalSeconds / _repository.CalcParameters.InternalStepsize
                     .TotalSeconds);
            if (numberofStepsPerDay > 40000)
            {
                Logger.Error("No carpet plots for time resolutions with more than 40.000 steps per day can be created!");
                return;
            }

            var row = 0;
            var col = 0;
            Logger.Info("Setting the pixels for " + person.Name + "...");

            //var descriptionDict = new Dictionary<Point, string>();
            using (var bmp = new BitmapPreparer((int)(ts.TotalDays + 1) * colwidth, numberofStepsPerDay))
            {
                //var prevAction = string.Empty;
                foreach (var time in times)
                {
                    row++;
                    if (row == numberofStepsPerDay)
                    {
                        row = 0;
                        col++;
                    }

                    SKColor c;
                    if (time.Ae == null)
                    {
                        throw new LPGException("time.ae was null");
                    }

                        //descriptionDict.Add(new Point(col * colwidth, row), time.Ae.AffordanceName);
                        //prevAction = time.Ae.AffordanceName;

                    if (taggingSet == null)
                    {
                        if (affordanceColorDict.ContainsKey(time.Ae.AffordanceName))
                        {
                            c = affordanceColorDict[time.Ae.AffordanceName];
                        }
                        else
                        {
                            c = new SKColor(2, 200, 200);
                        }
                    }
                    else
                    {
                        SKColor? c2 = null;

                        if (taggingSet.ContainsAffordance(time.Ae.AffordanceName))
                        {
                            var tag = taggingSet.GetAffordanceTag(time.Ae.AffordanceName);
                            if (taggingSet.Colors.ContainsKey(tag))
                            {
                                var tc = taggingSet.Colors[tag];
                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                                if (tc != null)
                                {
                                    c2 = new SKColor(tc.R, tc.G, tc.B);
                                }
                            }
                        }

                        if (c2 == null)
                        {
                            c2 = new SKColor(2, 200, 200);
                        }

                        c = (SKColor)c2;
                    }

                    for (var colwidthIdx = 0; colwidthIdx < colwidth; colwidthIdx++)
                    {
                        bmp.SetPixel(col * colwidth + colwidthIdx, row, c,true);
                    }

                    // add little white line for sickness
                    if (time.Ae.IsSick)
                    {
                        c = new SKColor(255, 255, 255);
                        bmp.SetPixel(col * colwidth + 0, row, c,false);
                        bmp.SetPixel(col * colwidth + 1, row, c,false);
                        bmp.SetPixel(col * colwidth + colwidth - 1, row, c,false);
                        //bmp.SetPixel(col * colwidth + colwidth, row, c, false);
                    }
                }

                //var renderbmp = AddAxisTexts(bmp.GetBitmap(), numberofStepsPerDay, colwidth, taggingSet);
                // bmp.GetBitmap()
                var filetag = string.Empty;
                if (taggingSet != null)
                {
                    filetag = AutomationUtili.CleanFileName(taggingSet.Name) + ".";
                }

                Logger.Info("Saving the carpet plot for " + person.Name + "...");
                using (
                    var fs =
                        fft.MakeFile<Stream>(
                            "Carpetplot." + householdKey + "." + person.Name + "." + colwidth + "." + filetag +
                            "png", "Carpet plot of all activities by " + person.Name, true,
                            ResultFileID.CarpetPlots, householdKey, TargetDirectory.Charts,
                            _repository.CalcParameters.InternalStepsize,CalcOption.ActionCarpetPlot, null, person.MakePersonInformation(), filetag))
                {
                    using (var image = SKImage.FromBitmap(bmp.GetBitmap())) {
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                            // save the data to a stream
                        {
                            data.SaveTo(fs);
                        }
                    }
                }

                /*var textbmp = AddDirectTexts(bmp.GetBitmap(), descriptionDict, colwidth);
                using (
                    var fs =
                        fft.MakeFile<Stream>("CarpetplotLabeled." + householdKey + "." + person.Name + filetag + ".png",
                            "Carpet plot of all activities by " + person.Name + " with labels", true,
                            ResultFileID.CarpetPlotLabeled, householdKey, TargetDirectory.Charts,
                            _calcParameters.InternalStepsize,CalcOption.ActionCarpetPlot, null,
                            person.MakePersonInformation(), filetag))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(textbmp));
                    encoder.Save(fs);
                }*/
            }
        }

        //[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        //private void MakeLegend([JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
        //                        [JetBrains.Annotations.NotNull] Dictionary<string, Color> affordanceColorDict,
        //                        [JetBrains.Annotations.NotNull] CalcPersonDto person,
        //                        [JetBrains.Annotations.NotNull][ItemNotNull] IEnumerable<ActionEntry> actionsForPerson,
        //                        [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
        //                        int resolutionMultiplier, LegendType legendType,
        //                        [CanBeNull] CalcAffordanceTaggingSetDto taggingSet)
        //{
        //    var tfarr = new Typeface[Fonts.SystemTypefaces.Count];
        //    Fonts.SystemTypefaces.CopyTo(tfarr, 0);

        //    var tf = tfarr.First(x => x.FontFamily.Source == "Arial");
        //    var drawingVisual = new DrawingVisual();
        //    var drawingContext = drawingVisual.RenderOpen();
        //    var legendrow = 2;
        //    var actionsForPersonDict = new Dictionary<string, string>();
        //    var nameColors = new List<Tuple<string, Color>>();
        //    switch (legendType)
        //    {
        //        case LegendType.ActionLegend:
        //            //make a list of all the actions that this person actually did
        //            foreach (var actionEntry in actionsForPerson)
        //            {
        //                if (!actionsForPersonDict.ContainsKey(actionEntry.AffordanceName))
        //                {
        //                    actionsForPersonDict.Add(actionEntry.AffordanceName, string.Empty);
        //                }
        //            }

        //            //look for
        //            foreach (var keyValuePair in actionsForPersonDict)
        //            {
        //                if (affordanceColorDict.ContainsKey(keyValuePair.Key))
        //                {
        //                    var color = affordanceColorDict[keyValuePair.Key];
        //                    var colordescription = ChartLocalizer.Get().GetTranslation(keyValuePair.Key);
        //                    if (color.R == 0 && color.G == 0 && color.B == 0)
        //                    {
        //                        color = Color.FromArgb(0,255, 255, 255);
        //                        colordescription += " (Color: Black!)";
        //                    }

        //                    nameColors.Add(new Tuple<string, Color>(colordescription, color));
        //                }
        //                else
        //                {
        //                    Logger.Error("Action Carpet Plot: No color was found for " + keyValuePair.Key);
        //                }
        //            }

        //            break;
        //        case LegendType.CategoryLegend:
        //            var categories = new Dictionary<string, string>();
        //            if (taggingSet == null)
        //            {
        //                throw new LPGException("Taggingset was null");
        //            }

        //            foreach (var entry in actionsForPerson)
        //            {
        //                var tag = taggingSet.AffordanceToTagDict[entry.AffordanceName];
        //                if (!categories.ContainsKey(tag))
        //                {
        //                    categories.Add(tag, entry.AffordanceName);
        //                }
        //            }

        //            foreach (var keyValuePair in affordanceColorDict)
        //            {
        //                if (actionsForPersonDict.ContainsValue(keyValuePair.Key))
        //                {
        //                    var c = affordanceColorDict[keyValuePair.Key];
        //                    var s = ChartLocalizer.Get().GetTranslation(keyValuePair.Key);
        //                    if (c.R == 0 && c.G == 0 && c.B == 0)
        //                    {
        //                        c = Color.FromArgb(0,255, 255, 255);
        //                        s += " (Color: Black!)";
        //                    }

        //                    nameColors.Add(new Tuple<string, Color>(s, c));
        //                }
        //                else
        //                {
        //                    Logger.Error("Action Carpet Plot: No color was found for " + keyValuePair.Key);
        //                }
        //            }

        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(legendType), legendType, null);
        //    }

        //    nameColors.Sort(Comparison);
        //    System.Windows.Media.Brush white = Brushes.White;
        //    var whitepen = new System.Windows.Media.Pen(white, 0);
        //    foreach (var nameColor in nameColors)
        //    {
        //        System.Windows.Media.Brush b = new SolidColorBrush(nameColor.Item2);
        //        var square = Convert.ToChar(9632).ToString();

        //        var squaretext = new FormattedText(square, new CultureInfo("en-us"), FlowDirection.LeftToRight,
        //            tf, 50, b, 1);
        //        var text = new FormattedText(nameColor.Item1, new CultureInfo("en-us"),
        //            FlowDirection.LeftToRight, tf, 28.0, Brushes.Black, 1);
        //        drawingContext.DrawRectangle(white, whitepen, new Rect(0, legendrow, 1200, text.Height));
        //        drawingContext.DrawText(squaretext, new Point(2, legendrow - 20));
        //        drawingContext.DrawText(text, new Point(40, legendrow));
        //        legendrow += 40;
        //    }

        //    drawingContext.Close();
        //    var bmp = new RenderTargetBitmap(1200 * resolutionMultiplier,
        //        actionsForPersonDict.Count * 40 * resolutionMultiplier, 96 * resolutionMultiplier,
        //        96 * resolutionMultiplier,
        //        PixelFormats.Pbgra32);
        //    bmp.Render(drawingVisual);
        //    using (
        //        var fslegend =
        //            fft.MakeFile<Stream>("CarpetplotLegend." + householdKey + "." + person.Name + ".png",
        //                "Carpet plot legend of all activities by " + person.Name, true,
        //                ResultFileID.CarpetPlotsLegend, householdKey, TargetDirectory.Charts,
        //                _calcParameters.InternalStepsize,CalcOption.ActionCarpetPlot, null,
        //                person.MakePersonInformation()))
        //    {
        //        var encoderLegend = new PngBitmapEncoder();
        //        encoderLegend.Frames.Add(BitmapFrame.Create(bmp));
        //        encoderLegend.Save(fslegend);
        //    }
        //}

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private TimeActionTuple[] MakeTimeArray(bool displayNegativeTime,
                                                [JetBrains.Annotations.NotNull][ItemNotNull] List<ActionEntry> actionsForPerson,
                                                int timesteps)
        {
            var times = new TimeActionTuple[timesteps];
            for (var i = 0; i < times.Length; i++)
            {
                times[i] = new TimeActionTuple();
            }

            if (displayNegativeTime)
            {
                times[0].Dt = _repository.CalcParameters.InternalStartTime;
            }
            else
            {
                times[0].Dt = _repository.CalcParameters.OfficialStartTime;
            }

            for (var i = 1; i < times.Length; i++)
            {
                times[i].Dt = times[i - 1].Dt + _repository.CalcParameters.InternalStepsize;
            }

            var currentaction = actionsForPerson[0];
            var currentactionindex = 0;
            foreach (var time in times)
            {
                if (currentactionindex < actionsForPerson.Count - 1)
                {
                    if (time.Dt > actionsForPerson[currentactionindex + 1].DateTime)
                    {
                        currentactionindex++;
                        currentaction = actionsForPerson[currentactionindex];
                    }
                }
                else
                {
                    currentaction = actionsForPerson[actionsForPerson.Count - 1];
                }

                time.Ae = currentaction;
            }

            return times;
        }

        [JetBrains.Annotations.NotNull]
        private Dictionary<CalcPersonDto, List<ActionEntry>> ReadActivities([JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                                                            [JetBrains.Annotations.NotNull] [ItemNotNull]
                                                                            List<CalcPersonDto> persons)
        {
            List<ActionEntry> actionEntries = _repository.ReadActionEntries(householdKey);
            Dictionary<string, CalcPersonDto> personsByGuid = new Dictionary<string, CalcPersonDto>();
            foreach (CalcPersonDto person in persons)
            {
                personsByGuid.Add(person.Guid.StrVal, person);
            }

            Dictionary<CalcPersonDto, List<ActionEntry>> dict = new Dictionary<CalcPersonDto, List<ActionEntry>>();
            foreach (ActionEntry entry in actionEntries)
            {
                var person = personsByGuid[entry.PersonGuid.StrVal];
                if (!dict.ContainsKey(person))
                {
                    dict.Add(person, new List<ActionEntry>());
                }

                dict[person].Add(entry);
            }

            return dict;
        }
        /*
        private Dictionary<string, Color> GetAffordanceColorDict() {
            var affordanceColorDict = new Dictionary<string, Color>();
                foreach (var affordance in _repository.LoadAffordances().Affordances) {
                    Color col = Color.FromRgb(affordance.ColorR, affordance.ColorG, affordance.ColorB);
                    if (!affordanceColorDict.ContainsKey(affordance.Name)) {
                        affordanceColorDict.Add(affordance.Name, col);
                    }
                    foreach (var subAffordance in affordance.SubAffordance) {
                        if (!affordanceColorDict.ContainsKey(subAffordance.Name)) {
                            affordanceColorDict.Add(subAffordance.Name, col);
                        }
                    }
            }
            return affordanceColorDict;
        }*/

        private void Run([JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft,
                         [JetBrains.Annotations.NotNull] Dictionary<string, SKColor> affordanceColorDict, [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                         [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcAffordanceTaggingSetDto> taggingSets,
                         [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcPersonDto> persons)
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            try {
                Logger.Info("Starting to generate the carpet plots...");
                var displayNegativeTime = _repository.CalcParameters.ShowSettlingPeriodTime;

                var ts = CalculateTime(displayNegativeTime);
                var timesteps = (int)(ts.TotalMinutes / _repository.CalcParameters.InternalStepsize.TotalMinutes);
                var activitiesPerPerson = ReadActivities(householdKey, persons);
                //var fileIdx = 0;
                if (activitiesPerPerson.Count == 0) {
                    throw new LPGException("There were no activities for any person in the household " + householdKey.Key);
                }

                foreach (var personActivities in activitiesPerPerson) {
                    //allEntries.AddRange(personActivities.Value);
                    Logger.Info("Starting to generate the carpet plot for " + personActivities.Key.Name + "...");
                    var times = MakeTimeArray(displayNegativeTime, personActivities.Value, timesteps);
                    //MakeLegend(fft, affordanceColorDict, personActivities.Key, personActivities.Value,
                    //  householdKey, 1, LegendType.ActionLegend, null);
                    MakeCarpet(fft, affordanceColorDict, times, personActivities.Key, ts, householdKey, _columnWidth, null);
                    if (Config.MakePDFCharts) {
                        // make tagging set charts
                        foreach (var calcAffordanceTaggingSet in taggingSets) {
                            if (calcAffordanceTaggingSet.MakeCharts) {
                                MakeCarpet(fft, affordanceColorDict, times, personActivities.Key, ts, householdKey, 7, calcAffordanceTaggingSet);
                            }
                        }
                    }
                }
            }
            finally {

                _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            }

            //if (Config.MakePDFCharts) {
            //   MakeLegend(fft, affordanceColorDict, activitiesPerPerson.Count + 1, "TotalLegend", allEntries,
            //     householdNumber, 3, LegendType.ActionLegend, null);
            //}
        }

        //private enum LegendType
        //{
        //    ActionLegend,
        //    CategoryLegend
        //}

        #region Nested type: TimeActionTuple

        private class TimeActionTuple
        {
            [CanBeNull]
            public ActionEntry Ae { get; set; }

            public DateTime Dt { get; set; }
        }

        #endregion
    }
}
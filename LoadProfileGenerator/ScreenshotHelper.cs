using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Automation;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Helpers;
using Database.Tables;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls;
using LoadProfileGenerator.Controls.Usercontrols;
using LoadProfileGenerator.Presenters.BasicElements;
using OxyPlot.Wpf;
using Image = System.Windows.Controls.Image;

namespace LoadProfileGenerator {
    public class ScreenshotHelper {
        [JetBrains.Annotations.NotNull] private readonly ApplicationPresenter _applicationPresenter;

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly List<string> _labels = new List<string>();
        [JetBrains.Annotations.NotNull] private readonly Simulator _sim;
        [JetBrains.Annotations.NotNull] private readonly TabControl _tabControl;

        [CanBeNull] private string _basisDir;

        [CanBeNull] private string _dstDir;

        [ItemNotNull] [CanBeNull] private List<UIElement> _elements;

        private int _itemCount = 1;

        [CanBeNull] private StackPanel _stackPanel;

        [CanBeNull] private TabItem _tab;

        [CanBeNull] private string _texName;

        [JetBrains.Annotations.NotNull] private string _typeDescRaw = string.Empty;

        public ScreenshotHelper([JetBrains.Annotations.NotNull] Simulator sim, [JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] TabControl tabControl)
        {
            _sim = sim;
            _tabControl = tabControl;
            _applicationPresenter = applicationPresenter;
        }

        public void Run()
        {
            var t = new Thread(() => {
                ScreenshotElement(_sim.LoadTypes.FindFirstByName("Electricity"));
                ScreenshotElement(_sim.Holidays.FindFirstByName("Christmas", FindMode.Partial));
                ScreenshotElement(_sim.GeographicLocations.FindFirstByName("Chemnitz", FindMode.Partial));
                ScreenshotElement(_sim.TemperatureProfiles.Items[0]);
                ScreenshotElement(_sim.DateBasedProfiles.Items[0]);
                ScreenshotElement(_sim.Vacations.Items[0]);
                ScreenshotElement(_sim.Desires.Items[0]);
                ScreenshotElement(_sim.Locations.Items[0]);
                ScreenshotElement(_sim.Persons.Items[0]);
                ScreenshotElement(_sim.DeviceCategories.Items[0]);
                ScreenshotElement(_sim.RealDevices.Items[0]);
                ScreenshotElement(_sim.DeviceActions.Items[0]);
                ScreenshotElement(_sim.DeviceActionGroups.Items[0]);
                ScreenshotElement(_sim.DeviceTaggingSets.Items[0]);
                ScreenshotElement(_sim.Timeprofiles.Items[0]);
                ScreenshotElement(_sim.TimeLimits.FindFirstByName("Time Limit Demo", FindMode.Partial));
                ScreenshotElement(_sim.Variables.Items[0]);
                ScreenshotElement(_sim.Affordances.Items[0]);
                ScreenshotElement(_sim.SubAffordances.Items[0]);
                ScreenshotElement(_sim.AffordanceTaggingSets.Items[0]);
                ScreenshotElement(_sim.TraitTags.Items[0]);
                ScreenshotElement(_sim.HouseholdTraits.Items[0]);
                ScreenshotElement(_sim.HouseholdTemplates.Items[0]);
                ScreenshotElement(_sim.DeviceSelections.Items[0]);
                ScreenshotElement(_sim.ModularHouseholds.Items[0]);
                ScreenshotElement(_sim.EnergyStorages.Items[0]);
                ScreenshotElement(_sim.Generators.Items[0]);
                ScreenshotElement(_sim.HouseTypes.Items[0]);
                ScreenshotElement(_sim.Houses.Items[0]);
                ScreenshotElement(_sim.SettlementTemplates.Items[0]);
                ScreenshotElement(_sim.Settlements.Items[0]);
                ScreenshotElement(_sim.TransformationDevices.Items[0]);
                ScreenshotElement(_sim.HouseholdTags.Items[0]);
                ScreenshotElement(_sim.HouseholdPlans.Items[0]);
                MessageWindowHandler.Mw.ShowInfoMessage("Finished", "Success");
            });
            t.Start();
        }

        public void RunOthers()
        {
            _itemCount = 40;
            var t = new Thread(() => {
                ScreenshotElement(_sim.Categories.First(x => {
                    if (x is OtherCategory oc && oc.Name == "Calculation")
                    {
                        return true;
                    }

                    return false;
                }));
                ScreenshotElement(_sim.Categories.First(x => {
                    if (x is OtherCategory oc && oc.Name == "Settings") {
                        return true;
                    }

                    return false;
                }));
                MessageWindowHandler.Mw.ShowInfoMessage("Finished", "Finished");
            });
            t.Start();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void SnapshotExpanders([CanBeNull] TabItem ti)
        {
            try {
                _labels.Clear();
                Logger.Get().SafeExecuteWithWait(() => {
                    var view = (UserControl) ti.Content;
                    dynamic presenter = view.DataContext;
                    if (TestWithReflection(presenter, "Item")) {
                        DBBase item = presenter.Item;
                        _typeDescRaw = item.TypeDescription;
                    }
                    else {
                        _typeDescRaw = ti.Header.ToString();
                    }

                    var typeDescClean = AutomationUtili.CleanFileName(_typeDescRaw).Replace(" ", string.Empty);
                    _basisDir = _sim.MyGeneralConfig.DestinationPath;
                    _dstDir = Path.Combine(_basisDir,
                        _itemCount.ToString("00", CultureInfo.CurrentCulture) + "-" + typeDescClean);
                    if (!Directory.Exists(_dstDir)) {
                        Directory.CreateDirectory(_dstDir);
                    }

                    var di = new DirectoryInfo(_dstDir);
                    if (di.Parent == null) {
                        throw new LPGException("Di parent was null");
                    }

                    _texName = Path.Combine(di.Parent.FullName,
                        _itemCount.ToString("00", CultureInfo.CurrentCulture) + ".Screenshots." + typeDescClean +
                        ".tex");
                    dynamic dynView = view;
                    ScrollViewer sv = null;
                    if (dynView.Content is Grid maingrid)
                    {
                        foreach (var child in maingrid.Children)
                        {
                            sv = child as ScrollViewer;
                            if (sv != null)
                            {
                                break;
                            }
                        }
                    } else
                    {
                        return; // the view does not contain a grid (e.g. the WelcomeView)
                    }

                    if (sv == null) {
                        throw new LPGException("No scrollviewer");
                    }

                    _stackPanel = (StackPanel) sv.Content;
                });
                var count = 1;
                if (_texName == null) {
                    throw new LPGException("Texname was null");
                }

                if (_basisDir == null) {
                    throw new LPGException("_basisDir was null");
                }

                if (_stackPanel == null)
                {
                    Logger.Error("Skipped the selected view because a contained stackpanel could not be found.");
                    return;
                }

                using (var sw = new StreamWriter(_texName)) {
                    if (_dstDir == null) {
                        throw new LPGException("_dstDir was null");
                    }

                    var overviewName = Path.Combine(_dstDir, "0-overview.png");
                    Logger.Get().SafeExecuteWithWait(() => {
                        WriteTexBlock(sw, overviewName, "Overview", _typeDescRaw, _basisDir);
                        _elements = new List<UIElement>();
                        if (_stackPanel == null) {
                            throw new LPGException("Stackpanel was null");
                        }

                        foreach (var child in _stackPanel.Children) {
                            _elements.Add((UIElement) child);
                        }
                    });
                    // split everything
                    if (_elements == null) {
                        throw new LPGException("_elements was null");
                    }

                    foreach (var uiElement in _elements) {
                        var exp = (Expander) uiElement;
                        Logger.Get().SafeExecuteWithWait(() => exp.IsExpanded = true);
                        Thread.Sleep(100);
                        var count1 = count;
                        Logger.Get().SafeExecuteWithWait(() => {
                            var border = (Border) exp.Header;
                            var textblock = (TextBlock) border.Child;
                            _labels.Clear();
                            FixElements(exp.Content);
                            exp.UpdateLayout();
                            var blockName = textblock.Text;
                            var cleanblockName = AutomationUtili.CleanFileName(blockName).Replace(" ", string.Empty);
                            if (_dstDir == null) {
                                throw new LPGException("_dstDir was null");
                            }

                            var blockFileName = Path.Combine(_dstDir, count1 + "-" + cleanblockName + ".png");
                            if (_basisDir == null) {
                                throw new InvalidOperationException();
                            }

                            WriteTexBlock(sw, blockFileName, blockName, _typeDescRaw, _basisDir);
                            SnapshotPng(exp, blockFileName);
                        });
                        count++;
                    }

                    if (_stackPanel == null) {
                        throw new LPGException("_stackpanel was null");
                    }

                    Logger.Get().SafeExecuteWithWait(() => SnapshotPng(_stackPanel, overviewName));
                    Logger.Info("Successfully screenshotted " + _typeDescRaw + " to " + _dstDir);
                }
            }
            catch (Exception ex) {
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("ReSharper", "TailRecursiveCall")]
        private void FixElements([JetBrains.Annotations.NotNull] object element)
        {
            if (element is Grid g) {
                foreach (var child in g.Children) {
                    FixElements(child);
                }

                return;
            }

            if (element is StackPanel sp) {
                foreach (var child in sp.Children) {
                    FixElements(child);
                }

                return;
            }

            if (element is ScrollViewer sv) {
                FixElements(sv.Content);
                return;
            }

            if (element is Button b) {
                if (b.Content != null) {
                    _labels.Add(b.Content.ToString());
                }

                return;
            }

            if (element is ComboBox cmb) {
                if (cmb.Items.Count > 0 && cmb.SelectedIndex == -1) {
                    cmb.SelectedIndex = 0;
                }

                return;
            }

            if (element is ListView list) {
                list.MaxHeight = 200;
                if (list.Items.Count > 0 && list.SelectedIndex == -1) {
                    list.SelectedIndex = 0;
                }

                return;
            }

            if (element is TextBlock) {
                return;
            }

            if (element is Label label) {
                if (label.Content != null) {
                    _labels.Add(label.Content.ToString());
                }

                return;
            }

            if (element is TextBox) {
                return;
            }

            if (element is PlotView) {
                return;
            }

            if (element is CheckBox chkBox) {
                if (chkBox.Content != null) {
                    _labels.Add(chkBox.Content.ToString());
                }

                return;
            }

            if (element is DeviceCategoryPicker) {
                return;
            }

            if (element is Image) {
                return;
            }

            if (element is PermissionLine) {
                return;
            }

            if (element is Border border) {
                FixElements(border.Child);
                return;
            }

            if (element is DeviceSelectorControl) {
                return;
            }

            if (element is RadioButton radio) {
                if (radio.Content != null) {
                    _labels.Add(radio.Content.ToString());
                }

                return;
            }

            if (element is TreeView tv) {
                if (tv.SelectedValue == null) {
                    tv.SelectItem(tv.Items[0], true);
                }

                return;
            }

            throw new LPGException("Unknown type:" + element.GetType());
        }

        private void ScreenshotElement([CanBeNull] object lt)
        {
            if (lt == null) {
                throw new LPGException("No item was found!");
            }

            Logger.Get().SafeExecuteWithWait(() => {
                _applicationPresenter.OpenItem(lt);
                var idx = _tabControl.SelectedIndex;
                _tab = (TabItem) _tabControl.Items[idx];
            });
            Thread.Sleep(500);
            if (_tab == null) {
                throw new LPGException("Tab was null");
            }

            SnapshotExpanders(_tab);

            // Provisionary fix for the screenshot helper
            // Removing the opened tab in the next line led to the wrong tab being screenshotted,
            // so this part is commented out for now.
            //Logger.Get().SafeExecuteWithWait(() => _tabControl.Items.RemoveAt(0));
            // Sleep for 1s to ensure the screenshot is taken of the correct tab
            Thread.Sleep(1000);
            _itemCount++;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void SnapshotPng([JetBrains.Annotations.NotNull] UIElement source, [JetBrains.Annotations.NotNull] string dstFileName)
        {
            try {
                if (Math.Abs(source.RenderSize.Height) < 0.000001) {
                    return;
                }

                if (Math.Abs(source.RenderSize.Width) < 0.000001) {
                    return;
                }

                double verticalTransform = 1;
                double horizontalTransform = 1;
                if (dstFileName.EndsWith("5-Standby.png", StringComparison.Ordinal)) {
                    verticalTransform = 1.55;
                }

                if (dstFileName.EndsWith("3-Timeswhenacertaindeviceispermittedtorun.png", StringComparison.Ordinal)) {
                    horizontalTransform = 2.1;
                }

                if (dstFileName.EndsWith("16-TimeLimit\\0-overview.png", StringComparison.Ordinal)) {
                    horizontalTransform = 2.1;
                }

                var actualHeight = source.RenderSize.Height;
                var actualWidth = source.RenderSize.Width;
                const double resolutionFactor = 1;
                var renderHeight = actualHeight * resolutionFactor;
                var renderWidth = actualWidth * resolutionFactor;

                var renderTarget = new RenderTargetBitmap((int) renderWidth, (int) renderHeight,
                    96 * resolutionFactor, 96 * resolutionFactor, PixelFormats.Pbgra32);
                var sourceBrush = new VisualBrush(source);

                var drawingVisual = new DrawingVisual();
                var drawingContext = drawingVisual.RenderOpen();

                using (drawingContext) {
                    drawingContext.PushTransform(new ScaleTransform(horizontalTransform, verticalTransform));
                    drawingContext.DrawRectangle(sourceBrush, null,
                        new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
                }

                renderTarget.Render(drawingVisual);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                using (var stream = new FileStream(dstFileName, FileMode.Create, FileAccess.Write)) {
                    encoder.Save(stream);
                }
            }
            catch (Exception e) {
                MessageWindowHandler.Mw.ShowDebugMessage(e);
                Logger.Exception(e);
            }
        }

        private static bool TestWithReflection([JetBrains.Annotations.NotNull] dynamic d, [JetBrains.Annotations.NotNull] string propertyName)
        {
            Type type = d.GetType();

            return type.GetProperties().Any(p => p.Name.Equals(propertyName));
        }

        private void WriteTexBlock([JetBrains.Annotations.NotNull] StreamWriter sw, [JetBrains.Annotations.NotNull] string pngFileName, [JetBrains.Annotations.NotNull] string blockName, [JetBrains.Annotations.NotNull] string typeDesc,
            [JetBrains.Annotations.NotNull] string basisDir)
        {
            sw.WriteLine();
            sw.WriteLine(@"<!--\FloatBarrier-->");
            var cleanBlockName = typeDesc + "_" + blockName;
            cleanBlockName = AutomationUtili.CleanFileName(cleanBlockName).Replace(" ", string.Empty);
            sw.WriteLine("#### " + blockName + " [" + cleanBlockName + "]");
            sw.WriteLine();
            sw.WriteLine("<!--");
            sw.WriteLine(@"\begin{figure}[!ht]");
            sw.WriteLine(@"\centering");
            var relativefilename = "Manual/" + pngFileName.Substring(basisDir.Length).Replace("\\", "/");
            sw.WriteLine(@"\includegraphics[keepaspectratio,width=\textwidth,height=10cm]{" + relativefilename + "}");
            if (blockName.ToLower(CultureInfo.CurrentCulture) == "overview") {
                sw.WriteLine(@"\caption{" + blockName + " of the element '" + typeDesc + "'}");
            }
            else {
                sw.WriteLine(@"\caption{Section '" + blockName + "' of the element '" + typeDesc + "'}");
            }

            var cleanBlockname = AutomationUtili.CleanFileName(blockName).Replace(" ", string.Empty);
            var cleanTypeDesc = AutomationUtili.CleanFileName(typeDesc).Replace(" ", string.Empty);
            sw.WriteLine(@"\label{" + cleanTypeDesc + "_" + cleanBlockname + "}");
            sw.WriteLine(@"\end{figure}");
            sw.WriteLine("-->");
            sw.WriteLine();
            foreach (var label in _labels) {
                sw.WriteLine("- **" + label + "** <!--\\newline--> ");
                sw.WriteLine();
            }
        }
    }
}

#pragma warning disable RCS1093 // Remove file with no code.
/*using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using CommonDataWPF;
using LoadProfileGenerator;

namespace TestingHarness {
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal class Program {
        private static string _currentElementName = string.Empty;
        private static string _lastElementName = string.Empty;
        private static bool _showDebug;

        private static void FindCloseButton(AutomationElement root) {
            Condition conditions = new PropertyCondition(AutomationElement.NameProperty, "close");

            var sub = root.FindAll(TreeScope.Subtree, conditions);
            foreach (AutomationElement element in sub) {
                object objPattern;
                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out objPattern)) {
                    var invPattern = objPattern as InvokePattern;
                    if (_showDebug) {
                        Logger.Info("found invPattern");
                    }
                    if (invPattern == null) {
                        throw new LPGException("Invpattern was null");
                    }

                    invPattern.Invoke();
                }
            }
        }

        private static IntPtr GetWindowHandle(string wName) {
            var hWnd = IntPtr.Zero;
            foreach (var pList in Process.GetProcesses()) {
                if (pList.MainWindowTitle.Contains(wName)) {
                    hWnd = pList.MainWindowHandle;
                }
            }
            return hWnd;
        }

        private static void InteractClickMenu(AutomationElement element, int level, AutomationElement root,
            bool openAll) {
            var builder = new StringBuilder();
            for (var i = 0; i < level; i++) {
                builder.Append(" ");
            }
            var space = builder.ToString();
            var stuff = "Class: " + element.Current.ClassName;
            stuff += " AutomationId: " + element.Current.AutomationId;
            stuff += " Name: " + element.Current.Name;
            if (_showDebug) {
                Logger.Info(space + stuff);
            }
            if (element.Current.ClassName == "MenuItem") {
                var patterns = element.GetSupportedPatterns();
                foreach (var automationPattern in patterns) {
                    if (_showDebug) {
                        Logger.Info(space + automationPattern.ProgrammaticName);
                    }
                }
                object objPattern;
                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out objPattern)) {
                    var expcolPattern = (ExpandCollapsePattern) objPattern;
                    if (expcolPattern.Current.ExpandCollapseState == ExpandCollapseState.Collapsed ||
                        expcolPattern.Current.ExpandCollapseState == ExpandCollapseState.PartiallyExpanded) {
                        expcolPattern.Expand();
                    }
                }
                if (element.Current.Name.StartsWith("Add", StringComparison.Ordinal)) {
                    if (element.TryGetCurrentPattern(InvokePattern.Pattern, out objPattern)) {
                        var invokePattern = (InvokePattern) objPattern;
                        invokePattern.Invoke();
                    }
                }
            }
        }

        private static void InteractOpenAll(AutomationElement element, int level, AutomationElement root,
            bool openAll) {
            var space = string.Empty;
            var builder = new StringBuilder();
            builder.Append(space);
            for (var i = 0; i < level; i++) {
                builder.Append(" ");
            }
            space = builder.ToString();
            var stuff = "Class: " + element.Current.ClassName;
            stuff += " AutomationId: " + element.Current.AutomationId;
            stuff += " Name: " + element.Current.Name;
            if (_showDebug) {
                Logger.Info(space + stuff);
            }

            if (element.Current.ClassName == "TreeViewItem") {
                var expcolPattern =
                    (ExpandCollapsePattern) element.GetCurrentPattern(ExpandCollapsePattern.Pattern);
                if (expcolPattern.Current.ExpandCollapseState == ExpandCollapseState.LeafNode &&
                    (_currentElementName != _lastElementName || openAll)) {
                    object objPattern;
                    if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out objPattern)) {
                        var selection = (SelectionItemPattern) objPattern;

                        selection.Select();
                        FindCloseButton(root);
                        _lastElementName = _currentElementName;
                    }
                }
                if (expcolPattern.Current.ExpandCollapseState != ExpandCollapseState.LeafNode) {
                    _currentElementName = element.Current.Name;
                }
                if (_showDebug) {
                    Logger.Info(space + "found expandcollapsepattern");
                }
                var currentState = expcolPattern.Current.ExpandCollapseState;
                try {
                    if (currentState == ExpandCollapseState.Collapsed ||
                        currentState == ExpandCollapseState.PartiallyExpanded) {
                        expcolPattern.Expand();

                        if (_showDebug) {
                            Logger.Info(space + "activated expandcollapsepattern");
                        }
                    }
                }
                catch (InvalidOperationException) {
                    // The current state of the element is LeafNode.
                    if (_showDebug) {
                        Logger.Info("Unable to expand or collapse the element.");
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args")]
        [STAThread]
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void Main(string[] args) {
            try {
                var original = _showDebug;
                _showDebug = true;
                _showDebug = original;
                Logger.Info(Environment.NewLine+"Begin WPF UIAutomation test run"+ Environment.NewLine);
                Logger.Info("Launching LPG application");
                Logger.Info(Environment.CurrentDirectory);
                var appthread = new Thread(() => {
                    var myApp = new App();
                    myApp.InitializeComponent();
                    myApp.Run();
                });
                appthread.SetApartmentState(ApartmentState.STA);
                appthread.Start();
                Thread.Sleep(1000);
                Logger.Info("Found LPG process");
                Logger.Info(Environment.NewLine+"Getting Desktop");
                var aeDesktop = AutomationElement.RootElement;
                if (aeDesktop == null) {
                    throw new TestingException("Unable to get Desktop");
                }
                Logger.Info("Found Desktop"+ Environment.NewLine);

                Logger.Info(Environment.NewLine+"Looking for LPG main window... ");
                Thread.Sleep(10000);
                var windowHandle = GetWindowHandle("LoadProfileGenerator");
                var aeLPG = AutomationElement.FromHandle(windowHandle);
                if (aeLPG == null) {
                    throw new TestingException("Could not find the LPG window");
                }
                Logger.Info("Found LPG main window");
                ParseTree(aeLPG, 0, aeLPG, InteractClickMenu, false);
                ParseTree(aeLPG, 0, aeLPG, InteractOpenAll, false);
            }
            catch (Exception e) {
                Logger.Info(e.Message);
                Logger.Exception(e);
            }
            Logger.Info(Environment.NewLine+"finished now... ");

            Console.ReadKey();
        }

        private static void ParseTree(AutomationElement element, int level, AutomationElement root,
            Action<AutomationElement, int, AutomationElement, bool> interact, bool openall) {
            interact(element, level, root, openall);
            Condition conditions = new OrCondition(new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                new PropertyCondition(AutomationElement.IsEnabledProperty, false));
            var sub = element.FindAll(TreeScope.Children, conditions);
            foreach (AutomationElement subelement in sub) {
                ParseTree(subelement, level + 1, root, interact, openall);
            }
        }

        [Serializable]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public class TestingException : Exception {
            public TestingException(string name) : base(name) {
            }

            protected TestingException(SerializationInfo si, StreamingContext sc) : base(si, sc) {
            }
        }
    }
}*/
#pragma warning restore RCS1093 // Remove file with no code.
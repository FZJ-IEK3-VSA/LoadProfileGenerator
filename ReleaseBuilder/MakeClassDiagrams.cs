using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ReleaseBuilder {
    [TestFixture]
    public class MakeClassDiagrams {
        private static void Printedges([NotNull] MyClassNode clss, [NotNull] StreamWriter sw) {
            foreach (var subclass in clss.Subclasses) {
                Printedges(subclass, sw);
            }
            if (clss.BaseClassNode != null) {
                var edge = "edge"+ Environment.NewLine;
                edge += "["+ Environment.NewLine;
                edge += "\tsource  " + clss.ID + Environment.NewLine;
                edge += "\ttarget  " + clss.BaseClassNode.ID + Environment.NewLine;
                edge += "\tgraphics"+ Environment.NewLine;
                edge += "\t["+ Environment.NewLine;
                edge += "\t\tfill    \"#000000\""+ Environment.NewLine;
                edge += "\t]"+ Environment.NewLine;
                edge += "]";
                sw.WriteLine(edge);
            }
        }

        private static void Printnodes([NotNull]MyClassNode clss,[NotNull] StreamWriter sw) {
            var node = "node"+ Environment.NewLine;
            node += "["+ Environment.NewLine;
            node += "\tid " + clss.ID + Environment.NewLine;
            node += "\tlabel \"" + clss.MyType.FullName + "\"" + Environment.NewLine;
            node += "\tgraphics" + Environment.NewLine;
            node += "\t["+ Environment.NewLine;
            node += "\t\tx " + clss.ID + Environment.NewLine;
            node += "\t\ty 58.0"+ Environment.NewLine;
            var width = (int) (clss.MyType.Name.Length * 6.75 + 10);
            node += "\t\tw " + width + Environment.NewLine;
            node += "\t\th 35"+ Environment.NewLine;
            node += "\t\tfill \"#FFCC00\""+ Environment.NewLine;
            node += "\t\toutline \"#000000\""+ Environment.NewLine;
            node += "\t]"+ Environment.NewLine;
            node += "\tLabelGraphics"+ Environment.NewLine;
            node += "\t["+ Environment.NewLine;
            node += "\t\ttext    \"" + clss.MyType.Name + "\""+ Environment.NewLine;
            node += "\t\tfontSize    12"+ Environment.NewLine;
            node += "\t\tfontStyle   \"bold\""+ Environment.NewLine;
            node += "\t\tfontName    \"Dialog\""+ Environment.NewLine;
            node += "\t\tanchor  \"c\""+ Environment.NewLine;
            node += "\t]"+ Environment.NewLine;
            node += "]";

            foreach (var subclass in clss.Subclasses) {
                Printnodes(subclass, sw);
            }
            sw.WriteLine(node);
        }

        private static void ProcessAssembly([NotNull] string assemblyFilename, [NotNull] string gmlFileName) {
            using (var sw = new StreamWriter(gmlFileName)) {
                var assembly = Assembly.Load(assemblyFilename);
                // assembly.loa
                var classes = new List<MyClassNode>();
                var id = 0;
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsDefined(typeof(CompilerGeneratedAttribute), false)) {
                        continue;
                    }
                    if (type.Name.Contains("__")) {
                        continue;
                    }
                    if (type.IsNested) {
                        continue;
                    }
                    var mc = new MyClassNode(type, id++);
                    classes.Add(mc);
                }

                var toRemove = new List<MyClassNode>();
                foreach (var myclass in classes) {
                    foreach (var potentialBaseClass in classes) {
                        var potentialtype = potentialBaseClass.MyType;
                        if (myclass.BaseName == potentialtype.FullName) {
                            potentialBaseClass.Subclasses.Add(myclass);
                            myclass.BaseClassNode = potentialBaseClass;
                            toRemove.Add(myclass);
                        }
                    }
                }
                foreach (var myClass in toRemove) {
                    classes.Remove(myClass);
                }

                var header = "Creator \"yFiles\""+ Environment.NewLine;
                header += "Version \"2.12\""+ Environment.NewLine;
                header += "graph"+ Environment.NewLine;
                header += "["+ Environment.NewLine;
                header += "hierarchic 1"+ Environment.NewLine;
                header += "label \"lpg\" "+ Environment.NewLine;
                header += "directed 1";
                var abaseclass = new MyClassNode(typeof(object), -1);
                abaseclass.Subclasses.AddRange(classes);
                sw.WriteLine(header);
                Printnodes(abaseclass, sw);
                Printedges(abaseclass, sw);
                sw.WriteLine("]"+ Environment.NewLine);
            }
        }

        public class MyClassNode {
            public MyClassNode([NotNull]Type myType, int id) {
                MyType = myType;
                ID = id;
                if (myType.BaseType != null) {
                    BaseName = myType.BaseType.Namespace + "." + myType.BaseType.Name;
                }
            }
            public MyClassNode? BaseClassNode { get; set; }
            public string? BaseName { get; }

            public int ID { get; }
            [NotNull]
            public Type MyType { get; }
            [NotNull]
            [ItemNotNull]
            public List<MyClassNode> Subclasses { get; } = new List<MyClassNode>();
        }

        [Category("QuickChart")]
        [Test]
        public void Run() {
            ProcessAssembly("loadprofilegenerator.exe", @"e:\lpg.gml");
            ProcessAssembly("DatabaseIO.dll", @"e:\Db.gml");
            ProcessAssembly("CalcController.dll", @"e:\calcController.gml");
            ProcessAssembly("Calculation.dll", @"e:\calculation.gml");
            ProcessAssembly("ChartCreator.dll", @"e:\ChartCreator.gml");
            ProcessAssembly("SimulationEngine.exe", @"e:\SimEngine.gml");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Automation {
    [AttributeUsage(AttributeTargets.All)]
    public class CommentAttribute : Attribute {
        [NotNull] private string _text;

        public CommentAttribute([NotNull] string text, ListPossibleOptions list= ListPossibleOptions.None)
        {
            _text = text;
            List = list;
        }

        [NotNull]
        public string Text {
            get => _text;

            set => _text = value.Replace("\n","").Replace("\r","");
        }

        [NotNull]
        [ItemNotNull]
        public List<string> TurnIntoComment(int indentDepth)
        {
            List<string> wrapped = WrapText(Text, 60);
            switch (List) {
                case ListPossibleOptions.ListOutputFileDefaults: {
                    wrapped.Add("Possible Options:");
                    foreach (var name in Enum.GetNames(typeof(OutputFileDefault))) {
                        wrapped.Add(name);
                    }

                    break;
                }
                case ListPossibleOptions.None:
                    break;
                case ListPossibleOptions.ListCalcOptions:
                    wrapped.Add("Possible Options:");
                    foreach (var name in Enum.GetNames(typeof(CalcOption)))
                    {
                        wrapped.Add(name);
                    }

                    break;
                case ListPossibleOptions.EnergyIntensityTypes:
                    wrapped.Add("Possible Options:");
                    foreach (var name in Enum.GetNames(typeof(EnergyIntensityType)))
                    {
                        wrapped.Add(name);
                    }
                    break;
                case ListPossibleOptions.LoadTypePriorities:
                    wrapped.Add("Possible Options:");
                    foreach (var name in Enum.GetNames(typeof(LoadTypePriority)))
                    {
                        wrapped.Add(name);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            string indentSpace = "";
            for (int i = 0; i < indentDepth; i++) {
                indentSpace += " ";
            }
            for (int i = 0; i < wrapped.Count; i++) {
                wrapped[i] = indentSpace + "// " + wrapped[i];
            }
            return wrapped;
        }
        public ListPossibleOptions List { get; }

        [NotNull]
        [ItemNotNull]
        public static List<string> WrapText([NotNull] string text, int targetLineLength)
        {
            string[] originalLines = text.Split(new[] { " " },
                StringSplitOptions.None);
            List<string> wrappedLines = new List<string>();
            StringBuilder actualLine = new StringBuilder();
            double actualWidth = 0;
            foreach (var item in originalLines)
            {
                actualLine.Append(item + " ");
                actualWidth += item.Length;

                if (actualWidth > targetLineLength)
                {
                    wrappedLines.Add(actualLine.ToString());
                    actualLine.Clear();
                    actualWidth = 0;
                }
            }
            if (actualLine.Length > 0) {
                wrappedLines.Add(actualLine.ToString());
            }
            return wrappedLines;
        }
    }
}
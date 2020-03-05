using System;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace CalcPostProcessor.GeneralHouseholdSteps {
    public class ColourGenerator
    {
        private int _index;
        [NotNull] private readonly IntensityGenerator _intensityGenerator = new IntensityGenerator();

        [NotNull]
        public string NextColour()
        {
            string colour = string.Format(NextPattern(_index),
                _intensityGenerator.NextIntensity(_index));
            _index++;
            return colour;
        }
        [NotNull]
        public static string NextPattern(int index)
        {
            switch (index % 7)
            {
                case 0: return "{0}0000";
                case 1: return "00{0}00";
                case 2: return "0000{0}";
                case 3: return "{0}{0}00";
                case 4: return "{0}00{0}";
                case 5: return "00{0}{0}";
                case 6: return "{0}{0}{0}";
                default: throw new LPGException("Math error");
            }
        }
        private class IntensityGenerator
        {
            [CanBeNull] private IntensityValueWalker _walker;
            private int _current;

            [NotNull]
            public string NextIntensity(int index)
            {
                if (index == 0)
                {
                    _current = 255;
                }
                else if (index % 7 == 0)
                {
                    if (_walker == null)
                    {
                        _walker = new IntensityValueWalker();
                    }
                    else
                    {
                        _walker.MoveNext();
                    }
                    _current = _walker.Current.Value;
                }
                string currentText = _current.ToString("X");
                if (currentText.Length == 1)
                {
                    currentText = "0" + currentText;
                }

                return currentText;
            }
        }

        private class IntensityValue
        {
            [CanBeNull] private IntensityValue _mChildA;
            [CanBeNull] private IntensityValue _mChildB;

            public IntensityValue([CanBeNull] IntensityValue parent, int value, int level)
            {
                if (level > 7)
                {
                    throw new Exception("There are no more colours left");
                }

                Value = value;
                Parent = parent;
                Level = level;
            }

            public int Level { get; set; }
            public int Value { get; set; }
            [CanBeNull]
            public IntensityValue Parent { get; set; }

            [NotNull]
            public IntensityValue ChildA => _mChildA ?? (_mChildA = new IntensityValue(this, Value - (1 << (7 - Level)), Level + 1));

            [NotNull]
            public IntensityValue ChildB => _mChildB ?? (_mChildB = new IntensityValue(this, Value + (1 << (7 - Level)), Level + 1));
        }

        private class IntensityValueWalker
        {
            public IntensityValueWalker()
            {
                Current = new IntensityValue(null, 1 << 7, 1);
            }

            [NotNull]
            public IntensityValue Current { get; set; }

            public void MoveNext()
            {
                if (Current.Parent == null)
                {
                    Current = Current.ChildA;
                }
                else if (Current.Parent.ChildA == Current)
                {
                    Current = Current.Parent.ChildB;
                }
                else
                {
                    int levelsUp = 1;
                    Current = Current.Parent;
                    while (Current.Parent != null && Current == Current.Parent.ChildB)
                    {
                        Current = Current.Parent;
                        levelsUp++;
                    }
                    if (Current.Parent != null)
                    {
                        Current = Current.Parent.ChildB;
                    }
                    else
                    {
                        levelsUp++;
                    }
                    for (int i = 0; i < levelsUp; i++)
                    {
                        Current = Current.ChildA;
                    }
                }
            }
        }
    }
}
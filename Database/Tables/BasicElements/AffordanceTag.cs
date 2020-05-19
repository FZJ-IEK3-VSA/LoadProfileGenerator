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

using System;
using System.Collections.ObjectModel;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class AffordanceTag : DBBase, IComparable<AffordanceTag> {
        public const string TableName = "tblAffordanceTag";
        private readonly int _taggingSetID;
        private ColorRGB _carpetPlotColor;

        public AffordanceTag([NotNull] string name, int taggingSetID, [NotNull] string connectionString,[CanBeNull] int? pID, ColorRGB color, StrGuid guid)
            : base(name, pID, TableName, connectionString,  guid)
        {
            _taggingSetID = taggingSetID;
            _carpetPlotColor = color;
            TypeDescription = "Affordance Tag";
        }

        [UsedImplicitly]
        public int Blue {
            get => CarpetPlotColor.B;
            set {
                if (value == _carpetPlotColor.B) {
                    return;
                }
                _carpetPlotColor = new ColorRGB(_carpetPlotColor.R, _carpetPlotColor.G, (byte) value);
                ColorChanged(true);
            }
        }
        /*
        [NotNull]
        [UsedImplicitly]
        public Brush CarpetPlotBrush => new SolidColorBrush(_carpetPlotColor);
        */
        public ColorRGB CarpetPlotColor {
            get => _carpetPlotColor;
            set {
                if (_carpetPlotColor == value) {
                    return;
                }
                if ( _carpetPlotColor.R == value.R &&
                    _carpetPlotColor.G == value.G && _carpetPlotColor.B == value.B) {
                    return;
                }
                _carpetPlotColor = value;
                ColorChanged(false);
            }
        }

        [UsedImplicitly]
        public int Green {
            get => CarpetPlotColor.G;
            set {
                if (value == _carpetPlotColor.G) {
                    return;
                }
                _carpetPlotColor.G = (byte)value;
                ColorChanged(true);
            }
        }

        [UsedImplicitly]
        public int Red {
            get => CarpetPlotColor.R;
            set {
                if (value == _carpetPlotColor.R) {
                    return;
                }
                _carpetPlotColor.R = (byte)value;
                ColorChanged(true);
            }
        }

        public int TaggingSetID => _taggingSetID;

        public int CompareTo([CanBeNull] AffordanceTag other)
        {
            if (other == null) {
                return 0;
            }
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        [NotNull]
        private static AffordanceTag AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var tagID = dr.GetIntFromLong("ID");
            var taggingSetID = dr.GetIntFromLong("TaggingSetID", false, ignoreMissingFields, -1);
            var name = dr.GetString("Name");
            var colorint2 = dr.GetIntFromLong("CarpetPlotColor2", false, ignoreMissingFields);
            var colorint3 = dr.GetIntFromLong("CarpetPlotColor3", false, ignoreMissingFields);
            var colorint4 = dr.GetIntFromLong("CarpetPlotColor4", false, ignoreMissingFields);
            var carpetPlotColor = new ColorRGB( (byte) colorint2, (byte) colorint3, (byte) colorint4);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new AffordanceTag(name, taggingSetID,
                connectionString, tagID, carpetPlotColor,guid);
        }

        private void ColorChanged(bool save)
        {
            OnPropertyChanged(nameof(CarpetPlotColor));
            //OnPropertyChanged(nameof(CarpetPlotBrush));
            OnPropertyChanged(nameof(Red));
            OnPropertyChanged(nameof(Green));
            OnPropertyChanged(nameof(Blue));
            if (save) {
                SaveToDB();
            }
        }

        public override int CompareTo(object obj)
        {
            if (!(obj is AffordanceTag other))
            {
                return 0;
            }
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_taggingSetID < 0) {
                message = "Parent Tagging Set was not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<AffordanceTag> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
            cmd.AddParameter("TaggingSetID", TaggingSetID);
            cmd.AddParameter("CarpetPlotColor2", _carpetPlotColor.R);
            cmd.AddParameter("CarpetPlotColor3", _carpetPlotColor.G);
            cmd.AddParameter("CarpetPlotColor4", _carpetPlotColor.B);
        }

        public override string ToString() => Name;
    }
}
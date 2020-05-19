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
using Automation.ResultFiles;
using JetBrains.Annotations;

#endregion

namespace Common {
    [Serializable]
    public class BasicElement : IComparable<BasicElement>, IComparable, IFilterable {
        [NotNull] private string _name;

        public BasicElement([NotNull] string name)
        {
            _name = name;
        }

        [NotNull]
        public virtual string Name {
            get => _name;
            set => _name = value;
        }

        public virtual int CompareTo([CanBeNull] object obj)
        {
            if (!(obj is BasicElement other))
            {
                return 0;
            }
            return string.Compare(_name, other._name, StringComparison.CurrentCultureIgnoreCase);
        }

        #region IComparable<BasicElement> Members

        public virtual int CompareTo([CanBeNull] BasicElement other)
        {
            if (other == null) {
                throw new LPGException("Comparision failed, Other = null");
            }
            return string.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion

        public virtual bool IsValid(string filter)
        {
            if (filter == null) {
                throw new LPGException("isvalid failed, s = null");
            }
            if (_name == null) {
                throw new LPGException("name was null");
            }
            if (_name.ToUpperInvariant().Contains(filter.ToUpperInvariant())) {
                return true;
            }
            return false;
        }

        [NotNull]
        public override string ToString() => Name;
    }
}
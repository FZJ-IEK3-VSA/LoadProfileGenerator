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

using System.Collections.ObjectModel;
using Common;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Views.BasicElements;

#endregion

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class VariablePresenter : PresenterBaseDBBase<VariableView> {
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<UsedIn> _usedIn;

        [JetBrains.Annotations.NotNull] private readonly Variable _variable;

        public VariablePresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] VariableView view, [JetBrains.Annotations.NotNull] Variable variable)
            : base(view, "ThisVariable.HeaderString", variable, applicationPresenter)
        {
            _variable = variable;

            _usedIn = new ObservableCollection<UsedIn>();
            RefreshUsedIn();
        }

        [JetBrains.Annotations.NotNull]
        public Variable ThisVariable => _variable;

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIn => _usedIn;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _variable.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete()
        {
            Sim.Variables.DeleteItem(_variable);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            return obj is VariablePresenter presenter && presenter.ThisVariable.Equals(_variable);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                const int hash = 17;
                // Suitable nullity checks etc, of course :)
                return hash * 23 + TabHeaderPath.GetHashCode();
            }
        }

        public void RefreshUsedIn()
        {
            var usedIn = _variable.CalculateUsedIns(Sim);
            _usedIn.SynchronizeWithList(usedIn);
        }
    }
}
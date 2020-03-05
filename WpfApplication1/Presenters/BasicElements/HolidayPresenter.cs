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

using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Views.BasicElements;

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class HolidayPresenter : PresenterBaseDBBase<HolidayView> {
        [NotNull] private readonly Holiday _thisHoliday;

        public HolidayPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] HolidayView view, [NotNull] Holiday holiday)
            : base(view, "ThisHoliday.HeaderString", holiday, applicationPresenter) => _thisHoliday = holiday;

        [NotNull]
        [UsedImplicitly]
        public Holiday ThisHoliday => _thisHoliday;

        public override void Close(bool saveToDB, bool removeLast = false) {
            if (saveToDB) {
                _thisHoliday.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete() {
            Sim.Holidays.DeleteItem(_thisHoliday);
            Close(false);
        }

        public override bool Equals([CanBeNull] object obj) {
            return obj is HolidayPresenter presenter && presenter.ThisHoliday.Equals(_thisHoliday);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + TabHeaderPath.GetHashCode();
                return hash;
            }
        }
    }
}
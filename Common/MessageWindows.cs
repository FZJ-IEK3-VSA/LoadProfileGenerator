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

namespace Common
{
    public enum LPGMsgBoxResult {
        Yes,
        No
    }
    public interface IMessageWindow {
        LPGMsgBoxResult ShowYesNoMessage([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] string caption);
        void ShowDataIntegrityMessage([JetBrains.Annotations.NotNull] DataIntegrityException exception);
        void ShowInfoMessage([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] string caption);
        void ShowErrorMessage([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] string caption);
        void ShowDebugMessage([JetBrains.Annotations.NotNull] Exception exception);
    }

    public static class MessageWindowHandler {
        public static IMessageWindow Mw { get; set; }

        public static void SetMainWindow([JetBrains.Annotations.NotNull] IMessageWindow mainwindow)
        {
            Mw = mainwindow;
        }
    }
}
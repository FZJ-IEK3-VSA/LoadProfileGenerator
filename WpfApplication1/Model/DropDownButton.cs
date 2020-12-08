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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Model
{
    internal class DropDownButton : ToggleButton
    {
        // *** Dependency Properties ***

        [JetBrains.Annotations.NotNull] public static readonly DependencyProperty DropDownProperty = DependencyProperty.Register("DropDown",
            typeof(ContextMenu), typeof(DropDownButton), new UIPropertyMetadata(null));

        // *** Constructors ***

        public DropDownButton()
        {
            // Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property

            Binding binding = new Binding("DropDown.IsOpen")
            {
                Source = this
            };
            SetBinding(IsCheckedProperty, binding);
        }

        // *** Properties ***
        [CanBeNull]
        [UsedImplicitly]
        public ContextMenu DropDown
        {
            get => (ContextMenu) GetValue(DropDownProperty);
            set => SetValue(DropDownProperty, value);
        }

        // *** Overridden Methods ***

        protected override void OnClick()
        {
            if (DropDown != null)
            {
                // If there is a drop-down assigned to this button, then position and display it

                DropDown.PlacementTarget = this;
                DropDown.Placement = PlacementMode.Top;

                DropDown.IsOpen = true;
            }
        }
    }
}
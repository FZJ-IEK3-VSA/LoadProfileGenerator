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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using JetBrains.Annotations;

#endregion

namespace LoadProfileGenerator.Controls {
    public static class TreeViewHelper {
        /// <summary>
        ///     Expands all children of a TreeView
        /// </summary>
        /// <param name="treeView">The TreeView whose children will be expanded</param>
        public static void ExpandAll([NotNull] this TreeView treeView)
        {
            ExpandSubContainers(treeView);
        }

        /// <summary>
        ///     Searches a TreeView for the provided object and selects it if found
        /// </summary>
        /// <param name="treeView">The TreeView containing the item</param>
        /// <param name="item">The item to search and select</param>
        /// ///
        /// <param name="expand">Expand to this element</param>
        public static void SelectItem([NotNull] this TreeView treeView, [CanBeNull] object item, bool expand)
        {
            if (item == null) {
                return;
            }

            ExpandAndSelectItem(treeView, item, expand);
        }

        /// <summary>
        ///     Finds the provided object in an ItemsControl's children and selects it
        /// </summary>
        /// <param name="parentContainer">The parent container whose children will be searched for the selected item</param>
        /// <param name="itemToSelect">The item to select</param>
        /// ///
        /// <param name="expand">Wether to Expand this element</param>
        /// <returns>True if the item is found and selected, false otherwise</returns>
        private static bool ExpandAndSelectItem([NotNull] ItemsControl parentContainer, [NotNull] object itemToSelect, bool expand)
        {
            // check all items at the current level
            foreach (var item in parentContainer.Items) {
                // if the data item matches the item we want to select, set the corresponding
                // TreeViewItem IsSelected to true
                if (item == itemToSelect && parentContainer.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem currentContainer)
                {
                    currentContainer.IsSelected = true;
                    currentContainer.BringIntoView();
                    currentContainer.Focus();

                    // the item was found
                    return true;
                }
            }

            // if we get to this point, the selected item was not found at the current level, so we must check the children
            foreach (var item in parentContainer.Items) {
                // if children exist
                if (parentContainer.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem currentContainer &&
                    currentContainer.Items.Count > 0) {
                    // keep track of if the TreeViewItem was expanded or not
                    var wasExpanded = currentContainer.IsExpanded;

                    // expand the current TreeViewItem so we can check its child TreeViewItems
                    currentContainer.IsExpanded = true;

                    // if the TreeViewItem child containers have not been generated, we must listen to
                    // the StatusChanged event until they are
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) {
                        // store the event handler in a variable so we can remove it (in the handler itself)
                        void Eh(object sender, EventArgs args)
                        {
                            if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
                                if (!ExpandAndSelectItem(currentContainer, itemToSelect, expand)) {
                                    // The assumption is that code executing in this EventHandler is the result of the parent not
                                    // being expanded since the containers were not generated.
                                    // since the itemToSelect was not found in the children, collapse the parent since it was previously collapsed
                                    currentContainer.IsExpanded = false;
                                }

                                // remove the StatusChanged event handler since we just handled it (we only needed it once)
                                currentContainer.ItemContainerGenerator.StatusChanged -= Eh;
                            }
                        }

                        currentContainer.ItemContainerGenerator.StatusChanged += Eh;
                    }
                    else // otherwise the containers have been generated, so look for item to select in the children
                    {
                        if (!ExpandAndSelectItem(currentContainer, itemToSelect, expand)) {
                            // restore the current TreeViewItem's expanded state
                            currentContainer.IsExpanded = wasExpanded;
                        }
                        else // otherwise the node was found and selected, so return true
                        {
                            if (expand) {
                                currentContainer.IsExpanded = true;
                            }

                            return true;
                        }
                    }
                }
            }

            // no item was found
            return false;
        }

        /// <summary>
        ///     Expands all children of a TreeView or TreeViewItem
        /// </summary>
        /// <param name="parentContainer">The TreeView or TreeViewItem containing the children to expand</param>
        private static void ExpandSubContainers([NotNull] ItemsControl parentContainer)
        {
            foreach (var item in parentContainer.Items) {
                if (parentContainer.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem currentContainer &&
                    currentContainer.Items.Count > 0) {
                    // expand the item
                    currentContainer.IsExpanded = true;

                    // if the item's children are not generated, they must be expanded
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) {
                        // store the event handler in a variable so we can remove it (in the handler itself)
                        void Eh(object sender, EventArgs args)
                        {
                            // once the children have been generated, expand those children's children then remove the event handler
                            if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
                                ExpandSubContainers(currentContainer);
                                currentContainer.ItemContainerGenerator.StatusChanged -= Eh;
                            }
                        }

                        currentContainer.ItemContainerGenerator.StatusChanged += Eh;
                    }
                    else // otherwise the children have already been generated, so we can now expand those children
                    {
                        ExpandSubContainers(currentContainer);
                    }
                }
            }
        }
    }
}
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

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Automation;
using Automation.ResultFiles;

namespace Common {
    public static class Extensions {

        [NotNull]
        public static StrGuid ToStrGuid(this Guid myguid)
        {
            return new StrGuid(myguid);
        }

        [NotNull]
        public static StrGuid ToStrGuid([NotNull] this string myguid)
        {
            return new StrGuid(myguid);
        }
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        //public static string GetCurrentMethod() {
        //    var st = new StackTrace();
        //    var sf = st.GetFrame(1);

        //    return sf.GetMethod().Name;
        //}
        public static void Sort<T>([NotNull][ItemNotNull] this ObservableCollection<T> collection, [NotNull] Comparison<T> comparer)
            where T : IComparable {
            if (collection == null) {
                throw new LPGException("Sorting failed, collection = null");
            }
            var sorted = collection.ToList();
            sorted.Sort(comparer);
            for (var i = 0; i < sorted.Count; i++) {
                collection.Move(collection.IndexOf(sorted[i]), i);
            }
        }

        public static void Sort<T>([NotNull][ItemNotNull] this ObservableCollection<T> collection) where T : IComparable {
            if (collection == null) {
                throw new LPGException("Sorting failed, collection = null");
            }
            var sorted = collection.OrderBy(x => x).ToList();
            for (var i = 0; i < sorted.Count; i++) {
                if (!sorted[i].Equals(collection[i])) {
                    collection.Move(collection.IndexOf(sorted[i]), i);
                }

                /*
                 * if (sorted[i].CompareTo(collection[i]) != 0) {
                    collection.Move(collection.IndexOf(sorted[i]), i);
                }
                 */
            }
        }

        public static void SynchronizeWithList<T>([NotNull][ItemNotNull] this ObservableCollection<T> collection, [NotNull][ItemNotNull] List<T> list)
            where T : IComparable {
            foreach (var item in list) {
                if (!collection.Contains(item)) {
                    collection.Add(item);
                }
            }
            var todelete = new List<T>();
            foreach (var item in collection) {
                if (!list.Contains(item)) {
                    todelete.Add(item);
                }
            }
            foreach (var item in todelete) {
                collection.Remove(item);
            }
            Sort(collection);
        }

        //public static void SynchronizeWithList<T>(this List<T> collection, List<T> list)
        //    where T : IComparable {
        //    foreach (var item in list) {
        //        if (!collection.Contains(item)) {
        //            collection.Add(item);
        //        }
        //    }
        //    var todelete = new List<T>();
        //    foreach (var item in collection) {
        //        if (!list.Contains(item)) {
        //            todelete.Add(item);
        //        }
        //    }
        //    foreach (var item in todelete) {
        //        collection.Remove(item);
        //    }
        //    collection.Sort();
        //}
    }
}
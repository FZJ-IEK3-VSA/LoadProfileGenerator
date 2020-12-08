using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Common {
    [Serializable]
    public class LPGNotImplementedException : Exception {
        public LPGNotImplementedException() {
        }

        public LPGNotImplementedException([JetBrains.Annotations.NotNull] string message) : base(message) {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public LPGNotImplementedException([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] Exception inner) : base(message, inner) {
        }

        protected LPGNotImplementedException([JetBrains.Annotations.NotNull] SerializationInfo si, StreamingContext sc) : base(si, sc) {
        }

        //public static string ErrorLocation() {
        //    var callStack = new StackFrame(1, true);
        //    return " File: " + callStack.GetFileName() + ", Function " + callStack.GetMethod() + ", Line: " +
        //           callStack.GetFileLineNumber();
        //}
    }
}
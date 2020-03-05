using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Automation.ResultFiles {
    [Serializable]
    public class LPGException : Exception {
        [UsedImplicitly]
        public LPGException() {
        }

        public LPGException([NotNull] string message) : base(message) {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public LPGException([NotNull] string message, [NotNull] Exception inner) : base(message, inner) {
        }

        protected LPGException([NotNull] SerializationInfo si, StreamingContext sc) : base(si, sc) {
        }

        [NotNull]
        public static string ErrorLocation() {
            var callStack = new StackFrame(1, true);
            return " File: " + callStack.GetFileName() + ", Function " + callStack.GetMethod() + ", Line: " +
                   callStack.GetFileLineNumber();
        }
    }
    public class LPGCommandlineException : Exception
    {
        [UsedImplicitly]
        public LPGCommandlineException()
        {
        }

        public LPGCommandlineException([NotNull] string message) : base(message)
        {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public LPGCommandlineException([NotNull] string message, [NotNull] Exception inner) : base(message, inner)
        {
        }

        protected LPGCommandlineException([NotNull] SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        [NotNull]
        public static string ErrorLocation()
        {
            var callStack = new StackFrame(1, true);
            return " File: " + callStack.GetFileName() + ", Function " + callStack.GetMethod() + ", Line: " +
                   callStack.GetFileLineNumber();
        }
    }
}
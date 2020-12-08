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

        public LPGException([JetBrains.Annotations.NotNull] string message) : base(message) {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public LPGException([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] Exception inner) : base(message, inner) {
        }

        protected LPGException([JetBrains.Annotations.NotNull] SerializationInfo si, StreamingContext sc) : base(si, sc) {
        }

        [JetBrains.Annotations.NotNull]
        public static string ErrorLocation() {
            var callStack = new StackFrame(1, true);
            return " File: " + callStack.GetFileName() + ", Function " + callStack.GetMethod() + ", Line: " +
                   callStack.GetFileLineNumber();
        }
    }

    public class LPGCancelException : Exception
    {
        [UsedImplicitly]
        public LPGCancelException()
        {
        }

        public LPGCancelException([JetBrains.Annotations.NotNull] string message) : base(message)
        {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public LPGCancelException([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] Exception inner) : base(message, inner)
        {
        }

        protected LPGCancelException([JetBrains.Annotations.NotNull] SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

    }

    public class LPGCommandlineException : Exception
    {
        [UsedImplicitly]
        public LPGCommandlineException()
        {
        }

        public LPGCommandlineException([JetBrains.Annotations.NotNull] string message) : base(message)
        {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public LPGCommandlineException([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] Exception inner) : base(message, inner)
        {
        }

        protected LPGCommandlineException([JetBrains.Annotations.NotNull] SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        [JetBrains.Annotations.NotNull]
        public static string ErrorLocation()
        {
            var callStack = new StackFrame(1, true);
            return " File: " + callStack.GetFileName() + ", Function " + callStack.GetMethod() + ", Line: " +
                   callStack.GetFileLineNumber();
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Automation.ResultFiles {
    // ReSharper disable once InconsistentNaming
    public class LPGPBadParameterException : Exception
    {
        [UsedImplicitly]
        public LPGPBadParameterException()
        {
        }

        public LPGPBadParameterException([JetBrains.Annotations.NotNull] string message) : base(message)
        {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public LPGPBadParameterException([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] Exception inner) : base(message, inner)
        {
        }

        protected LPGPBadParameterException([JetBrains.Annotations.NotNull] SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

    }
}
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

        public LPGPBadParameterException([NotNull] string message) : base(message)
        {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public LPGPBadParameterException([NotNull] string message, [NotNull] Exception inner) : base(message, inner)
        {
        }

        protected LPGPBadParameterException([NotNull] SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

    }
}
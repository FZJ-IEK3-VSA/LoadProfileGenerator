using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace LoadProfileGenerator {
    public class BindingErrorListener : TraceListener {
        [CanBeNull] private Action<string> _logAction;

        public static void Listen([NotNull] Action<string> logAction) {
            PresentationTraceSources.DataBindingSource.Listeners
#pragma warning disable CC0022 // Should dispose object
                .Add(new BindingErrorListener {_logAction = logAction});
#pragma warning restore CC0022 // Should dispose object
        }

        public override void Write([CanBeNull]string message) {
            // needed for interface
        }

        public override void WriteLine([CanBeNull] string message) {
            _logAction?.Invoke(message);
        }
    }
}
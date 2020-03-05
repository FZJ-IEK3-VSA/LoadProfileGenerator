using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using MigraDoc.DocumentObjectModel;

namespace ChartCreator2.PDF {
    internal interface IPageCreator {
        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        void MakePage([NotNull] Document doc, [NotNull] string dstdir, bool requireAll);
    }

    internal interface IPageCreatorToc {
        void MakePage([NotNull] Document doc, [NotNull] string dstdir, bool requireAll, [ItemNotNull] [NotNull] List<string> pngFiles, [NotNull] Section tocSection);
    }
}
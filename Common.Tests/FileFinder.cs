using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;

namespace Common.Tests
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public static class FileFinder
    {
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public static List<FileInfo> GetRecursiveFiles([JetBrains.Annotations.NotNull] DirectoryInfo di, [JetBrains.Annotations.NotNull] string fileName)
        {
            DirectoryInfo[] subdirs = di.GetDirectories();
            List<FileInfo> results = new List<FileInfo>();

            foreach (DirectoryInfo info in subdirs)
            {
                List<FileInfo> subresults = GetRecursiveFiles(info, fileName);
                results.AddRange(subresults);
            }
            FileInfo[] fis = di.GetFiles(fileName);
            results.AddRange(fis);
            return results;
        }
    }
}
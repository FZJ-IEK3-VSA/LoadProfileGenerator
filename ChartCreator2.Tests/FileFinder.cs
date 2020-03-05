using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace ChartCreator2.Tests
{
    public static class FileFinder
    {
        [ItemNotNull]
        [NotNull]
        public static List<FileInfo> GetRecursiveFiles([NotNull] DirectoryInfo di, [NotNull] string fileName)
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
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace VersionIncreaser
{
    public static class Program
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static void Main()
        {
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            var fis = di.GetFiles("Directory.Build.props");
            if (fis.Length != 1) {
                throw new Exception("build.props not found.");
            }
            StreamReader sr = new StreamReader(fis[0].FullName);
            string s = sr.ReadLine();
            while (!s.Contains("AssemblyVersion")) {
                s = sr.ReadLine();
            }
            sr.Close();
            s = s.Replace("<AssemblyVersion>", "").Replace("</AssemblyVersion>", "").Trim();
            var arr = s.Split('.');
            var build = Convert.ToInt32(arr[3]);
            build = build + 1;
            var newversion = arr[0] + "." + arr[1] + "." + arr[2] + "." + build.ToString();
            var fileContent = File.ReadAllText(fis[0].FullName);
            var newFileContent = fileContent.Replace(s, newversion);
            Console.WriteLine(newFileContent);
            File.WriteAllText(fis[0].FullName,newFileContent);
            Console.WriteLine("Increased version from " + s + " to " + newversion);
            var fis2 = di.GetFiles("VersionInfo.cs");
            if (fis2.Length != 1)
            {
                throw new Exception("build.props not found.");
            }
            var fis1 = di.GetFiles("VersionInfo.cs");
            var fileContent1 = File.ReadAllText(fis1[0].FullName);
            var newFileContent1 = fileContent1.Replace(s, newversion);
            File.WriteAllText(fis1[0].FullName, newFileContent1);
            Console.WriteLine(newFileContent1);
        }
    }
}

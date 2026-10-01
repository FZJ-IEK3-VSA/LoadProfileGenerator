namespace VersionIncreaser
{
    public static class Program
    {
        /// <summary>
        /// Increments the build number in the AssemblyVersion and FileVersion tags in the Directory.Build.props file.
        /// </summary>
        /// <exception cref="Exception">if the file is missing or has an unexpected format</exception>
        public static void Main()
        {
            var di = new DirectoryInfo(Environment.CurrentDirectory);
            var buildPropsFiles = di.GetFiles("Directory.Build.props");
            if (buildPropsFiles.Length != 1)
            {
                throw new Exception("File Directory.Build.props not found.");
            }

            // find and parse the current assembly version from the build props file
            var sr = new StreamReader(buildPropsFiles[0].FullName);
            string? s = sr.ReadLine();
            while (s?.Contains("AssemblyVersion") != true)
            {
                s = sr.ReadLine();
            }
            sr.Close();
            s = s.Replace("<AssemblyVersion>", "").Replace("</AssemblyVersion>", "").Trim();
            var numbers = s.Split('.');
            var build = Convert.ToInt32(numbers[3]);

            // increment build number and create new assembly version string
            build = build + 1;
            var newversion = $"{numbers[0]}.{numbers[1]}.{numbers[2]}.{build}";

            // write the new version into the build props file
            var fileContent = File.ReadAllText(buildPropsFiles[0].FullName);
            var newFileContent = fileContent.Replace(s, newversion);
            Console.WriteLine(newFileContent);
            File.WriteAllText(buildPropsFiles[0].FullName, newFileContent);
            Console.WriteLine("Increased version from " + s + " to " + newversion);
        }
    }
}

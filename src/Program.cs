using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nullstats
{
    class Stats
    {
        public string Name { get; set; }
        public int Count { get; set; } = 0;
        public int Enabled => Count - Disabled;
        public int Disabled { get; set; } = 0;
        public List<Stats> Subfolders { get; } = new List<Stats>();
        public override string ToString() => $"{Name}: {(Percentage).ToString("0.00")}%  ({Enabled} of {Count})";
        public void Add(Stats s)
        {
            Count += s.Count;
            Disabled += s.Disabled;
        }
        public double Percentage => Enabled * 100d / Count;
    }
    class Program
    {
        static void Main(string[] args)
        {
            string[] folders =
            {
                @"e:\apps\dotnet\dotnet-api\src\Esri.ArcGISRuntime",
                @"e:\apps\dotnet\dotnet-api\src\Esri.ArcGISRuntime.Hydrography",
                @"e:\apps\dotnet\dotnet-api\src\LocalServer",
                @"e:\apps\dotnet\dotnet-api\src\Esri.ArcGISRuntime.Preview",
            };
            List<Stats> stats = new List<Stats>();
            foreach(var folder in folders)
            {
                var s = new Stats() { Name = folder };
                GetStats(folder, s);
                stats.Add(s);
                PrintResults(s, 0);
            }
        }

        private static void PrintResults(Stats s, int indent)
        {
            if (indent > 0)
            {
                for (int i = 0; i < indent; i++)
                {
                    Console.Write("  ");
                }
                Console.Write("- ");
            }
            if (s.Percentage == 100)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (s.Percentage < 25)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s.ToString());
            Console.ResetColor();
            if (s.Percentage < 100)
            {
                foreach (var sf in s.Subfolders.OrderBy(s=>s.Disabled))
                    PrintResults(sf, indent + 1);
            }
        }

        private static void GetStats(string folder, Stats stats)
        {
            DirectoryInfo di = new DirectoryInfo(folder);
            var files = di.GetFiles("*.cs");
            foreach(var file in files)
            {
                var code = File.ReadAllText(file.FullName);
                int linesOfCode = LinesOfCode(code);
                stats.Count += linesOfCode;
                if (code.Contains("#nullable disable"))
                    stats.Disabled += linesOfCode;
            }
            foreach (var subfolder in di.GetDirectories())
            {
                if (subfolder.Name == "bin" || subfolder.Name == "obj")
                    continue;
                var s = new Stats() { Name = subfolder.Name };
                GetStats(subfolder.FullName, s);
                if (s.Count > 0)
                {
                    stats.Subfolders.Add(s);
                    stats.Add(s);
                }
            }
        }

        private static int LinesOfCode(string code)
        {
            int lines = 0;
            foreach (var line in code.Split('\n').Select(l => l.Trim()))
            {
                if (line.Length == 0 || line.StartsWith("//") || line[0] == '#' || line[0]=='{' || line[0] == '}' || line.StartsWith("using ") ||
                    line.StartsWith("namespace"))
                    continue;
                lines++;
            }
            return lines;
        }
    }
}

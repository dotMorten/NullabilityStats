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
            string[] folders = null;
            var parameters = ParseArguments(args);
            if (parameters.ContainsKey("h") || parameters.ContainsKey("help") || parameters.ContainsKey("?"))
            {
                Console.WriteLine("Parameters:");
                Console.WriteLine("    -d=[n] : Max Folder depth to report stats for.");
                Console.WriteLine("    -f=[folder] : Folder to processs (separate by ; or use multiple -f parameters for several folders). Default is current directory.");
                Console.WriteLine("    -c=[filename] : File name for CSV report");
                return;
            }
            if (parameters.ContainsKey("f"))
            {
                folders = parameters["f"];
            }
            if(folders == null || folders.Length == 0)
            {
                folders = new[] { "." };
            }

            int depth = -1;
            if (parameters.ContainsKey("d"))
            {
                if (int.TryParse(parameters["d"].FirstOrDefault(), out int d))
                    depth = d;
            }
            StreamWriter csvFile = null;
            if(parameters.ContainsKey("c"))
            {
                csvFile = new StreamWriter(File.OpenWrite(parameters["c"].First()));
                csvFile.WriteLine("Name,Fraction,LinesOfCode,LinesEnabled,LinesDisabled");
            }
            List<Stats> stats = new List<Stats>();
            foreach(var folder in folders)
            {
                var s = new Stats() { Name = folder };
                GetStats(folder, s);
                stats.Add(s);
                PrintResults(s, 0, depth);
                
                if (csvFile != null)
                    ExportResults(csvFile, s, new DirectoryInfo(folder).FullName, 0, depth);
            }
            csvFile?.Dispose();
        }

        private static Dictionary<string,string[]> ParseArguments(string[] args)
        {
            Dictionary<string, string[]> result = new Dictionary<string, string[]>();
            for (int i = 0; i < args.Length; i++)
            {
                var parameter = args[i];
                string key = null;
                string value = null;
                if(parameter.StartsWith("--"))
                    key = parameter.Substring(2);
                else if (parameter.StartsWith('-') || parameter.StartsWith('/'))
                    key = parameter.Substring(1);

                var equals = key.IndexOf('=');
                if(equals > 0)
                {
                    value = key.Substring(equals + 1);
                    key = key.Substring(0, equals);
                }
                else
                {
                    if(args.Length > i + 1)
                    {
                        i++;
                        value = args[i];
                    }
                }

                string[] values = null;
                if (value != null)
                {
                    if (value.Contains(';'))
                    {
                        {
                            values = value.Split(';');
                        }
                    }
                    else
                    {
                        values = new string[] { value };
                    }
                }
                if (values != null && result.ContainsKey(key) && result[key] != null)
                {
                    values = result[key].Concat(values).ToArray();
                }
                result[key] = values;
            }
            return result;
        }

        private static void PrintResults(Stats s, int currentDepth, int maxDepth)
        {
            if (maxDepth > -1 && currentDepth > maxDepth)
                return;
            if (currentDepth > 0)
            {
                for (int i = 0; i < currentDepth; i++)
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
                foreach (var sf in s.Subfolders.OrderBy(s => s.Name))
                    PrintResults(sf, currentDepth + 1, maxDepth);
            }
        }
        private static void ExportResults(StreamWriter sw, Stats s, string folderName, int currentDepth, int maxDepth)
        {
            if (maxDepth == currentDepth)
            {
                sw.WriteLine($"\"{folderName}\",{s.Percentage.ToString("0.##")},{s.Count},{s.Enabled},{s.Disabled}");
            }
            else
            {
                var folderCount = s.Count - s.Subfolders.Sum(s => s.Count);
                if (folderCount > 0)
                {
                    var enabled = s.Enabled - s.Subfolders.Sum(s => s.Enabled);
                    var disabled = s.Disabled - s.Subfolders.Sum(s => s.Disabled);
                    var percentage = enabled * 100d / folderCount;
                    sw.WriteLine($"\"{folderName}\",{percentage.ToString("0.##")},{folderCount},{enabled},{disabled}");
                }
                foreach (var sf in s.Subfolders.OrderBy(s => s.Name))
                    ExportResults(sw, sf, $"{folderName}\\{sf.Name}", currentDepth + 1, maxDepth);
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

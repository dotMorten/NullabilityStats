# NullabilityStats
Commandline tool that gives an estimate per-folder how many lines of code hasn't been nullability annotated

Assumes the entire project has Nullability turned on, and counts files where nullability support has been turned off.
Does a best guess estimate on lines of code (ignoring usings, namespace, `{}`, empty lines and comments), and gives stats per-folder to be able to measure progress on adding nullability annotations.

### Install:

```
dotnet tool install dotMorten.NullabilityStats -g
```

### Usage:
 ```
   nullstats <-d depth> <-f folder(s)> <-c csvfile>
      -d: Depth to output stats for (files below are still counted). Skip or set to -1 for no limit
      -f: Folder to processs (separate by ; or use multiple -f parameters for several folders). Default is current directory.
      -c: File name for CSV report


    No parameters: current folder, no limit, no csv file.
   ```